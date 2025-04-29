namespace StreamGate.Worker.Application.Interfaces;

public interface IRedisCacheService
{
    Task SetObject<T>(string key, T objects, TimeSpan? expiry = null);
    Task<T?> GetObject<T>(string key);
    bool DeleteObject(string key);
}