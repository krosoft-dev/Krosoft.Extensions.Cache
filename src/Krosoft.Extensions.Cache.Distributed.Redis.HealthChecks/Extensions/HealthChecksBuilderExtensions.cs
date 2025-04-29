using Krosoft.Extensions.Cache.Distributed.Redis.HealthChecks.Checks;
using Microsoft.Extensions.DependencyInjection;

namespace Krosoft.Extensions.Cache.Distributed.Redis.HealthChecks.Extensions;

public static class HealthChecksBuilderExtensions
{
    public static IHealthChecksBuilder AddRedisCheck(this IHealthChecksBuilder healthChecksBuilder)
    {
        healthChecksBuilder.AddCheck<RedisHealthCheck>("Redis");
        return healthChecksBuilder;
    }
}