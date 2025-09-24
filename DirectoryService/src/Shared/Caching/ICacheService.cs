﻿using Microsoft.Extensions.Caching.Distributed;

namespace Shared.Caching;

public interface ICacheService
{
    Task<T?> GetOrSetAsync<T>(
        string key,
        DistributedCacheEntryOptions options,
        Func<Task<T?>> factory,
        CancellationToken cancellationToken = default)
        where T : class;

    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        where T : class;

    Task SetAsync<T>(
        string key,
        T value,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken)
        where T : class;

    Task RemoveAsync(string key, CancellationToken cancellationToken);

    Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default);
}