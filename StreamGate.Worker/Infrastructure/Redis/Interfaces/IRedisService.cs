using StackExchange.Redis;

namespace StreamGate.Worker.Infrastructure.Redis.Interfaces;

public interface IRedisService
{
    IDatabase GetDatabase();
}