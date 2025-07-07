namespace bookwooks.orderapi.infrastructure.caching;
public class DistributedCacheService : ICacheService
{
  private readonly IDistributedCache _cache;
  private readonly ILogger<DistributedCacheService> _logger;
  private readonly ISerializerService _serializer;

  public DistributedCacheService(IDistributedCache cache, ISerializerService serializer, ILogger<DistributedCacheService> logger) =>
      (_cache, _serializer, _logger) = (cache, serializer, logger);

  public T? Get<T>(string key)
  {
    return TryExecute(() =>
    {
      var data = _cache.Get(key);
      return data is not null ? Deserialize<T>(data) : default;
    }, "Get", key);
  }

  public Task<T?> GetAsync<T>(string key, CancellationToken token = default)
  {
     return TryExecuteAsync(async () =>
    {
        var data = await _cache.GetAsync(key, token);
        return data is not null ? Deserialize<T>(data) : default;
    }, "GetAsync", key);
  }

  public void Set<T>(string key, T value, TimeSpan? slidingExpiration = null)
  {
    TryExecute(() =>
    {
      var data = Serialize(value);
      _cache.Set(key, data, GetOptions(slidingExpiration));
      _logger.LogDebug("Added to Cache: {Key}", key);
    }, "Set", key);
  }

  public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken token = default)
  {
    return TryExecuteAsync(async () =>
    {
      var data = Serialize(value);
      await _cache.SetAsync(key, data, GetOptions(slidingExpiration), token);
      _logger.LogDebug("Added to Cache: {Key}", key);
    }, "SetAsync", key);
  }

  public void Remove(string key)
  {
    TryExecute(() => _cache.Remove(key), "Remove", key);
  }

  public Task RemoveAsync(string key, CancellationToken token = default)
  {
    return TryExecuteAsync(() => _cache.RemoveAsync(key, token), "RemoveAsync", key);
  }

  public void Refresh(string key)
  {
    TryExecute(() => _cache.Refresh(key), "Refresh", key);
  }

  public Task RefreshAsync(string key, CancellationToken token = default)
  {
    return TryExecuteAsync(async () =>
    {
      await _cache.RefreshAsync(key, token);
      _logger.LogDebug("Cache Refreshed: {Key}", key);
    }, "RefreshAsync", key);
  }

  private static DistributedCacheEntryOptions GetOptions(TimeSpan? slidingExpiration) =>
      new DistributedCacheEntryOptions
      {
        SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(10)
      };

  private byte[] Serialize<T>(T item) =>
      Encoding.Default.GetBytes(_serializer.Serialize(item));

  private T Deserialize<T>(byte[] data) =>
      _serializer.Deserialize<T>(Encoding.Default.GetString(data));

  private void TryExecute(Action action, string operation, string key)
  {
    try
    {
      action();
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Cache {Operation} failed for key: {Key}", operation, key);
    }
  }

  private T? TryExecute<T>(Func<T?> func, string operation, string key)
  {
    try
    {
      return func();
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Cache {Operation} failed for key: {Key}", operation, key);
      return default;
    }
  }

  private async Task TryExecuteAsync(Func<Task> func, string operation, string key)
  {
    try
    {
      await func();
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Async cache {Operation} failed for key: {Key}", operation, key);
    }
  }
  private async Task<T?> TryExecuteAsync<T>(Func<Task<T?>> func, string operation, string key)
  {
    try
    {
      return await func();
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Async cache {Operation} failed for key: {Key}", operation, key);
      return default;
    }
  }
}
