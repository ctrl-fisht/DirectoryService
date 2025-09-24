using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Shared.Core.Caching;

public class CacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, bool> _cacheKeys = new();
    private readonly IDistributedCache _cache;
    
    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }
    
    public async Task<T?> GetOrSetAsync<T>(
        string key,
        DistributedCacheEntryOptions options,
        Func<Task<T?>> factory,
        CancellationToken cancellationToken = default) where T : class
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        if (cachedValue is not null)
            return cachedValue;

        var newValue = await factory();
        if (newValue is not null)
            await SetAsync(key, newValue, options, cancellationToken);
        
        return newValue;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken) where T : class
    {
        string? cachedValue = await _cache.GetStringAsync(key, cancellationToken);

        if (cachedValue is null)
            return null;

        return JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken) where T : class
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
        _cacheKeys.TryAdd(key, true);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(key, cancellationToken);
        _cacheKeys.TryRemove(key, out bool _);
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        var tasks = _cacheKeys
            .Keys
            .Where(k => k.StartsWith(prefixKey))
            .Select(k => RemoveAsync(k, cancellationToken));

        await Task.WhenAll(tasks);
    }
}