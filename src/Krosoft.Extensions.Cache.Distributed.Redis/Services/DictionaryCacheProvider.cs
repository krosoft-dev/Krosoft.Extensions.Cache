using System.Collections;
using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;
using Krosoft.Extensions.Core.Extensions;
using Newtonsoft.Json;

namespace Krosoft.Extensions.Cache.Distributed.Redis.Services;

public class DictionaryCacheProvider : IDistributedCacheProvider
{
    /// <summary>
    /// Simulation d'un cache via un dictionnaire.
    /// </summary>
    private readonly Dictionary<string, dynamic> _cache = new Dictionary<string, dynamic>();

    public async Task DeleteAllAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var keys = GetKeys(pattern);
        foreach (var key in keys)
        {
            await DeleteAsync(key, cancellationToken);
        }
    }

    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return await Task.FromResult(true);
    }

    public Task<bool> DeleteRowAsync(string collectionKey, string entryKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<long> DeleteRowsAsync(string collectionKey, ISet<string> entriesKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public IEnumerable<string> GetKeys(string pattern) => _cache.Keys;

    public async Task<long> GetLengthAsync(string collectionKey, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        if (!_cache.ContainsKey(collectionKey))
        {
            return 0;
        }

        var item = _cache[collectionKey];
        if (item is string)
        {
            return 1;
        }

        if (item is IDictionary dico)
        {
            return dico.Count;
        }

        return 0;
    }

    public async Task<bool> IsExistAsync(string key, CancellationToken cancellationToken = default)
    {
        var containsKey = _cache.ContainsKey(key);
        return await Task.FromResult(containsKey);
    }

    public async Task<bool> IsExistRowAsync(string collectionKey, string entryKey, CancellationToken cancellationToken = default)
    {
        var containsKey = _cache.ContainsKey(collectionKey);
        return await Task.FromResult(containsKey);
    }

    public async Task<TimeSpan> PingAsync(CancellationToken cancellationToken = default) => await Task.FromResult(new TimeSpan(0, 0, 42));

    public async Task<T?> ReadRowAsync<T>(string collectionKey, string entryKey, CancellationToken cancellationToken = default)
    {
        var collection = GetCollection<T>(collectionKey);

        var entry = collection.GetValueOrDefault(entryKey);

        return await Task.FromResult(entry);
    }

    public async Task<List<T>> ReadRowsAsync<T>(string collectionKey, IEnumerable<string> entryKeys, CancellationToken cancellationToken = default)
    {
        var collection = GetCollection<T>(collectionKey);

        var entries = new List<T>();
        foreach (var entryKey in entryKeys)
        {
            var entry = collection.GetValueOrDefault(entryKey);
            if (entry != null)
            {
                entries.Add(entry);
            }
        }

        return await Task.FromResult(entries);
    }

    public async Task<IEnumerable<T>> ReadRowsAsync<T>(string collectionKey, CancellationToken cancellationToken = default)
    {
        var collection = GetCollection<T>(collectionKey);
        var collectionValues = collection.Values;
        return await Task.FromResult(collectionValues);
    }

    public async Task RefreshAsync<T>(string collectionKey, Func<Task<List<T>>> func, Func<T, string> getId, CancellationToken cancellationToken = default)
    {
        var items = await func();
        await DeleteAsync(collectionKey, cancellationToken);
        var itemById = items.ToDictionary(getId);
        await SetRowAsync(collectionKey, itemById, cancellationToken);
    }

    public async Task SetAsync<T>(string key, T entry, CancellationToken cancellationToken = default)
    {
        var dataString = JsonConvert.SerializeObject(entry);
        _cache.Add(key, dataString);

        await Task.CompletedTask;
    }

    public Task<bool> SetRowAsync<T>(string collectionKey, string entryKey, T entry, CancellationToken cancellationToken = default)
    {
        var collection = GetCollection<T>(collectionKey);
        collection.Add(entryKey, entry);
        _cache.AddOrReplace(collectionKey, collection);

        return Task.FromResult(true);
    }

    public async Task SetRowAsync<T>(string collectionKey, IDictionary<string, T> entryByKey, CancellationToken cancellationToken = default)
    {
        _cache.Add(collectionKey, entryByKey);
        await Task.CompletedTask;
    }

    public Task<bool> DeleteRowAsync(string collectionKey, ISet<string> entriesKey, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    private IDictionary<string, T> GetCollection<T>(string collectionKey)
    {
        Dictionary<string, T> collection;
        if (!_cache.ContainsKey(collectionKey))

        {
            collection = new Dictionary<string, T>();
        }
        else
        {
            collection = _cache[collectionKey] as Dictionary<string, T> ?? new Dictionary<string, T>();
        }

        return collection;
    }
}