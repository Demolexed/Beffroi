using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Communes;
using Beffroi.Core.Application.Contracts;
using Beffroi.Core.Application.Ports;
using Beffroi.Core.Domain.Thematiques.Enums;

namespace Beffroi.Core.Application.Thematiques;

/// <summary>
/// Le référentiel thématique. Exposé pour que le front n'ait pas à le dupliquer :
/// c'est la même grille de lecture pour toutes les communes, elle doit venir d'un seul endroit.
/// </summary>
public sealed record ListerThematiquesQuery : IQuery<IReadOnlyList<ThematiqueDto>>;

internal sealed class ListerThematiquesQueryHandler
    : IQueryHandler<ListerThematiquesQuery, IReadOnlyList<ThematiqueDto>>
{
    public Task<IReadOnlyList<ThematiqueDto>> HandleAsync(
        ListerThematiquesQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ThematiqueDto> thematiques =
            [.. Enum.GetValues<Thematique>().Select(thematique => thematique.VersDto())];

        return Task.FromResult(thematiques);
    }
}

/// <summary>
/// Les sept thématiques vues depuis une commune : poids budgétaire et dernière décision.
///
/// Les sept sont toujours renvoyées, y compris celles sans budget ni décision — une thématique
/// vide est une information en soi (« la commune n'a rien décidé sur la sécurité cette année »),
/// et l'escamoter donnerait une image incomplète.
/// </summary>
public sealed record ListerThematiquesCommunalesQuery(string CodeInsee)
    : IQuery<IReadOnlyList<ThematiqueCommunaleDto>?>;

internal sealed class ListerThematiquesCommunalesQueryHandler(
    ICommuneRepository communes,
    IBudgetRepository budgets,
    ISeanceRepository seances)
    : IQueryHandler<ListerThematiquesCommunalesQuery, IReadOnlyList<ThematiqueCommunaleDto>?>
{
    public async Task<IReadOnlyList<ThematiqueCommunaleDto>?> HandleAsync(
        ListerThematiquesCommunalesQuery query,
        CancellationToken cancellationToken)
    {
        if (!CodeInseeValide.Essayer(query.CodeInsee, out var code))
        {
            return null;
        }

        var commune = await communes.ObtenirParCodeAsync(code, cancellationToken);
        if (commune is null)
        {
            return null;
        }

        var budget = await budgets.ObtenirLePlusRecentAsync(code, cancellationToken);
        var reunions = await seances.ListerAsync(code, null, null, cancellationToken);

        // Les séances arrivent de la plus récente à la plus ancienne : la première délibération
        // rencontrée pour une thématique est donc la plus récente.
        var dernieres = new Dictionary<Thematique, DeliberationDto>();
        foreach (var seance in reunions)
        {
            foreach (var deliberation in seance.Deliberations)
            {
                if (deliberation.Thematique is { } thematique && !dernieres.ContainsKey(thematique))
                {
                    dernieres[thematique] = deliberation.VersDto(seance.Date);
                }
            }
        }

        return
        [
            .. Enum.GetValues<Thematique>().Select(thematique => new ThematiqueCommunaleDto(
                Projections.Code(thematique),
                Projections.Libelle(thematique),
                budget?.Exercice,
                budget?.PartPour(thematique),
                budget?.TotalPour(thematique).Euros,
                dernieres.GetValueOrDefault(thematique)))
        ];
    }
}
