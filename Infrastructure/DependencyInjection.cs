
using Domain.Entities;
using Application.Abstractions;
using Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.BackgroundJobs;
using Infrastructure.Caching;
using Microsoft.Extensions.Hosting;
using Infrastructure.FileStorage;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IHostEnvironment environment)
    {
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var currentConfiguration = sp.GetRequiredService<IConfiguration>();
            var connectionString = currentConfiguration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found");
            options
                .UseLazyLoadingProxies()
                .UseSqlServer(connectionString)
                .UseSeeding((context, _) =>
                {
                    DatabaseSeeder.Seed((AppDbContext)context, currentConfiguration);
                })
                .UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    await DatabaseSeeder.SeedAsync((AppDbContext) context, currentConfiguration, cancellationToken);
                });
        });

        if(environment.IsDevelopment())
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(_ => { });

            services.AddOptions<RedisCacheOptions>()
                .Configure<IConfiguration>((options, currentConfiguration) => 
                {
                    var redisConnectionString = currentConfiguration.GetConnectionString("Redis");

                    if(string.IsNullOrWhiteSpace(redisConnectionString))
                    {
                        throw new InvalidOperationException(
                            "Redis connection string is required outside Development environment.");
                    }

                    options.Configuration = redisConnectionString;
                    options.InstanceName = currentConfiguration["Cache:InstanceName"] ?? "crm:";
                });
        }

        services.AddOptions<SmtpEmailOptions>()
            .BindConfiguration("Email:Smtp")
            .Validate(o => !o.Enabled || !string.IsNullOrWhiteSpace(o.Host), 
                "Email:Smtp:Host must not be empty when SMTP is enabled")
            .Validate(o => !o.Enabled || o.Port > 0, 
                "Email:Smtp:Port must be greater than 0 when SMTP is enabled")
            .Validate(o => !o.Enabled || !string.IsNullOrWhiteSpace(o.From), 
                "Email:Smtp:From must not be empty when SMTP is enabled")
            .Validate(o => 
                string.IsNullOrWhiteSpace(o.User) == string.IsNullOrWhiteSpace(o.Password),
                "Email:Smtp:User and Email:Smtp:Password must be both set or both empty")
            .ValidateOnStart();

        services.AddScoped<IAppDbContext>(sp => 
            sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<AuthTokenCleanupJob>();
        services.AddScoped<IAppCache, DistributedAppCache>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<IReportsReadDb, ReportsReadDb>();

        return services;
    }
}