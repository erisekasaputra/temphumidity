using Microsoft.Extensions.Options;
using Presentation.Dashboard.Configurations;
using Presentation.Dashboard.Infrastructures.Redis.Interfaces;
using StackExchange.Redis;

namespace Presentation.Dashboard.Infrastructures.Redis.Services;

public class RedisService(IOptions<RedisOption> options) : IRedisService
{ 
    private readonly IConnectionMultiplexer _connection = ConnectionMultiplexer.Connect(options.Value.Configuration);

    public IDatabase GetDatabase() => _connection.GetDatabase();
}