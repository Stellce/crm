using System.Text.Json;
using Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace Infrastructure.Caching;

public sealed class DistributedAppCache(IDistributedCache cache) : IAppCache
{
    private static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var json = await cache.GetStringAsync(key, cancellationToken);

        return json is null
            ? default
            : JsonSerializer.Deserialize<T>(json, jsonOptions);
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, jsonOptions);

        await cache.SetStringAsync(
            key,
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            },
            cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await cache.RemoveAsync(key, cancellationToken);
    }
}