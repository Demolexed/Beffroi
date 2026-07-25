namespace Beffroi.Core.Application.Ports;

/// <summary>
/// Port secondaire : accès à l'horloge.
/// Existe pour que le cœur ne dépende jamais de <c>DateTimeOffset.UtcNow</c> directement
/// (testabilité, et démonstration du sens des dépendances : c'est l'Infrastructure qui implémente).
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
