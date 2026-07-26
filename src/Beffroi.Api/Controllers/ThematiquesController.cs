using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Contracts;
using Beffroi.Core.Application.Thematiques;
using Microsoft.AspNetCore.Mvc;

namespace Beffroi.Api.Controllers;

/// <summary>Référentiel thématique, commun à toutes les communes.</summary>
[ApiController]
[Route("api/v1/thematiques")]
public sealed class ThematiquesController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>Les sept thématiques et leur code d'URL.</summary>
    [HttpGet(Name = "ListerThematiques")]
    [ProducesResponseType<IReadOnlyList<ThematiqueDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ThematiqueDto>>> Lister(CancellationToken cancellationToken)
        => Ok(await dispatcher.QueryAsync(new ListerThematiquesQuery(), cancellationToken));
}

/// <summary>Les sept thématiques vues depuis une commune.</summary>
[ApiController]
[Route("api/v1/communes/{codeInsee}/thematiques")]
public sealed class ThematiquesParCommuneController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>
    /// Pour chaque thématique : son poids dans le dernier budget connu et la dernière décision
    /// du conseil dans ce domaine.
    /// </summary>
    /// <remarks>
    /// Les sept sont toujours renvoyées, y compris vides. Une thématique sans décision est une
    /// information en soi, l'escamoter donnerait une image incomplète de l'action municipale.
    /// </remarks>
    /// <response code="404">Code INSEE inconnu ou mal formé.</response>
    [HttpGet(Name = "ListerThematiquesCommunales")]
    [ProducesResponseType<IReadOnlyList<ThematiqueCommunaleDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ThematiqueCommunaleDto>>> Lister(
        string codeInsee,
        CancellationToken cancellationToken)
    {
        var thematiques = await dispatcher.QueryAsync(
            new ListerThematiquesCommunalesQuery(codeInsee), cancellationToken);

        return thematiques is null ? NotFound() : Ok(thematiques);
    }
}
