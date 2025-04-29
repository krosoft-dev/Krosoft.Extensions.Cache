using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Cache.Distributed.Redis.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Krosoft.Extensions.Cache.Distributed.Redis.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDistributedCacheExt(this IServiceCollection services)
    {
        services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
        services.AddTransient<IDistributedCacheProvider, DistributedCacheProvider>();
        services.AddTransient<ITenantDistributedCacheProvider, TenantDistributedCacheProvider>();

        return services;
    }
}