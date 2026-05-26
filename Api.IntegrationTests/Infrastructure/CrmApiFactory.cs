using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests;

public sealed class CrmApiFactory
    : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public CrmApiFactory(string connectionString)
    {
        _connectionString = connectionString;

        Environment.SetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT",
            "Testing");

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            connectionString);

        Environment.SetEnvironmentVariable(
            "Jwt__Key",
            "integration-tests-secret-key-at-least-32-chars");

        Environment.SetEnvironmentVariable(
            "Jwt__Issuer",
            "crm-api");

        Environment.SetEnvironmentVariable(
            "Jwt__Audience",
            "crm-client");

        Environment.SetEnvironmentVariable(
            "Auth__AccessTokenLifetime",
            "00:10:00");

        Environment.SetEnvironmentVariable(
            "Auth__RefreshTokenLifetime",
            "00:30:00");

        Environment.SetEnvironmentVariable(
            "Auth__TokenClockSkew",
            "00:00:30");

        Environment.SetEnvironmentVariable(
            "Seed__SuperAdmin__Email",
            "superadmin@crm.local");

        Environment.SetEnvironmentVariable(
            "Seed__SuperAdmin__Password",
            "SuperAdmin123!");
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }
}