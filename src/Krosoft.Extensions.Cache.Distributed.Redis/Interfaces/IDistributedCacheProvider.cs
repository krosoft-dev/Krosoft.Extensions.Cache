namespace Krosoft.Extensions.Cache.Distributed.Redis.Interfaces;

/// <summary>
/// Interface pour les gestionnaires de cache.
/// </summary>
public interface IDistributedCacheProvider
{
    Task DeleteAllAsync(string pattern, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> DeleteRowAsync(string collectionKey, string entryKey, CancellationToken cancellationToken = default);
    Task<long> DeleteRowsAsync(string collectionKey, ISet<string> entriesKey, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    IEnumerable<string> GetKeys(string pattern);
    Task<long> GetLengthAsync(string collectionKey, CancellationToken cancellationToken = default);

    Task<bool> IsExistAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> IsExistRowAsync(string collectionKey, string entryKey, CancellationToken cancellationToken = default);
    Task<TimeSpan> PingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une ligne de la collection parente.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collectionKey">Clé de la collection parente.</param>
    /// <param name="entryKey">Clé de l'entrée.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Une ligne de la collection parente.</returns>
    Task<T?> ReadRowAsync<T>(string collectionKey,
                             string entryKey,
                             CancellationToken cancellationToken = default);

    Task<List<T>> ReadRowsAsync<T>(string collectionKey,
                                   IEnumerable<string> entryKeys,
                                   CancellationToken cancellationToken = default);

    /// <summary>
    /// Récupère une collection de type T en cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collectionKey">Clé de la collection parente.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    /// <returns>Collection de type T.</returns>
    Task<IEnumerable<T>> ReadRowsAsync<T>(string collectionKey,
                                          CancellationToken cancellationToken = default);

    Task RefreshAsync<T>(string collectionKey,
                         Func<Task<List<T>>> func,
                         Func<T, string> getId,
                         CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T entry, CancellationToken cancellationToken = default);

    Task<bool> SetRowAsync<T>(string collectionKey, string entryKey, T entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute une nouvelle entrée dans le cache.
    /// </summary>
    /// <param name="collectionKey">Clé de la collection parente.</param>
    /// <param name="entryByKey">Elements de la collection.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    Task SetRowAsync<T>(string collectionKey,
                        IDictionary<string, T> entryByKey,
                        CancellationToken cancellationToken = default);
}