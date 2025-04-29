using StackExchange.Redis;

namespace Presentation.Dashboard.Infrastructures.Redis.Interfaces;

public interface IRedisService
{
    IDatabase GetDatabase();
}