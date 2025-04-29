#if !(NET9_0_OR_GREATER)
using System.Collections;
#endif
using System.Reflection;
using Krosoft.Extensions.Cache.Memory.Interfaces;
using Krosoft.Extensions.Core.Models.Exceptions;
using Microsoft.Extensions.Caching.Memory;

namespace Krosoft.Extensions.Cache.Memory.Services;

public class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheProvider(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Vide le cache.
    /// </summary>
    public void Clear()
    {
        var keys = GetKeys();

        foreach (var key in keys)
        {
            Remove(key);
        }
    }

    /// <summary>
    /// Obtient une entrée du cache.
    /// </summary>
    /// <typeparam name="T">Type de la valeur.</typeparam>
    /// <param name="key">Clé de l'entrée.</param>
    /// <returns>Valeur de l'entrée du cache correspondante.</returns>
    public T? Get<T>(string key)
    {
        _memoryCache.TryGetValue(key, out T? cacheEntry);
        return cacheEntry;
    }

    /// <summary>
    /// Récupère une instance d'objet de type T en cache.
    /// Si elle n'est pas présente, on l'y met.
    /// </summary>
    /// <typeparam name="T">Type de l'objet en cache.</typeparam>
    /// <param name="key">Clé de l'entrée.</param>
    /// <param name="firstLoad">Action à faire lors de la première mise en cache.</param>
    /// <returns>Instance de T depuis le cache.</returns>
    public T? Get<T>(string key, Func<T> firstLoad) => GetValueOrDefault(key, firstLoad());

    /// <summary>
    /// Récupère une collection de type T en cache.
    /// Si elle n'est pas présente, on l'y met.
    /// </summary>
    /// <typeparam name="T">Type de l'objet en cache.</typeparam>
    /// <param name="firstLoad">Action à faire lors de la première mise en cache.</param>
    /// <returns>Collection de T depuis le cache.</returns>
    public IEnumerable<T>? Get<T>(Func<IEnumerable<T>> firstLoad)
    {
        var cacheKey = GetKey<T>();

        return GetValueOrDefault(cacheKey, firstLoad());
    }

    public IDictionary<string, Type> GetItemsType()
    {
        var keys = GetKeys();

        var itemsInfo = new Dictionary<string, Type>();

        foreach (var key in keys)
        {
            var value = _memoryCache.Get(key);

            if (value != null)
            {
                itemsInfo.Add(key, value.GetType());
            }
        }

        return itemsInfo;
    }

    public string GetKey<T>() => $"Cache_{typeof(T).Name}";

    /// <summary>
    /// Récupère les clés des objets en cache.
    /// </summary>
    /// <returns>Liste des clés des objets en cache.</returns>
    public IEnumerable<string> GetKeys()
    {
#if NET6_0
        var items = new List<string>();

        var field = typeof(MemoryCache).GetProperty("EntriesCollection",
                                                    BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null && field.GetValue(_memoryCache) is ICollection collection)
        {
            foreach (var item in collection)
            {
                var methodInfo = item.GetType().GetProperty("Key");
                if (methodInfo != null)
                {
                    var val = methodInfo.GetValue(item);
                    var key = val?.ToString();
                    if (key != null)
                    {
                        items.Add(key);
                    }
                }
            }

            return items;
        }

#endif
#if (NET7_0 || NET8_0)
        var fieldInfo = typeof(MemoryCache).GetField("_coherentState", BindingFlags.Instance | BindingFlags.NonPublic);
        if (fieldInfo != null)
        {
            var propertyInfo = fieldInfo.FieldType.GetProperty("EntriesCollection", BindingFlags.Instance | BindingFlags.NonPublic);
            var value = fieldInfo.GetValue(_memoryCache);
            if (propertyInfo != null)
            {
                var dict = propertyInfo.GetValue(value);
                if (dict is IDictionary cacheEntries)
                {
                    return cacheEntries.Keys
                                       .OfType<string>()
                                       .Select(d => d)
                                       .ToList();
                }
            }
        }
#endif
#if (NET9_0_OR_GREATER)
        var propertyInfo = typeof(MemoryCache).GetProperty("Keys", BindingFlags.Instance | BindingFlags.Public);

        if (propertyInfo != null)
        {
            var cacheKeys = propertyInfo
                .GetValue(_memoryCache) as IEnumerable<object>;

            if (cacheKeys == null)
            {
                return new List<string>();
            }

            return cacheKeys.Select(x => x.ToString())!;
        }

#endif

        throw new KrosoftTechnicalException("Impossible de récupérer les clés du cache mémoire.");
    }

    /// <summary>
    /// Obtient une entrée du cache.
    /// Si non trouvée, renvoie la valeur par défaut.
    /// </summary>
    /// <typeparam name="T">Type de la valeur.</typeparam>
    /// <param name="key">Clé.</param>
    /// <param name="defaultValue">Valeur par défaut.</param>
    /// <returns>Valeur trouvée ou valeur par défaut.</returns>
    public T? GetValueOrDefault<T>(string key, T? defaultValue = default)
    {
        if (!_memoryCache.TryGetValue(key, out T? cacheEntry))
        {
            return defaultValue;
        }

        return cacheEntry;
    }

    /// <summary>
    /// Permet de savoir si une clé est définie dans le cache.
    /// </summary>
    /// <param name="key">Clé à rechercher.</param>
    /// <returns><c>true</c> si elle existe dans le cache, <c>false</c> sinon.</returns>
    public bool IsSet(string key) => _memoryCache.TryGetValue(key, out _);

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    /// <summary>
    /// Vide le cache d'un objet de type T.
    /// </summary>
    /// <typeparam name="T">Type de l'objet en cache.</typeparam>
    public void Remove<T>()
    {
        Remove(GetKey<T>());
    }

    /// <summary>
    /// Ajoute une nouvelle entrée dans le cache.
    /// </summary>
    /// <param name="key">Clé de l'entrée.</param>
    /// <param name="value">Valeur associée à cette clé.</param>
    /// <param name="cacheTime">Durée de mise en cache (par défaut : illimité).</param>
    public void Set(string key, object value, TimeSpan cacheTime)
    {
        _memoryCache.Set(key, value, cacheTime);
    }

    public void Set(string key, object value)
    {
        _memoryCache.Set(key, value);
    }
}