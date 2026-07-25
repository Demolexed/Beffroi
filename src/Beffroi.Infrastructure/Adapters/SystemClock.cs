using Beffroi.Core.Application.Ports;

namespace Beffroi.Infrastructure.Adapters;

/// <summary>
/// Adapter secondaire du port <see cref="IClock"/> : l'horloge réelle de la machine.
/// </summary>
internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
