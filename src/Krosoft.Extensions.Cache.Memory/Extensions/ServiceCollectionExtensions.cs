using Krosoft.Extensions.Cache.Memory.Interfaces;
using Krosoft.Extensions.Cache.Memory.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Krosoft.Extensions.Cache.Memory.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMemoryCacheExt(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddTransient<ICacheProvider, MemoryCacheProvider>();

        return services;
    }
}