using System.Text.Json;
using StackExchange.Redis;
using StreamGate.Worker.Application.Interfaces;
using StreamGate.Worker.Infrastructure.Redis.Interfaces;

namespace StreamGate.Worker.Application.Services;

public class RedisCacheService(IRedisService redis) : IRedisCacheService
{
    private readonly IDatabase _con = redis.GetDatabase();

    public async Task SetObject<T>(string key, T objects, TimeSpan? expiry = null)
    {
        var jsonData = JsonSerializer.Serialize(objects);

        await _con.StringSetAsync(key, jsonData, expiry);
    }

    public async Task<T?> GetObject<T>(string key)
    { 
        var jsonData = await _con.StringGetAsync(key);

        return jsonData.HasValue ? JsonSerializer.Deserialize<T>(jsonData!) : default;
    }

    public bool DeleteObject(string key)
    {
        return _con.KeyDelete(key);
    }
}