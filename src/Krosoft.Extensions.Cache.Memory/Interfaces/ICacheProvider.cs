namespace Krosoft.Extensions.Cache.Memory.Interfaces;

/// <summary>
/// Interface pour les gestionnaires de cache.
/// </summary>
public interface ICacheProvider
{
    /// <summary>
    /// Vide le cache.
    /// </summary>
    void Clear();

    /// <summary>
    /// Obtient une entrée du cache.
    /// </summary>
    /// <typeparam name="T">Type de la valeur.</typeparam>
    /// <param name="key">Clé de l'entrée.</param>
    /// <returns>Valeur de l'entrée du cache correspondante.</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Récupère une instance d'objet de type T en cache.
    /// Si elle n'est pas présente, on l'y met.
    /// </summary>
    /// <typeparam name="T">Type de l'objet en cache.</typeparam>
    /// <param name="key">Clé de l'entrée.</param>
    /// <param name="firstLoad">Action à faire lors de la première mise en cache.</param>
    /// <returns>Instance de T depuis le cache.</returns>
    T? Get<T>(string key, Func<T> firstLoad);

    /// <summary>
    /// Récupère une collection de type T en cache.
    /// Si elle n'est pas présente, on l'y met.
    /// </summary>
    /// <typeparam name="T">Type de l'objet en cache.</typeparam>
    /// <param name="firstLoad">Action à faire lors de la première mise en cache.</param>
    /// <returns>Collection de T depuis le cache.</returns>
    IEnumerable<T>? Get<T>(Func<IEnumerable<T>> firstLoad);

    /// <summary>
    /// Récupère les types des objets en cache.
    /// </summary>
    /// <returns>Liste des types des objets en cache.</returns>
    IDictionary<string, Type> GetItemsType();

    /// <summary>
    /// Recupère la clé de cache d'un objet de type T.
    /// </summary>
    /// <typeparam name="T">Type de l'objet en cache.</typeparam>
    /// <returns>Clé de cache.</returns>
    string GetKey<T>();

    /// <summary>
    /// Récupère les clés des objets en cache.
    /// </summary>
    /// <returns>Liste des clés des objets en cache.</returns>
    IEnumerable<string> GetKeys();

    /// <summary>
    /// Obtient une entrée du cache.
    /// Si non trouvée, renvoie la valeur par défaut.
    /// </summary>
    /// <typeparam name="T">Type de la valeur.</typeparam>
    /// <param name="key">Clé.</param>
    /// <param name="defaultValue">Valeur par défaut.</param>
    /// <returns>Valeur trouvée ou valeur par défaut.</returns>
    T? GetValueOrDefault<T>(string key, T? defaultValue = default);

    /// <summary>
    /// Permet de savoir si une clé est définie dans le cache.
    /// </summary>
    /// <param name="key">Clé à rechercher.</param>
    /// <returns><c>true</c> si elle existe dans le cache, <c>false</c> sinon.</returns>
    bool IsSet(string key);

    /// <summary>
    /// Supprime une valeur du cache.
    /// </summary>
    /// <param name="key">Clé à supprimer.</param>
    void Remove(string key);

    /// <summary>
    /// Vide le cache d'un objet de type T.
    /// </summary>
    /// <typeparam name="T">Type de l'objet en cache.</typeparam>
    void Remove<T>();

    /// <summary>
    /// Ajoute une nouvelle entrée dans le cache.
    /// </summary>
    /// <param name="key">Clé de l'entrée.</param>
    /// <param name="value">Valeur associée à cette clé.</param>
    void Set(string key, object value);

    /// <summary>
    /// Ajoute une nouvelle entrée dans le cache.
    /// </summary>
    /// <param name="key">Clé de l'entrée.</param>
    /// <param name="value">Valeur associée à cette clé.</param>
    /// <param name="cacheTime">Durée de mise en cache.</param>
    void Set(string key, object value, TimeSpan cacheTime);
}