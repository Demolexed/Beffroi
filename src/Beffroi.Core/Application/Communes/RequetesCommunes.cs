using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Contracts;
using Beffroi.Core.Application.Ports;
using Beffroi.Core.Domain.Communes;

namespace Beffroi.Core.Application.Communes;

/// <summary>Recherche de communes par nom ou par code INSEE.</summary>
public sealed record RechercherCommunesQuery(string? Recherche) : IQuery<IReadOnlyList<CommuneDto>>;

internal sealed class RechercherCommunesQueryHandler(ICommuneRepository communes)
    : IQueryHandler<RechercherCommunesQuery, IReadOnlyList<CommuneDto>>
{
    public async Task<IReadOnlyList<CommuneDto>> HandleAsync(
        RechercherCommunesQuery query,
        CancellationToken cancellationToken)
    {
        var resultats = await communes.RechercherAsync(query.Recherche, cancellationToken);
        return [.. resultats.Select(commune => commune.VersDto())];
    }
}

/// <summary>Fiche d'une commune. <c>null</c> si le code INSEE est inconnu ou mal formé.</summary>
public sealed record ObtenirCommuneQuery(string CodeInsee) : IQuery<CommuneDto?>;

internal sealed class ObtenirCommuneQueryHandler(ICommuneRepository communes)
    : IQueryHandler<ObtenirCommuneQuery, CommuneDto?>
{
    public async Task<CommuneDto?> HandleAsync(ObtenirCommuneQuery query, CancellationToken cancellationToken)
    {
        if (!CodeInseeValide.Essayer(query.CodeInsee, out var code))
        {
            return null;
        }

        var commune = await communes.ObtenirParCodeAsync(code, cancellationToken);
        return commune?.VersDto();
    }
}

/// <summary>
/// Analyse tolérante d'un code INSEE venu d'une URL : un code mal formé donne un 404,
/// pas une erreur 500. La validation stricte reste dans le domaine.
/// </summary>
public static class CodeInseeValide
{
    public static bool Essayer(string? valeur, out CodeInsee code)
    {
        try
        {
            code = CodeInsee.Create(valeur ?? string.Empty);
            return true;
        }
        catch (Domain.Common.DomainException)
        {
            code = null!;
            return false;
        }
    }
}
