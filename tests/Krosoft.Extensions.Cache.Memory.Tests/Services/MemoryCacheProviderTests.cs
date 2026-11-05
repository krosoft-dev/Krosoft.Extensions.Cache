using Krosoft.Extensions.Cache.Memory.Services;
using Krosoft.Extensions.Cache.Memory.Tests.Fakes;
using Microsoft.Extensions.Caching.Memory;

namespace Krosoft.Extensions.Cache.Memory.Tests.Services;

[TestClass]
public class MemoryCacheProviderTests
{
    private FakeSystemClock _clock = null!;
    private MemoryCache _memoryCache = null!;
    private MemoryCacheProvider _provider = null!;

    [TestInitialize]
    public void Setup()
    {
        _clock = new FakeSystemClock();
        _memoryCache = new MemoryCache(new MemoryCacheOptions { Clock = _clock });
        _provider = new MemoryCacheProvider(_memoryCache);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _memoryCache.Dispose();
    }

    [TestMethod]
    public async Task GetOrCreateAsync_Appelle_La_Factory_Une_Seule_Fois()
    {
        var calls = 0;

        Task<string> Factory(CancellationToken _)
        {
            calls++;
            return Task.FromResult($"valeur{calls}");
        }

        var first = await _provider.GetOrCreateAsync("cle", Factory, TimeSpan.FromSeconds(5), CancellationToken.None);
        var second = await _provider.GetOrCreateAsync("cle", Factory, TimeSpan.FromSeconds(5), CancellationToken.None);

        Check.That(calls).IsEqualTo(1);
        Check.That(first).IsEqualTo("valeur1");
        Check.That(second).IsEqualTo("valeur1");
    }

    [TestMethod]
    public async Task GetOrCreateAsync_Rappelle_La_Factory_Apres_Expiration()
    {
        var calls = 0;

        Task<int> Factory(CancellationToken _)
        {
            calls++;
            return Task.FromResult(calls);
        }

        await _provider.GetOrCreateAsync("cle", Factory, TimeSpan.FromSeconds(5), CancellationToken.None);

        _clock.Advance(TimeSpan.FromSeconds(4));
        var avantExpiration = await _provider.GetOrCreateAsync("cle", Factory, TimeSpan.FromSeconds(5), CancellationToken.None);

        _clock.Advance(TimeSpan.FromSeconds(2));
        var apresExpiration = await _provider.GetOrCreateAsync("cle", Factory, TimeSpan.FromSeconds(5), CancellationToken.None);

        Check.That(avantExpiration).IsEqualTo(1);
        Check.That(apresExpiration).IsEqualTo(2);
        Check.That(calls).IsEqualTo(2);
    }

    [TestMethod]
    public async Task GetOrCreateAsync_Sans_Duree_N_Expire_Pas()
    {
        var calls = 0;

        Task<int> Factory(CancellationToken _)
        {
            calls++;
            return Task.FromResult(calls);
        }

        await _provider.GetOrCreateAsync("cle", Factory, null, CancellationToken.None);

        _clock.Advance(TimeSpan.FromDays(365));
        var result = await _provider.GetOrCreateAsync("cle", Factory, null, CancellationToken.None);

        Check.That(result).IsEqualTo(1);
        Check.That(calls).IsEqualTo(1);
    }

    [TestMethod]
    public async Task GetOrCreateAsync_Ne_Met_Rien_En_Cache_Si_La_Factory_Echoue()
    {
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
            _provider.GetOrCreateAsync<string>("cle",
                                               _ => throw new InvalidOperationException("Erreur"),
                                               TimeSpan.FromSeconds(5),
                                               CancellationToken.None));

        Check.That(_provider.IsSet("cle")).IsFalse();

        var result = await _provider.GetOrCreateAsync("cle", _ => Task.FromResult("valeur"), TimeSpan.FromSeconds(5), CancellationToken.None);

        Check.That(result).IsEqualTo("valeur");
    }

    [TestMethod]
    public async Task GetOrCreateAsync_Transmet_Le_Jeton_D_Annulation()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken received = default;

        await _provider.GetOrCreateAsync("cle", ct =>
        {
            received = ct;
            return Task.FromResult(1);
        }, null, cts.Token);

        Check.That(received).IsEqualTo(cts.Token);
    }

    [TestMethod]
    public void Get_Avec_Cle_Appelle_FirstLoad_Une_Seule_Fois_Et_Met_En_Cache()
    {
        var calls = 0;

        string FirstLoad()
        {
            calls++;
            return $"valeur{calls}";
        }

        var first = _provider.Get("cle", FirstLoad);
        var second = _provider.Get("cle", FirstLoad);

        Check.That(calls).IsEqualTo(1);
        Check.That(first).IsEqualTo("valeur1");
        Check.That(second).IsEqualTo("valeur1");
        Check.That(_provider.Get<string>("cle")).IsEqualTo("valeur1");
    }

    [TestMethod]
    public void Get_Avec_Cle_N_Appelle_Pas_FirstLoad_Si_La_Valeur_Existe()
    {
        _provider.Set("cle", "existante");
        var calls = 0;

        var result = _provider.Get("cle", () =>
        {
            calls++;
            return "nouvelle";
        });

        Check.That(calls).IsEqualTo(0);
        Check.That(result).IsEqualTo("existante");
    }

    [TestMethod]
    public void Get_Collection_Appelle_FirstLoad_Une_Seule_Fois_Et_Met_En_Cache()
    {
        var calls = 0;

        IEnumerable<int> FirstLoad()
        {
            calls++;
            return new List<int> { calls };
        }

        var first = _provider.Get(FirstLoad);
        var second = _provider.Get(FirstLoad);

        Check.That(calls).IsEqualTo(1);
        Check.That(first).ContainsExactly(1);
        Check.That(second).ContainsExactly(1);
        Check.That(_provider.IsSet(_provider.GetKey<int>())).IsTrue();
    }
}
