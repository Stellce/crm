using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests;

public sealed class CrmApiFactory(string connectionString, string redisConnectionString)
        : WebApplicationFactory<Program>
{
    private readonly string _connectionString = connectionString;
    private readonly string _redisConnectionString = redisConnectionString;
    private readonly string _cacheInstanceName = $"crm-tests:{Guid.NewGuid():N}:";

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["ConnectionStrings:Redis"] = _redisConnectionString,
                ["Cache:InstanceName"] = _cacheInstanceName,
                ["Jwt:Key"] = "integration-tests-secret-key-at-least-32-chars",
                ["Jwt:Issuer"] = "crm-api",
                ["Jwt:Audience"] = "crm-client",
                ["Auth:AccessTokenLifetime"] = "00:10:00",
                ["Auth:RefreshTokenLifetime"] = "00:30:00",
                ["Auth:TokenClockSkew"] = "00:00:30",
                ["PasswordReset:TokenLifetime"] = "00:30:00",
                ["Email:smtp:Enabled"] = "false",
                ["PasswordReset:FrontendBaseUrl"] = "https://localhost:5173",
                ["Seed:SuperAdmin:Email"] = "superadmin@crm.local",
                ["Seed:SuperAdmin:Password"] = "SuperAdmin123!",
            });
        });
    }
}