using Microsoft.Extensions.Options;
using StackExchange.Redis;
using StreamGate.Worker.Configuration;
using StreamGate.Worker.Infrastructure.Redis.Interfaces;

namespace StreamGate.Worker.Infrastructure.Redis.Services;

public class RedisService(IOptions<RedisOption> options) : IRedisService
{ 
    private readonly IConnectionMultiplexer _connection = ConnectionMultiplexer.Connect(options.Value.Configuration);

    public IDatabase GetDatabase() => _connection.GetDatabase();
}