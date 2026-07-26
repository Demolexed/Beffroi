using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Communes;
using Beffroi.Core.Application.Contracts;
using Beffroi.Core.Application.Ports;
using Beffroi.Core.Domain.Seances;

namespace Beffroi.Core.Application.Seances;

/// <summary>Séances d'une commune, de la plus récente à la plus ancienne.</summary>
public sealed record ListerSeancesQuery(string CodeInsee, DateOnly? Depuis, DateOnly? Jusqua)
    : IQuery<IReadOnlyList<SeanceDto>?>;

internal sealed class ListerSeancesQueryHandler(ISeanceRepository seances, IClock horloge)
    : IQueryHandler<ListerSeancesQuery, IReadOnlyList<SeanceDto>?>
{
    public async Task<IReadOnlyList<SeanceDto>?> HandleAsync(
        ListerSeancesQuery query,
        CancellationToken cancellationToken)
    {
        if (!CodeInseeValide.Essayer(query.CodeInsee, out var code))
        {
            return null;
        }

        var aujourdhui = DateOnly.FromDateTime(horloge.UtcNow.UtcDateTime);
        var resultats = await seances.ListerAsync(code, query.Depuis, query.Jusqua, cancellationToken);

        return [.. resultats.Select(seance => seance.VersDto(aujourdhui))];
    }
}

/// <summary>Détail d'une séance : délibérations, présences et état du procès-verbal.</summary>
public sealed record ObtenirSeanceQuery(Guid Id) : IQuery<SeanceDetailDto?>;

internal sealed class ObtenirSeanceQueryHandler(ISeanceRepository seances, IClock horloge)
    : IQueryHandler<ObtenirSeanceQuery, SeanceDetailDto?>
{
    public async Task<SeanceDetailDto?> HandleAsync(ObtenirSeanceQuery query, CancellationToken cancellationToken)
    {
        var seance = await seances.ObtenirAsync(new SeanceId(query.Id), cancellationToken);
        var aujourdhui = DateOnly.FromDateTime(horloge.UtcNow.UtcDateTime);

        return seance?.VersDetailDto(aujourdhui);
    }
}
