
using Domain.Entities;
using Application.Abstractions;
using Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Data;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found");

        services.AddDbContext<AppDbContext>(options =>
        {
            options
                .UseLazyLoadingProxies()
                .UseSqlServer(connectionString)
                .UseSeeding((context, _) =>
                {
                    DatabaseSeeder.Seed((AppDbContext)context, configuration);
                })
                .UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    await DatabaseSeeder.SeedAsync((AppDbContext) context, configuration, cancellationToken);
                });
        });

        services.AddScoped<IAppDbContext>(sp => 
            sp.GetRequiredService<AppDbContext>());

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}