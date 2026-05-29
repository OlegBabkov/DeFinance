using System.Text.Json;
using System.Text.Json.Serialization;
using DeFinance.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace DeFinance.Infrastructure.Services;

public class RedisCacheService(IDistributedCache cache, IConnectionMultiplexer redis) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        Converters = { new JsonStringEnumConverter() },
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var data = await cache.GetStringAsync(key, ct);
        return data is null ? default : JsonSerializer.Deserialize<T>(data, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
        };
        await cache.SetStringAsync(key, json, options, ct);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default) =>
        await cache.RemoveAsync(key, ct);

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var endpoint = redis.GetEndPoints().FirstOrDefault();
        if (endpoint is null) return;
        var server = redis.GetServer(endpoint);
        var keys = server.Keys(pattern: $"{prefix}*").ToArray();
        if (keys.Length > 0)
            await redis.GetDatabase().KeyDeleteAsync(keys);
    }
}
