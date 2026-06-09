using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using Serilog.Events;
using Application;
using Infrastructure;
using Api.Security;
using Application.Security;
using Api.Exceptions;
using System.Threading.RateLimiting;
using System.Globalization;
using Hangfire;
using Infrastructure.BackgroundJobs;
using Application.Storage;
using Microsoft.Extensions.Options;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting CRM API");


    var builder = WebApplication.CreateBuilder(args);
    var isTesting = builder.Environment.IsEnvironment("Testing");

    var preserveStaticLogger = isTesting;

    builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext(),
        preserveStaticLogger: preserveStaticLogger);

    builder.Services.AddControllers();

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Environment);
    builder.Services.AddJwtAuthentication(builder.Configuration);

    if (!isTesting)
    {
        builder.Services.AddHangfire(configuration =>
        {
            configuration.UseSqlServerStorage(
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new Exception("Missing DefaultConnection"));
        });
        builder.Services.AddHangfireServer();
    }

    builder.Services.AddOptions<FileStorageOptions>()
        .BindConfiguration("FileStorage")
        .Validate(o => !string.IsNullOrWhiteSpace(o.RootPath), "File storage RootPath must not be empty")
        .Validate(o => o.MaxFileSizeBytes > 0, "FileStorage.MaxFileSizeBytes must be greater than zero")
        .Validate(o => o.AllowedExtensions.Count > 0, "FileStorage.AllowedExtensions size must be greater than zero")
        .ValidateOnStart();

    builder.Services.AddOptions<AuthOptions>()
        .BindConfiguration("Auth")
        .Validate(o => o.AccessTokenLifetime > TimeSpan.Zero, "AccessTokenLifetime must be greater than zero")
        .Validate(o => o.RefreshTokenLifetime > o.AccessTokenLifetime, "RefreshTokenLifetime must be greater than AccessTokenLifetime")
        .Validate(o => o.TokenClockSkew >= TimeSpan.Zero, "TokenClockSkew must be non-negative")
        .ValidateOnStart();

    builder.Services.AddOptions<PasswordResetOptions>()
        .BindConfiguration("PasswordReset")
        .Validate(o => !string.IsNullOrWhiteSpace(o.FrontendBaseUrl), "PasswordReset.FrontendBaseUrl must not be empty")
        .Validate(o => o.TokenLifetime > TimeSpan.Zero, "PasswordReset.TokenLifetime must be greater than zero")
        .ValidateOnStart();

    builder.Services.AddAppAuthorization();

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Status = 400,
                Title = "Validation error"
            };

            return new BadRequestObjectResult(problemDetails);
        };
    });

    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Extensions["traceId"] =
                context.HttpContext.TraceIdentifier;

            context.ProblemDetails.Extensions["timestamp"] =
                DateTimeOffset.UtcNow;
        };
    });

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddFluentValidationAutoValidation();

    builder.Services.AddOptions<RateLimitingOptions>()
        .BindConfiguration("RateLimiting")
        .Validate(o => o.Auth.PermitLimit > 0, "RateLimiting.Auth.PermitLimit must be greater than zero")
        .Validate(o => o.Auth.Window > TimeSpan.Zero, "RateLimiting.Auth.Window must be greater than zero")
        .Validate(o => o.Auth.QueueLimit >= 0, "RateLimiting.Auth.QueueLimit must be non-negative")
        .Validate(o => o.UserApi.PermitLimit > 0, "RateLimiting.UserApi.PermitLimit must be greater than zero")
        .Validate(o => o.UserApi.Window > TimeSpan.Zero, "RateLimiting.UserApi.Window must be greater than zero")
        .Validate(o => o.UserApi.SegmentsPerWindow > 0, "RateLimiting.UserApi.SegmentsPerWindow must be greater than zero")
        .Validate(o => o.UserApi.QueueLimit >= 0, "RateLimiting.UserApi.QueueLimit must be non-negative")
        .ValidateOnStart();

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
            }

            await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests",
                Detail = "Rate limiting exceeded. Try again later."
            }, cancellationToken);
        };

        options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
        {
            var rateLimitOptions = httpContext.RequestServices
                .GetRequiredService<IOptions<RateLimitingOptions>>()
                .Value;

            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ip,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitOptions.Auth.PermitLimit,
                    Window = rateLimitOptions.Auth.Window,
                    QueueLimit = rateLimitOptions.Auth.QueueLimit,
                    AutoReplenishment = true
                });
        });

        options.AddPolicy(RateLimitPolicies.UserApi, httpContext =>
        {
            var rateLimitOptions = httpContext.RequestServices
                .GetRequiredService<IOptions<RateLimitingOptions>>()
                .Value;
                
            var userId = httpContext.User.FindFirst("sub")?.Value;
            var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var key = userId is not null
                ? $"user:{userId}"
                : $"ip:{ip}";

            return RateLimitPartition.GetSlidingWindowLimiter(
                partitionKey: key,
                factory: _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = rateLimitOptions.UserApi.PermitLimit,
                    Window = rateLimitOptions.UserApi.Window,
                    SegmentsPerWindow = rateLimitOptions.UserApi.SegmentsPerWindow,
                    QueueLimit = rateLimitOptions.UserApi.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                });
        });
    });

    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer(async (document, context, cancellationToken) =>
        {
            var authSchemeProvider = context.ApplicationServices.GetRequiredService<IAuthenticationSchemeProvider>();

            var schemes = await authSchemeProvider.GetAllSchemesAsync();

            if (!schemes.Any(s => s.Name == JwtBearerDefaults.AuthenticationScheme))
            {
                return;
            }

            document.Components ??= new OpenApiComponents();

            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();

            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter Jwt token without bearer prefix"
            };
        });
        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;

            var hasAuthorize = metadata.OfType<AuthorizeAttribute>().Any();
            var hasAllowAnonymous = metadata.OfType<AllowAnonymousAttribute>().Any();

            if (!hasAuthorize || hasAllowAnonymous)
            {
                return Task.CompletedTask;
            }

            operation.Security ??= [];

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
            });

            return Task.CompletedTask;
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "CRM API v1");
            options.RoutePrefix = "swagger";
        });

        app.UseHangfireDashboard("/hangfire");
    }

    if (!app.Environment.IsEnvironment("Testing"))
    {
        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

        recurringJobManager.AddOrUpdate<AuthTokenCleanupJob>(
            "auth-token-cleanup",
            job => job.RunAsync(),
            Cron.Daily(3),
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = 
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        options.GetLevel = (httpContext, elapsed, exception) =>
        {
            if (exception is not null || httpContext.Response.StatusCode >= 500)
                return LogEventLevel.Error;

            if (httpContext.Response.StatusCode >= 400)
                return LogEventLevel.Warning;

            return LogEventLevel.Information;
        };

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
        };
    });

    app.UseExceptionHandler();

    if(!app.Environment.IsEnvironment("Testing"))
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthentication();
    app.UseRateLimiter();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch(Exception ex) when (ex.GetType().Name != "HostAbortedException")
{
    Log.Fatal(ex, "CRM API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }