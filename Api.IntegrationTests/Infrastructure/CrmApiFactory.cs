using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests;

public sealed class CrmApiFactory(string connectionString, string redisConnectionString) : WebApplicationFactory<Program>
{
    private readonly string _cacheInstanceName = $"crm-tests:{Guid.NewGuid():N}:";
    private readonly string _fileStorageRootPath = 
        Path.Combine(Path.GetTempPath(), "crm-integration-tests", Guid.NewGuid().ToString("N"));

    public async Task ExecuteDbAsync(Func<AppDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await action(db);
    }

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
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["ConnectionStrings:Redis"] = redisConnectionString,
                ["Cache:InstanceName"] = _cacheInstanceName,
                ["Jwt:Key"] = "integration-tests-secret-key-at-least-32-chars",
                ["Jwt:Issuer"] = "crm-api",
                ["Jwt:Audience"] = "crm-client",
                ["Auth:AccessTokenLifetime"] = "00:10:00",
                ["Auth:RefreshTokenLifetime"] = "00:30:00",
                ["Auth:TokenClockSkew"] = "00:00:30",
                ["PasswordReset:TokenLifetime"] = "00:30:00",
                ["Email:Smtp:Enabled"] = "false",
                ["PasswordReset:FrontendBaseUrl"] = "https://localhost:5173",
                ["Seed:SuperAdmin:Email"] = "superadmin@crm.local",
                ["Seed:SuperAdmin:Password"] = "SuperAdmin123!",
                ["FileStorage:RootPath"] = _fileStorageRootPath,
                ["FileStorage:MaxFileSizeBytes"] = "5242880",
                ["FileStorage:AllowedExtensions:0"] = ".pdf",
                ["FileStorage:AllowedExtensions:1"] = ".png",
                ["FileStorage:AllowedExtensions:2"] = ".jpg",
                ["FileStorage:AllowedExtensions:3"] = ".jpeg",
                ["RateLimiting:Auth:PermitLimit"] = "5",
                ["RateLimiting:Auth:Window"] = "00:05:00",
                ["RateLimiting:Auth:QueueLimit"] = "0",
                ["RateLimiting:UserApi:PermitLimit"] = "60",
                ["RateLimiting:UserApi:Window"] = "00:01:00",
                ["RateLimiting:UserApi:SegmentsPerWindow"] = "6",
                ["RateLimiting:UserApi:QueueLimit"] = "0"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (Directory.Exists(_fileStorageRootPath))
        {
            Directory.Delete(_fileStorageRootPath, recursive: true);
        }
    }
}