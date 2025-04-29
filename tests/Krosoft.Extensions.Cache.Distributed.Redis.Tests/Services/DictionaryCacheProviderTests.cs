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



    
 
    private DictionaryCacheProvider _provider = null!;

    [TestInitialize]
    public void Setup()
    {
        _provider = new DictionaryCacheProvider();
    }

    [TestMethod]
    public async Task SetAsync_should_store_and_get_value()
    {
        var key = "key1";
        var value = "value1";

        await _provider.SetAsync(key, value);
        var result = await _provider.GetAsync<string>(key);

        Check.That(result).IsEqualTo(value);
    }

    [TestMethod]
    public async Task DeleteAsync_should_remove_key()
    {
        var key = "key2";
        await _provider.SetAsync(key, "value2");

        var deleted = await _provider.DeleteAsync(key);
        var result = await _provider.GetAsync<string>(key);

        Check.That(deleted).IsTrue();
        Check.That(result).IsNull();
    }

    [TestMethod]
    public async Task DeleteAllAsync_should_remove_keys_matching_pattern()
    {
        await _provider.SetAsync("user:1", "a");
        await _provider.SetAsync("user:2", "b");
        await _provider.SetAsync("session:1", "c");

        await _provider.DeleteAllAsync("user:");

        var keys = _provider.GetKeys("user:");
        Check.That(keys).IsEmpty();
        Check.That((await _provider.GetAsync<string>("session:1"))).IsEqualTo("c");
    }

    [TestMethod]
    public async Task IsExistAsync_should_return_true_for_existing_key()
    {
        await _provider.SetAsync("key3", "value3");
        var exists = await _provider.IsExistAsync("key3");

        Check.That(exists).IsTrue();
    }

    [TestMethod]
    public async Task SetRowAsync_should_store_entry_in_collection()
    {
        await _provider.SetRowAsync("collection1", "entry1", 123);
        var result = await _provider.ReadRowAsync<int>("collection1", "entry1");

        Check.That(result).IsEqualTo(123);
    }

    [TestMethod]
    public async Task SetRowAsync_with_dict_should_store_multiple_entries()
    {
        var data = new Dictionary<string, string>
        {
            ["e1"] = "v1",
            ["e2"] = "v2"
        };

        await _provider.SetRowAsync("collection2", data);
        var result = await _provider.ReadRowsAsync<string>("collection2");

        Check.That(result).ContainsExactly("v1", "v2");
    }

    [TestMethod]
    public async Task DeleteRowAsync_should_remove_entry_from_collection()
    {
        await _provider.SetRowAsync("collection3", "entry3", "toDelete");
        var deleted = await _provider.DeleteRowAsync("collection3", "entry3");
        var result = await _provider.ReadRowAsync<string>("collection3", "entry3");

        Check.That(deleted).IsTrue();
        Check.That(result).IsNull();
    }

    [TestMethod]
    public async Task DeleteRowsAsync_should_remove_multiple_entries()
    {
        await _provider.SetRowAsync("collection4", new Dictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2,
            ["c"] = 3
        });

        var deletedCount = await _provider.DeleteRowsAsync("collection4", new HashSet<string> { "a", "c" });
        var result = await _provider.ReadRowsAsync<int>("collection4");

        Check.That(deletedCount).IsEqualTo(2);
        Check.That(result).ContainsExactly(2);
    }

    [TestMethod]
    public async Task ReadRowsAsync_by_keys_should_return_selected_entries()
    {
        await _provider.SetRowAsync("collection5", new Dictionary<string, string>
        {
            ["a"] = "v1",
            ["b"] = "v2"
        });

        var result = await _provider.ReadRowsAsync<string>("collection5", new[] { "b" });

        Check.That(result).ContainsExactly("v2");
    }

    [TestMethod]
    public async Task GetLengthAsync_should_return_correct_length()
    {
        await _provider.SetRowAsync("collection6", new Dictionary<string, string>
        {
            ["a"] = "v1",
            ["b"] = "v2"
        });

        var length = await _provider.GetLengthAsync("collection6");

        Check.That(length).IsEqualTo(2);
    }

    [TestMethod]
    public async Task IsExistRowAsync_should_detect_entry()
    {
        await _provider.SetRowAsync("collection7", "rowKey", "yes");
        var exists = await _provider.IsExistRowAsync("collection7", "rowKey");

        Check.That(exists).IsTrue();
    }

    [TestMethod]
    public void PingAsync_should_return_valid_timespan()
    {
        var ts = _provider.PingAsync().Result;
        Check.That(ts.TotalMilliseconds).IsStrictlyGreaterThan(0);
    }

    [TestMethod]
    public void GetKeys_should_return_keys_matching_prefix()
    {
        _provider.SetAsync("abc:test", "1").Wait();
        _provider.SetAsync("abc:dev", "2").Wait();
        _provider.SetAsync("xyz:prod", "3").Wait();

        var keys = _provider.GetKeys("abc:");

        Check.That(keys).ContainsExactly("abc:test", "abc:dev");
    }
 



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