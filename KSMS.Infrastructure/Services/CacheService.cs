using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSMS.Application.Services;
using Microsoft.Extensions.Caching.Memory;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<bool> LockAsync(string key, TimeSpan expiration)
    {
        if (!_cache.TryGetValue(key, out _))
        {
            _cache.Set(key, true, expiration);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task UnlockAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
    
