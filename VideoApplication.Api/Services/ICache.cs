using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using VideoApplication.Api.Extensions;

namespace VideoApplication.Api.Services;

public interface ICache<T>
{
    Task<T> Get(string key, TimeSpan duration, Func<Task<T>> loadPredicate,
        CancellationToken cancellationToken = default);

    Task Clear(string key, CancellationToken cancellationToken = default);
}

public abstract class DistributedCache : IDisposable
{
    private readonly IDistributedCache _internalCache;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly ConcurrentDictionary<string, Task<byte[]>> _loadLocks = new();

    protected DistributedCache(IDistributedCache internalCache, ILogger logger)
    {
        _internalCache = internalCache;
        _logger = logger;
    }

    protected async Task<byte[]> Get(string key, TimeSpan duration, Func<Task<byte[]>> loadPredicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(loadPredicate);
        ArgumentNullException.ThrowIfNull(duration);
        
        var existingResult = await _internalCache.GetAsync(key, cancellationToken);
        if (existingResult != null)
        {
            return existingResult;
        }

        Task<byte[]> cachedTask;
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_loadLocks.TryGetValue(key, out var task))
            {
                _logger.LogDebug("Entry task was already in cache: {key}", key);
                cachedTask = task;
            }
            else
            {
                _logger.LogDebug("Entry task was not in cache: {key}", key);
                cachedTask = loadPredicate().AndThen(async data =>
                {
                    _logger.LogDebug("Loaded new data for cache: {key}", key);

                    await _internalCache.SetAsync(key, data, new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = duration
                    }, CancellationToken.None);
                    // Remove this pending task from the load list, so we don't leak memory
                    _loadLocks.TryRemove(key, out _);
                }).OnError(async () =>
                {
                    await _internalCache.RemoveAsync(key, CancellationToken.None);
                    _loadLocks.TryRemove(key, out _);
                });
                _loadLocks.TryAdd(key, cachedTask);
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return await cachedTask;
    }

    protected async Task InternalClear(string key, CancellationToken cancellationToken)
    {
        await _internalCache.RemoveAsync(key, cancellationToken);
        _loadLocks.TryRemove(key, out _);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _semaphore.Dispose();
            }

            _disposed = true;
        }
    }
}

public class DistributedCache<T> : DistributedCache, ICache<T>
{
    private readonly string _keyPrefix;
    
    public DistributedCache(IDistributedCache cache, ILogger<DistributedCache<T>> logger) : base(cache, logger)
    {

        _keyPrefix = typeof(T).GetPrettyTypeName();
    }

    public async Task<T> Get(string key, TimeSpan duration, Func<Task<T>> loadPredicate,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("No proper cache was provided", nameof(key));
        }

        key = GetCacheKey(key);

        var content = await base.Get(key, duration, cancellationToken: cancellationToken, loadPredicate: async () =>
        {
            var result = await loadPredicate();
            return Serialize(result);
        });

        return Parse(content);
    }

    private static byte[] Serialize(T item)
    {
        return JsonSerializer.SerializeToUtf8Bytes(item);
    }

    private static T Parse(byte[] content)
    {
        return JsonSerializer.Deserialize<T>(content)!;
    }

    public async Task Clear(string key, CancellationToken cancellationToken = default)
    {
        key = GetCacheKey(key);
        await InternalClear(key, cancellationToken);
    }


    private string GetCacheKey(string key)
    {
        return $"{_keyPrefix}-{key}";
    }
}