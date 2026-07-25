using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Ports;

namespace Beffroi.Core.Application.Diagnostics;

/// <summary>
/// Requête de vérification : traverse le dispatcher et un port secondaire (<see cref="IClock"/>)
/// pour prouver que la plomberie hexagonale + CQRS est correctement câblée.
/// </summary>
public sealed record PingQuery : IQuery<PingResponse>;

/// <summary>
/// Réponse de la requête <see cref="PingQuery"/>.
/// </summary>
/// <param name="Service">Nom du service interrogé.</param>
/// <param name="Status">Statut renvoyé par le cœur applicatif.</param>
/// <param name="ServerTimeUtc">Heure serveur, obtenue via le port <see cref="IClock"/>.</param>
public sealed record PingResponse(string Service, string Status, DateTimeOffset ServerTimeUtc);

internal sealed class PingQueryHandler(IClock clock) : IQueryHandler<PingQuery, PingResponse>
{
    public Task<PingResponse> HandleAsync(PingQuery query, CancellationToken cancellationToken)
        => Task.FromResult(new PingResponse("Beffroi", "ok", clock.UtcNow));
}
