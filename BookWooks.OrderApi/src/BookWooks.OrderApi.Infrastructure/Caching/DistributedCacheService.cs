namespace BookWooks.OrderApi.Infrastructure.Caching;

public class DistributedCacheService : ICacheService
{
  private readonly IDistributedCache _cache;
  private readonly ISerializerService _serializer;
  private readonly ILogger<DistributedCacheService> _logger;

  private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(10);

  public DistributedCacheService(
      IDistributedCache cache,
      ISerializerService serializer,
      ILogger<DistributedCacheService> logger)
      => (_cache, _serializer, _logger) = (cache, serializer, logger);

  public T? Get<T>(string key) =>
      ExecuteSafe(() =>
      {
        var data = _cache.Get(key);
        return data is not null ? Deserialize<T>(data) : default;
      }, nameof(Get), key);

  public bool TryGet<T>(string key, out T? value)
  {
    value = Get<T>(key);
    return value is not null;
  }

  public Task<T?> GetAsync<T>(string key, CancellationToken token = default) =>
      ExecuteSafeAsync(async () =>
      {
        var data = await _cache.GetAsync(key, token);
        return data is not null ? Deserialize<T>(data) : default;
      }, nameof(GetAsync), key);

  public void Set<T>(string key, T value, TimeSpan? slidingExpiration = null) =>
      ExecuteSafe(() =>
      {
        var bytes = Serialize(value);
        _cache.Set(key, bytes, GetOptions(slidingExpiration));
        LogDebug("Set", key, typeof(T), slidingExpiration);
      }, nameof(Set), key);

  public Task SetAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, CancellationToken token = default) =>
      ExecuteSafeAsync(async () =>
      {
        var bytes = Serialize(value);
        await _cache.SetAsync(key, bytes, GetOptions(slidingExpiration), token);
        LogDebug("SetAsync", key, typeof(T), slidingExpiration);
      }, nameof(SetAsync), key);

  public void Remove(string key) =>
      ExecuteSafe(() => _cache.Remove(key), nameof(Remove), key);

  public Task RemoveAsync(string key, CancellationToken token = default) =>
      ExecuteSafeAsync(() => _cache.RemoveAsync(key, token), nameof(RemoveAsync), key);

  public void Refresh(string key) =>
      ExecuteSafe(() => _cache.Refresh(key), nameof(Refresh), key);

  public Task RefreshAsync(string key, CancellationToken token = default) =>
      ExecuteSafeAsync(async () =>
      {
        await _cache.RefreshAsync(key, token);
        _logger.LogDebug("Cache Refreshed: {Key}", key);
      }, nameof(RefreshAsync), key);

  // ----- Helpers -----

  private static DistributedCacheEntryOptions GetOptions(TimeSpan? slidingExpiration) =>
      new() { SlidingExpiration = slidingExpiration ?? DefaultExpiration };

  private byte[] Serialize<T>(T item) => Encoding.UTF8.GetBytes(_serializer.Serialize(item));

  private T Deserialize<T>(byte[] data) => _serializer.Deserialize<T>(Encoding.UTF8.GetString(data));

  private void LogDebug(string operation, string key, Type type, TimeSpan? expiration) =>
      _logger.LogDebug("Cache {Operation}: {Key}, Type: {Type}, Expiration: {Expiration}",
          operation, key, type.Name, expiration ?? DefaultExpiration);

  private bool IsValidKey(string key)
  {
    if (string.IsNullOrWhiteSpace(key))
    {
      _logger.LogWarning("Cache operation called with null or empty key");
      return false;
    }
    return true;
  }

  private void ExecuteSafe(Action action, string operation, string key)
  {
    if (!IsValidKey(key)) return;
    try { action(); }
    catch (Exception ex) { HandleError(ex, operation, key); }
  }

  private T? ExecuteSafe<T>(Func<T?> func, string operation, string key)
  {
    if (!IsValidKey(key)) return default;
    try { return func(); }
    catch (Exception ex) { return HandleError<T>(ex, operation, key); }
  }

  private async Task ExecuteSafeAsync(Func<Task> func, string operation, string key)
  {
    if (!IsValidKey(key)) return;
    try { await func(); }
    catch (Exception ex) { HandleError(ex, operation, key); }
  }

  private async Task<T?> ExecuteSafeAsync<T>(Func<Task<T?>> func, string operation, string key)
  {
    if (!IsValidKey(key)) return default;
    try { return await func(); }
    catch (Exception ex) { return HandleError<T>(ex, operation, key); }
  }

  private void HandleError(Exception ex, string operation, string key)
  {
    var level = ex is ArgumentException or InvalidOperationException or IOException
        ? LogLevel.Warning : LogLevel.Error;
    _logger.Log(level, ex, "Cache {Operation} failed for key: {Key}", operation, key);
  }

  private T? HandleError<T>(Exception ex, string operation, string key)
  {
    HandleError(ex, operation, key);
    return default;
  }
}
