using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Communes;
using Beffroi.Core.Application.Contracts;
using Beffroi.Core.Application.Ports;
using Beffroi.Core.Domain.Seances;

namespace Beffroi.Core.Application.Deliberations;

/// <summary>
/// Délibérations d'une commune, filtrables.
/// <c>NonUnanimes</c> isole les décisions qui ont fait débat — c'est le contraire d'un détail :
/// une plateforme qui ne montrerait que les votes unanimes donnerait une image fausse du conseil.
///
/// <c>Limite</c> borne le nombre de résultats : la page d'accueil n'affiche que les trois
/// dernières décisions.
/// </summary>
public sealed record ListerDeliberationsQuery(
    string CodeInsee,
    string? Thematique,
    DateOnly? Depuis,
    DateOnly? Jusqua,
    bool? NonUnanimes,
    int? Limite) : IQuery<IReadOnlyList<DeliberationDto>?>;

internal sealed class ListerDeliberationsQueryHandler(ISeanceRepository seances)
    : IQueryHandler<ListerDeliberationsQuery, IReadOnlyList<DeliberationDto>?>
{
    private const int LimiteMaximale = 200;

    public async Task<IReadOnlyList<DeliberationDto>?> HandleAsync(
        ListerDeliberationsQuery query,
        CancellationToken cancellationToken)
    {
        if (!CodeInseeValide.Essayer(query.CodeInsee, out var code))
        {
            return null;
        }

        var thematique = Projections.DepuisCode(query.Thematique);
        if (query.Thematique is not null && thematique is null)
        {
            return null;
        }

        if (query.Limite is < 1 or > LimiteMaximale)
        {
            return null;
        }

        var resultats = await seances.ListerAsync(code, query.Depuis, query.Jusqua, cancellationToken);

        // Le dépôt rend les séances de la plus récente à la plus ancienne : l'ordre des
        // délibérations en découle, sans tri supplémentaire.
        var deliberations = resultats
            .SelectMany(seance => seance.Deliberations.Select(deliberation => (deliberation, seance.Date)))
            .Where(paire => thematique is null || paire.deliberation.Thematique == thematique)
            .Where(paire => query.NonUnanimes is not true
                            || (paire.deliberation.Vote is not null && !paire.deliberation.EstUnanime));

        if (query.Limite is { } limite)
        {
            deliberations = deliberations.Take(limite);
        }

        return [.. deliberations.Select(paire => paire.deliberation.VersDto(paire.Date))];
    }
}

/// <summary>Détail d'une délibération, décompte des voix compris.</summary>
public sealed record ObtenirDeliberationQuery(Guid Id) : IQuery<DeliberationDto?>;

internal sealed class ObtenirDeliberationQueryHandler(ISeanceRepository seances)
    : IQueryHandler<ObtenirDeliberationQuery, DeliberationDto?>
{
    public async Task<DeliberationDto?> HandleAsync(
        ObtenirDeliberationQuery query,
        CancellationToken cancellationToken)
    {
        var id = new DeliberationId(query.Id);
        var seance = await seances.ObtenirParDeliberationAsync(id, cancellationToken);

        return seance?.Deliberations
            .SingleOrDefault(deliberation => deliberation.Id == id)?
            .VersDto(seance.Date);
    }
}
