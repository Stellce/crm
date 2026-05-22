using System.Diagnostics;
using System.Text;
using Application.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Security;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration.GetSection("Jwt");
                var authOptions = configuration.GetSection("Auth").Get<AuthOptions>()
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

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";

                        var problemDetails = new ProblemDetails
                        {
                            Status = StatusCodes.Status401Unauthorized,
                            Title = "Unauthorized",
                            Detail = context.AuthenticateFailure is SecurityTokenExpiredException
                                ? "Access token has expired"
                                : "Invalid or missing access token",
                            Type = "https://httpstatuses.com/401"
                        };

                        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
                        problemDetails.Extensions["timestamp"] = DateTimeOffset.UtcNow;

                        await context.Response.WriteAsJsonAsync(problemDetails);
                    }
                };
            });

        return services;
    }
}