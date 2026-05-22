using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Application;
using Infrastructure;
using Api.Security;
using Application.Security;
using Api.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);


builder.Services.AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection("Auth"))
    .Validate(o => o.AccessTokenLifetime > TimeSpan.Zero, "AccessTokenLifetime must be greater than zero")
    .Validate(o => o.RefreshTokenLifetime > TimeSpan.Zero, "RefreshTokenLifetime must be greater than AccessTokenLifetime")
    .Validate(o => o.TokenClockSkew >= TimeSpan.Zero, "TokenClockSkew must be non-negative")
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
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();