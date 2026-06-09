using Api.IntegrationTests.Helpers;
using Infrastructure.Data;

namespace Api.IntegrationTests;

public abstract class IntegrationTestBase(
    SqlServerFixture sqlServer,
    RedisFixture redis) : IAsyncLifetime
{
    private readonly CrmApiFactory _factory = new(
        sqlServer.ConnectionString,
        redis.ConnectionString);

    protected HttpClient Client { get; private set; } = null!;
    protected CrmTestClient Api { get; private set; } = null!;
    protected Task ExecuteDbAsync(Func<AppDbContext, Task> action) => _factory.ExecuteDbAsync(action);

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