using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Services.Redis;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IDistributedCache cache,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Gets a cached value by key
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// Sets a value in the cache
    /// </summary>
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedData = JsonSerializer.Serialize(value, _jsonOptions);

            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1); 
            }

            await _cache.SetStringAsync(key, serializedData, options, cancellationToken);

            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}",
                key, options.AbsoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached value for key: {Key}", key);
        }
    }

    /// <summary>
    /// Removes a value from the cache
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Removed cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
        }
    }

    /// <summary>
    /// Checks if a key exists in the cache
    /// </summary>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence for key: {Key}", key);
            return false;
        }
    }
}
