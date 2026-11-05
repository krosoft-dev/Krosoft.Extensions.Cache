using Microsoft.Extensions.Internal;

namespace Krosoft.Extensions.Cache.Memory.Tests.Fakes;

/// <summary>
/// Horloge contrôlable pour tester l'expiration du cache sans attendre.
/// </summary>
public class FakeSystemClock : ISystemClock
{
    public DateTimeOffset UtcNow { get; private set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Avance l'horloge de la durée indiquée.
    /// </summary>
    /// <param name="duration">Durée à ajouter.</param>
    public void Advance(TimeSpan duration)
    {
        UtcNow = UtcNow.Add(duration);
    }
}
