using Api.IntegrationTests.Helpers;

namespace Api.IntegrationTests;

public abstract class IntegrationTestBase(SqlServerFixture sqlServer) : IAsyncLifetime
{
    private readonly CrmApiFactory _factory = new(sqlServer.ConnectionString);

    protected HttpClient Client { get; private set; } = null!;
    protected CrmTestClient Api { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        
        Client = _factory.CreateClient();
        Api = new CrmTestClient(Client);
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        _factory.Dispose();

        return Task.CompletedTask;
    }
}