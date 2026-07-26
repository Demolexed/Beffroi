using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Communes;
using Beffroi.Core.Application.Contracts;
using Beffroi.Core.Application.Ports;
using Beffroi.Core.Domain.Programmes;
using Beffroi.Core.Domain.Programmes.Enums;

namespace Beffroi.Core.Application.Programmes;

/// <summary>Programmes électoraux connus pour une commune, engagements compris.</summary>
public sealed record ListerProgrammesQuery(string CodeInsee) : IQuery<IReadOnlyList<ProgrammeDto>?>;

internal sealed class ListerProgrammesQueryHandler(IProgrammeRepository programmes)
    : IQueryHandler<ListerProgrammesQuery, IReadOnlyList<ProgrammeDto>?>
{
    public async Task<IReadOnlyList<ProgrammeDto>?> HandleAsync(
        ListerProgrammesQuery query,
        CancellationToken cancellationToken)
    {
        if (!CodeInseeValide.Essayer(query.CodeInsee, out var code))
        {
            return null;
        }

        var resultats = await programmes.ListerAsync(code, cancellationToken);
        return [.. resultats.Select(programme => programme.VersDto())];
    }
}

/// <summary>
/// Engagements d'une commune, tous programmes confondus, filtrables par thématique et par statut.
/// </summary>
public sealed record ListerEngagementsQuery(string CodeInsee, string? Thematique, string? Statut)
    : IQuery<IReadOnlyList<EngagementDto>?>;

internal sealed class ListerEngagementsQueryHandler(IProgrammeRepository programmes)
    : IQueryHandler<ListerEngagementsQuery, IReadOnlyList<EngagementDto>?>
{
    public async Task<IReadOnlyList<EngagementDto>?> HandleAsync(
        ListerEngagementsQuery query,
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

        StatutEngagement? statut = null;
        if (query.Statut is not null)
        {
            if (!Enum.TryParse<StatutEngagement>(query.Statut, ignoreCase: true, out var analyse))
            {
                return null;
            }

            statut = analyse;
        }

        var resultats = await programmes.ListerAsync(code, cancellationToken);

        var engagements = resultats
            .SelectMany(programme => programme.Engagements)
            .Where(engagement => thematique is null || engagement.Thematique == thematique)
            .Where(engagement => statut is null || engagement.Statut == statut);

        return [.. engagements.Select(engagement => engagement.VersDto())];
    }
}

/// <summary>Détail d'un engagement.</summary>
public sealed record ObtenirEngagementQuery(Guid Id) : IQuery<EngagementDto?>;

internal sealed class ObtenirEngagementQueryHandler(IProgrammeRepository programmes)
    : IQueryHandler<ObtenirEngagementQuery, EngagementDto?>
{
    public async Task<EngagementDto?> HandleAsync(
        ObtenirEngagementQuery query,
        CancellationToken cancellationToken)
    {
        var id = new EngagementId(query.Id);
        var programme = await programmes.ObtenirParEngagementAsync(id, cancellationToken);

        return programme?.Engagements.SingleOrDefault(engagement => engagement.Id == id)?.VersDto();
    }
}
