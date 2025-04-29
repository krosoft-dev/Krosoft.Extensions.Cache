namespace Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;

/// <summary>
/// Interface pour les gestionnaires de cache multi-tenant.
/// </summary>
public interface ITenantDistributedCacheProvider
{
    Task DeleteAllAsync(string tenantId, string pattern, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string tenantId, string key, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string tenantId, string key, CancellationToken cancellationToken = default);
    IEnumerable<string> GetKeys(string tenantId, string pattern);
    Task<long> GetLengthAsync(string tenantId, string collectionKey, CancellationToken cancellationToken = default);

    Task<bool> IsExistAsync(string tenantId, string key, CancellationToken cancellationToken = default);
    Task<TimeSpan> PingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une ligne de la collection parente.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="collectionKey">Clé de la collection parente.</param>
    /// <param name="entryKey">Clé de l'entrée.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Une ligne de la collection parente.</returns>
    Task<T?> ReadRowAsync<T>(string tenantId, string collectionKey, string entryKey, CancellationToken cancellationToken = default);

    Task<List<T>> ReadRowsAsync<T>(string tenantId, string collectionKey, IEnumerable<string> entryKeys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une collection de type T en cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="collectionKey">Clé de la collection parente.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Collection de type T.</returns>
    Task<IEnumerable<T>> ReadRowsAsync<T>(string tenantId, string collectionKey, CancellationToken cancellationToken = default);

    Task RefreshAsync<T>(string tenantId,
                         string collectionKey,
                         Func<Task<List<T>>> func,
                         Func<T, string> getId,
                         CancellationToken cancellationToken = default);

    Task SetAsync<T>(string tenantId, string key, T entry, CancellationToken cancellationToken = default);

    Task<bool> SetRowAsync<T>(string tenantId, string collectionKey, string entryKey, T entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute une nouvelle entrée dans le cache.
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant.</param>
    /// <param name="collectionKey">Clé de la collection parente.</param>
    /// <param name="entryByKey">Elements de la collection.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    Task SetRowAsync<T>(string tenantId,
                        string collectionKey,
                        IDictionary<string, T> entryByKey,
                        CancellationToken cancellationToken = default);
}