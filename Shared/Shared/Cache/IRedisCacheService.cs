namespace Shared.Cache
{
    public interface IRedisCacheService
    {
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key) where T : class;
        Task<string?> GetStringAsync(string key);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<TimeSpan?> GetTTLAsync(string key);
        Task<bool> SetIfNotExistsAsync(string key, string value, TimeSpan? expiry = null);
    }
}