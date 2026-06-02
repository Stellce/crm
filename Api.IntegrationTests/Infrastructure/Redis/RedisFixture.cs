using Testcontainers.Redis;

namespace Api.IntegrationTests;

public class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container = 
        new RedisBuilder("redis:8.8.0-alpine")
            .Build();

    public string ConnectionString => _container.GetConnectionString();

    public Task InitializeAsync()
    {
        return _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}