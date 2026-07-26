using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Communes;
using Beffroi.Core.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Beffroi.Api.Controllers;

/// <summary>
/// Communes. Identifiées par leur code INSEE : lisible, stable, citable.
/// </summary>
[ApiController]
[Route("api/v1/communes")]
public sealed class CommunesController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>Recherche des communes par nom ou par début de code INSEE.</summary>
    [HttpGet(Name = "RechercherCommunes")]
    [ProducesResponseType<IReadOnlyList<CommuneDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CommuneDto>>> Rechercher(
        [FromQuery] string? recherche,
        CancellationToken cancellationToken)
        => Ok(await dispatcher.QueryAsync(new RechercherCommunesQuery(recherche), cancellationToken));

    /// <summary>Fiche d'une commune.</summary>
    /// <param name="codeInsee">Code INSEE sur 5 caractères, par exemple 92072.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="404">Code INSEE inconnu ou mal formé.</response>
    [HttpGet("{codeInsee}", Name = "ObtenirCommune")]
    [ProducesResponseType<CommuneDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommuneDto>> Obtenir(string codeInsee, CancellationToken cancellationToken)
    {
        var commune = await dispatcher.QueryAsync(new ObtenirCommuneQuery(codeInsee), cancellationToken);
        return commune is null ? NotFound() : Ok(commune);
    }
}
