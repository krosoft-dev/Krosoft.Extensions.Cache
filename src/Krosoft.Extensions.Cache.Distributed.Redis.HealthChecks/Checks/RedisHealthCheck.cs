using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Core.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Krosoft.Extensions.Cache.Distributed.Redis.HealthChecks.Checks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IDistributedCacheProvider _distributedCacheProvider;

    public RedisHealthCheck(IDistributedCacheProvider distributedCacheProvider)
    {
        _distributedCacheProvider = distributedCacheProvider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeSpan = await _distributedCacheProvider.PingAsync(cancellationToken);
            return HealthCheckResult.Healthy($"Ping Redis en {timeSpan.ToShortString()}");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, ex.Message, ex);
        }
    }
}