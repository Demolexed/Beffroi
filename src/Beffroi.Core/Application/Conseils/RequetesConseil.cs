using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Communes;
using Beffroi.Core.Application.Contracts;
using Beffroi.Core.Application.Ports;

namespace Beffroi.Core.Application.Conseils;

/// <summary>
/// Conseil municipal à une date donnée. <c>Au</c> absent signifie « aujourd'hui ».
///
/// C'est la requête signature de Beffroi : elle répond « qui siégeait ce jour-là »,
/// démissions et remplacements compris.
/// </summary>
public sealed record ObtenirConseilQuery(string CodeInsee, DateOnly? Au) : IQuery<ConseilDto?>;

internal sealed class ObtenirConseilQueryHandler(IConseilMunicipalRepository conseils, IClock horloge)
    : IQueryHandler<ObtenirConseilQuery, ConseilDto?>
{
    public async Task<ConseilDto?> HandleAsync(ObtenirConseilQuery query, CancellationToken cancellationToken)
    {
        if (!CodeInseeValide.Essayer(query.CodeInsee, out var code))
        {
            return null;
        }

        var au = query.Au ?? DateOnly.FromDateTime(horloge.UtcNow.UtcDateTime);
        var conseil = await conseils.ObtenirAsync(code, au, cancellationToken);
        return conseil?.VersDto(au);
    }
}

/// <summary>Sièges occupés à une date donnée.</summary>
public sealed record ListerSiegesQuery(string CodeInsee, DateOnly? Au) : IQuery<IReadOnlyList<SiegeDto>?>;

internal sealed class ListerSiegesQueryHandler(IConseilMunicipalRepository conseils, IClock horloge)
    : IQueryHandler<ListerSiegesQuery, IReadOnlyList<SiegeDto>?>
{
    public async Task<IReadOnlyList<SiegeDto>?> HandleAsync(
        ListerSiegesQuery query,
        CancellationToken cancellationToken)
    {
        if (!CodeInseeValide.Essayer(query.CodeInsee, out var code))
        {
            return null;
        }

        var au = query.Au ?? DateOnly.FromDateTime(horloge.UtcNow.UtcDateTime);
        var conseil = await conseils.ObtenirAsync(code, au, cancellationToken);

        return conseil is null
            ? null
            : [.. conseil.CompositionAu(au).Select(siege => siege.VersDto(conseil, au))];
    }
}

/// <summary>Listes ayant obtenu des sièges sur la mandature couvrant la date demandée.</summary>
public sealed record ListerListesQuery(string CodeInsee, DateOnly? Au) : IQuery<IReadOnlyList<ListeElectoraleDto>?>;

internal sealed class ListerListesQueryHandler(IConseilMunicipalRepository conseils, IClock horloge)
    : IQueryHandler<ListerListesQuery, IReadOnlyList<ListeElectoraleDto>?>
{
    public async Task<IReadOnlyList<ListeElectoraleDto>?> HandleAsync(
        ListerListesQuery query,
        CancellationToken cancellationToken)
    {
        if (!CodeInseeValide.Essayer(query.CodeInsee, out var code))
        {
            return null;
        }

        var au = query.Au ?? DateOnly.FromDateTime(horloge.UtcNow.UtcDateTime);
        var conseil = await conseils.ObtenirAsync(code, au, cancellationToken);

        return conseil is null
            ? null
            : [.. conseil.Listes.Select(liste => liste.VersDto(conseil, au))];
    }
}

/// <summary>Historique des mandatures connues, de la plus récente à la plus ancienne.</summary>
public sealed record ListerMandaturesQuery(string CodeInsee) : IQuery<IReadOnlyList<MandatureDto>?>;

internal sealed class ListerMandaturesQueryHandler(IConseilMunicipalRepository conseils)
    : IQueryHandler<ListerMandaturesQuery, IReadOnlyList<MandatureDto>?>
{
    public async Task<IReadOnlyList<MandatureDto>?> HandleAsync(
        ListerMandaturesQuery query,
        CancellationToken cancellationToken)
    {
        if (!CodeInseeValide.Essayer(query.CodeInsee, out var code))
        {
            return null;
        }

        var mandatures = await conseils.ListerMandaturesAsync(code, cancellationToken);
        return mandatures.Count == 0 ? null : [.. mandatures.Select(conseil => conseil.VersDto())];
    }
}
