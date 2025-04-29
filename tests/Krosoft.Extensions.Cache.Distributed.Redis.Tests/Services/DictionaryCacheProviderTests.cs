using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Cache.Distributed.Redis.Services;
using Krosoft.Extensions.Cache.Distributed.Redis.Tests.Models;
using Krosoft.Extensions.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Krosoft.Extensions.Cache.Distributed.Redis.Tests.Services;

[TestClass]
public class DictionaryCacheProviderTests : BaseTest
{
    protected override void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IDistributedCacheProvider, DictionaryCacheProvider>();
    }

    [TestMethod]
    public async Task ReadRowAsyncTest()
    {
        await using var serviceProvider = CreateServiceCollection();
        var s = serviceProvider.GetService<IDistributedCacheProvider>();
        if (s != null)
        {
            var paysCache = await s.ReadRowAsync<PaysCache>("Pays", "00000000-0000-0000-0000-000000000000");
            Check.That(paysCache).IsNull();
        }
    }

    [TestMethod]
    public async Task ReadRowsAsyncTest()
    {
        await using var serviceProvider = CreateServiceCollection();
        var s = serviceProvider.GetService<IDistributedCacheProvider>();
        if (s != null)
        {
            var paysId = new List<string> { "00000000-0000-0000-0000-000000000000" };
            var paysFromCache = await s.ReadRowsAsync<PaysCache>("Pays", paysId);
            Check.That(paysFromCache).IsEmpty();
        }
    }

    [TestMethod]
    public async Task SetRowAsyncTest()
    {
        await using var serviceProvider = CreateServiceCollection();
        var s = serviceProvider.GetService<IDistributedCacheProvider>();
        if (s != null)
        {
            await s.SetRowAsync("CacheKeys", "contactId", new HashSet<string> { "test", "test" });
            var perimetre = await s.ReadRowAsync<HashSet<string>>("CacheKeys", "contactId");
            Check.That(perimetre).ContainsExactly("test");
        }
    }
}