namespace Payment.Api.Repositories.Base;

public interface IRedisCache
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<bool> DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<bool> RefreshAsync(string key, TimeSpan expiration);
}
