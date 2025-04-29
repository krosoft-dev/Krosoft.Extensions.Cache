using System.Diagnostics;
using System.Net;
using System.Text;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Core.Extensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Krosoft.Extensions.Cache.Distributed.Redis.Services;

public class DistributedCacheProvider : IDistributedCacheProvider
{
    private readonly IRedisConnectionFactory _factory;
    private readonly ILogger<DistributedCacheProvider> _logger;

    public DistributedCacheProvider(ILogger<DistributedCacheProvider> logger, IRedisConnectionFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public async Task DeleteAllAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var keys = GetKeys(pattern);
        foreach (var key in keys)
        {
            await DeleteAsync(key, cancellationToken);
        }
    }

    public async Task<bool> DeleteAsync(string key,
                                        CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var isDelete = await db.KeyDeleteAsync(key);
        return isDelete;
    }

    public async Task<bool> DeleteRowAsync(string collectionKey,
                                           string entryKey,
                                           CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var isDelete = await db.HashDeleteAsync(collectionKey, entryKey);
        return isDelete;
    }

    public async Task<long> DeleteRowsAsync(string collectionKey,
                                            ISet<string> entriesKey,
                                            CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();

        var hashFields = Map(entriesKey);

        var isDelete = await db.HashDeleteAsync(collectionKey, hashFields);
        return isDelete;
    }

    public async Task<T?> GetAsync<T>(string key,
                                      CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var data = await db.StringGetAsync(key);
        return ToObject<T>(data);
    }

    public IEnumerable<string> GetKeys(string pattern)
    {
        var endpoint = (DnsEndPoint)_factory.Connection.GetEndPoints()[0];
        var server = _factory.Connection.GetServer(endpoint.Host, endpoint.Port);
        var redisKeys = server.Keys(0, $"{pattern}*");
        var keys = new List<string>();
        foreach (var redisKey in redisKeys)
        {
            keys.Add(redisKey!);
        }

        return keys;
    }

    public async Task<long> GetLengthAsync(string collectionKey,
                                           CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var redisType = await db.KeyTypeAsync(collectionKey);
        if (redisType == RedisType.Hash)
        {
            return await db.HashLengthAsync(collectionKey);
        }

        return 1;
    }

    public async Task<bool> IsExistAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var isExists = await db.KeyExistsAsync(key);
        return isExists;
    }

    public async Task<bool> IsExistRowAsync(string collectionKey,
                                            string entryKey,
                                            CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var keys = await db.HashKeysAsync(collectionKey);
        var isExists = keys.Contains(entryKey);
        return isExists;
    }

    public async Task<TimeSpan> PingAsync(CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var timeSpan = await db.PingAsync();
        return timeSpan;
    }

    /// <summary>
    /// Récupère une ligne de la collection parente.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collectionKey">Clé de la collection parente.</param>
    /// <param name="entryKey">Clé de l'entrée.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Une ligne de la collection parente.</returns>
    public async Task<T?> ReadRowAsync<T>(string collectionKey,
                                          string entryKey,
                                          CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var redisValue = await db.HashGetAsync(collectionKey, entryKey);
        return ToObject<T>(redisValue);
    }

    /// <summary>
    /// Récupère une collection de type T en cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collectionKey">Clé de la collection parente.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Collection de type T.</returns>
    public async Task<IEnumerable<T>> ReadRowsAsync<T>(string collectionKey, CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var data = await db.HashGetAllAsync(collectionKey);
        return data.Select(x => ToObject<T>(x.Value))
                   .Where(x => !EqualityComparer<T>.Default.Equals(x, default))
                   .ToList()!;
    }

    public async Task<List<T>> ReadRowsAsync<T>(string collectionKey,
                                                IEnumerable<string> entryKeys,
                                                CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var hashFields = entryKeys.Select(key => (RedisValue)key).ToArray();
        var data = await db.HashGetAsync(collectionKey, hashFields);
        return data.Select(ToObject<T>)
                   .Where(x => !EqualityComparer<T>.Default.Equals(x, default))
                   .ToList()!;
    }

    public async Task RefreshAsync<T>(string collectionKey, Func<Task<List<T>>> func, Func<T, string> getId, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        var items = await func();
        await DeleteAsync(collectionKey, cancellationToken);
        var itemById = items.ToDictionary(getId);
        await SetRowAsync(collectionKey, itemById, cancellationToken);

        _logger.LogInformation($"Refresh du cache {collectionKey} en {sw.Elapsed.ToShortString()}");
    }

    public async Task SetAsync<T>(string key,
                                  T entry,
                                  CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var data = ToRedisValue(entry);
        await db.StringSetAsync(key, data);
    }

    public async Task<bool> SetRowAsync<T>(string collectionKey,
                                           string entryKey,
                                           T entry,
                                           CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var data = ToRedisValue(entry);
        return await db.HashSetAsync(collectionKey, entryKey, data);
    }

    /// <summary>
    /// Ajoute une nouvelle entrée dans le cache.
    /// </summary>
    /// <param name="collectionKey">Clé de la collection parente.</param>
    /// <param name="entryByKey">Elements de la collection.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    public async Task SetRowAsync<T>(string collectionKey,
                                     IDictionary<string, T> entryByKey,
                                     CancellationToken cancellationToken = default)
    {
        var db = _factory.Connection.GetDatabase();
        var entries = ToHashEntries(entryByKey);
        await db.HashSetAsync(collectionKey, entries);
    }

    private static RedisValue[] Map(ISet<string> entriesKey)
    {
        var testValues = new List<RedisValue>();

        foreach (var entryKey in entriesKey)
        {
            testValues.Add(entryKey);
        }

        return testValues.ToArray();
    }

    private static HashEntry[] ToHashEntries<T>(IDictionary<string, T> entryByKey)
    {
        var entries = new List<HashEntry>();

        foreach (var entry in entryByKey)
        {
            var data = ToRedisValue(entry.Value);
            var hashEntry = new HashEntry(entry.Key, data);
            entries.Add(hashEntry);
        }

        return entries.ToArray();
    }

    private static T? ToObject<T>(RedisValue redisValue)
    {
        if (redisValue.HasValue)
        {
            var entryString = Encoding.UTF8.GetString(redisValue!);

            var entry = JsonConvert.DeserializeObject<T>(entryString);
            return entry;
        }

        return default;
    }

    private static byte[] ToRedisValue<T>(T entry)
    {
        var dataString = JsonConvert.SerializeObject(entry);
        var data = Encoding.UTF8.GetBytes(dataString);
        return data;
    }
}