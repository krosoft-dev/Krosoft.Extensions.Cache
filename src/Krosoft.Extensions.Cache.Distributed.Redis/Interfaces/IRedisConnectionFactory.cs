using StackExchange.Redis;

namespace Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;

public interface IRedisConnectionFactory
{
    IConnectionMultiplexer Connection { get; }
}