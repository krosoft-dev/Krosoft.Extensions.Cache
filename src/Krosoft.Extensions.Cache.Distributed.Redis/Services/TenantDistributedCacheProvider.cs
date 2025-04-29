using Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;

namespace Krosoft.Extensions.Cache.Distributed.Redis.Services;

public class TenantDistributedCacheProvider : ITenantDistributedCacheProvider
{
    private readonly IDistributedCacheProvider _distributedCacheProvider;

    public TenantDistributedCacheProvider(IDistributedCacheProvider distributedCacheProvider)
    {
        _distributedCacheProvider = distributedCacheProvider;
    }

    public Task DeleteAllAsync(string tenantId,
                               string pattern,
                               CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, pattern);
        return _distributedCacheProvider.DeleteAllAsync(tenantKey, cancellationToken);
    }

    public Task<bool> DeleteAsync(string tenantId,
                                  string key,
                                  CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, key);
        return _distributedCacheProvider.DeleteAsync(tenantKey, cancellationToken);
    }

    public Task<T?> GetAsync<T>(string tenantId,
                                string key,
                                CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, key);
        return _distributedCacheProvider.GetAsync<T>(tenantKey, cancellationToken);
    }

    public IEnumerable<string> GetKeys(string tenantId,
                                       string pattern)

    {
        var tenantKey = GetTenantKey(tenantId, pattern);
        return _distributedCacheProvider.GetKeys(tenantKey);
    }

    public Task<long> GetLengthAsync(string tenantId,
                                     string collectionKey,
                                     CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, collectionKey);
        return _distributedCacheProvider.GetLengthAsync(tenantKey, cancellationToken);
    }

    public Task<bool> IsExistAsync(string tenantId,
                                   string key,
                                   CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, key);
        return _distributedCacheProvider.IsExistAsync(tenantKey, cancellationToken);
    }

    public Task<TimeSpan> PingAsync(CancellationToken cancellationToken = default) => _distributedCacheProvider.PingAsync(cancellationToken);

    /// <summary>
    /// Récupère une ligne de la collection parente.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="collectionKey">Clé de la collection parente.</param>
    /// <param name="entryKey">Clé de l'entrée.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Une ligne de la collection parente.</returns>
    public Task<T?> ReadRowAsync<T>(string tenantId,
                                    string collectionKey,
                                    string entryKey,
                                    CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, collectionKey);
        return _distributedCacheProvider.ReadRowAsync<T>(tenantKey, entryKey, cancellationToken);
    }

    public Task<List<T>> ReadRowsAsync<T>(string tenantId,
                                          string collectionKey,
                                          IEnumerable<string> entryKeys,
                                          CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, collectionKey);
        return _distributedCacheProvider.ReadRowsAsync<T>(tenantKey, entryKeys, cancellationToken);
    }

    public Task<IEnumerable<T>> ReadRowsAsync<T>(string tenantId,
                                                 string collectionKey,
                                                 CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, collectionKey);
        return _distributedCacheProvider.ReadRowsAsync<T>(tenantKey, cancellationToken);
    }

    public Task RefreshAsync<T>(string tenantId,
                                string collectionKey,
                                Func<Task<List<T>>> func,
                                Func<T, string> getId,
                                CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, collectionKey);
        return _distributedCacheProvider.RefreshAsync(tenantKey, func, getId, cancellationToken);
    }

    public Task SetAsync<T>(string tenantId,
                            string key,
                            T entry,
                            CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, key);
        return _distributedCacheProvider.SetAsync(tenantKey, entry, cancellationToken);
    }

    public Task<bool> SetRowAsync<T>(string tenantId,
                                     string collectionKey,
                                     string entryKey,
                                     T entry,
                                     CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, collectionKey);
        return _distributedCacheProvider.SetRowAsync(tenantKey, entryKey, entry, cancellationToken);
    }

    public Task SetRowAsync<T>(string tenantId,
                               string collectionKey,
                               IDictionary<string, T> entryByKey,
                               CancellationToken cancellationToken = default)
    {
        var tenantKey = GetTenantKey(tenantId, collectionKey);
        return _distributedCacheProvider.SetRowAsync(tenantKey, entryByKey, cancellationToken);
    }

    private static string GetTenantKey(string tenantId,
                                       string collectionKey) => $"{tenantId}/{collectionKey}";
}