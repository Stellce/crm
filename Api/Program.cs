using System.Text;
using Api.Data;
using Api.Entities;
using Api.Exceptions;
using Api.Security;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using Api.Validators;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string not found");

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
        .UseLazyLoadingProxies()   
        .UseSqlServer(connectionString)
        .UseSeeding((context, _) =>
        {
            DatabaseSeeder.Seed((AppDbContext)context, builder.Configuration);
        })
        .UseAsyncSeeding(async (context, _, CancellationToken) =>
        {
            await DatabaseSeeder.SeedAsync((AppDbContext)context, builder.Configuration);
        });
});

builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<JwtService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var authOptions = builder.Configuration.GetSection("Auth").Get<AuthOptions>() 
            ?? throw new InvalidOperationException("Auth options not found");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = authOptions.TokenClockSkew,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
            ),

            
            RoleClaimType = "role",
            NameClaimType = "sub"
        };
        options.MapInboundClaims = false;
    });

builder.Services.AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection("Auth"))
    .Validate(o => o.AccessTokenLifetime > TimeSpan.Zero, "AccessTokenLifetime must be greater than zero")
    .Validate(o => o.RefreshTokenLifetime > TimeSpan.Zero, "RefreshTokenLifetime must be greater than AccessTokenLifetime")
    .Validate(o => o.TokenClockSkew >= TimeSpan.Zero, "TokenClockSkew must be non-negative")
    .ValidateOnStart();

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

builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<PatchCustomerRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();

app.MapControllers();

app.Run();