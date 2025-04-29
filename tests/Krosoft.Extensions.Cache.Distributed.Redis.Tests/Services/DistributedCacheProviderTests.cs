using System.Text;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Cache.Distributed.Redis.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Krosoft.Extensions.Cache.Distributed.Redis.Tests.Services;

[TestClass]
public class DistributedCacheProviderTests
{
    private Mock<IConnectionMultiplexer> _connectionMock = null!;
    private Mock<IDatabase> _dbMock = null!;
    private Mock<IRedisConnectionFactory> _factoryMock = null!;
    private Mock<ILogger<DistributedCacheProvider>> _loggerMock = null!;
    private DistributedCacheProvider _provider = null!;

    [TestInitialize]
    public void Setup()
    {
        _factoryMock = new Mock<IRedisConnectionFactory>();
        _connectionMock = new Mock<IConnectionMultiplexer>();
        _dbMock = new Mock<IDatabase>();
        _loggerMock = new Mock<ILogger<DistributedCacheProvider>>();

        _factoryMock.Setup(f => f.Connection).Returns(_connectionMock.Object);
        _connectionMock.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_dbMock.Object);

        _provider = new DistributedCacheProvider(_loggerMock.Object, _factoryMock.Object);
    }

    [TestMethod]
    public async Task GetAsync_ShouldReturnStoredValue()
    {
        var key = "key";
        var value = new { Name = "Test" };
        var json = JsonConvert.SerializeObject(value);
        var bytes = Encoding.UTF8.GetBytes(json);
        _dbMock.Setup(db => db.StringGetAsync(key, It.IsAny<CommandFlags>())).ReturnsAsync(bytes);

        var result = await _provider.GetAsync<dynamic>(key);

        Check.That((string)result!.Name).IsEqualTo("Test");
    }

    [TestMethod]
    public async Task DeleteAsync_ShouldDeleteKey()
    {
        var key = "key";
        _dbMock.Setup(db => db.KeyDeleteAsync(key, It.IsAny<CommandFlags>())).ReturnsAsync(true);
        var result = await _provider.DeleteAsync(key);
        Check.That(result).IsTrue();
    }

    [TestMethod]
    public async Task IsExistAsync_ShouldReturnTrueIfKeyExists()
    {
        var key = "key";
        _dbMock.Setup(db => db.KeyExistsAsync(key, It.IsAny<CommandFlags>())).ReturnsAsync(true);
        var result = await _provider.IsExistAsync(key);
        Check.That(result).IsTrue();
    }

    [TestMethod]
    public async Task SetRowAsync_ShouldStoreHashEntry()
    {
        var collectionKey = "collection";
        var entryKey = "entry";
        var entry = new { Id = 1 };
        await _provider.SetRowAsync(collectionKey, entryKey, entry);
        _dbMock.Verify(db => db.HashSetAsync(collectionKey, entryKey, It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [TestMethod]
    public async Task ReadRowAsync_ShouldReturnEntry()
    {
        var collectionKey = "collection";
        var entryKey = "entry";
        var obj = new { Id = 1 };
        var json = JsonConvert.SerializeObject(obj);
        var bytes = Encoding.UTF8.GetBytes(json);
        _dbMock.Setup(db => db.HashGetAsync(collectionKey, entryKey, It.IsAny<CommandFlags>())).ReturnsAsync(bytes);

        var result = await _provider.ReadRowAsync<dynamic>(collectionKey, entryKey);
        Check.That((int)result!.Id).IsEqualTo(1);
    }
}