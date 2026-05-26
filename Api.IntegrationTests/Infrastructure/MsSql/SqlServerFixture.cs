using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace Api.IntegrationTests;

public sealed class SqlServerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = 
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-CU5-ubuntu-24.04")
            .Build();

    private readonly string _databaseName = "CrmIntegrationTests";

    public string ConnectionString 
    {
        get 
        {
            var builder = new SqlConnectionStringBuilder(_container.GetConnectionString())
            {
                InitialCatalog = _databaseName
            };

            return builder.ConnectionString;
        }
    }

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}