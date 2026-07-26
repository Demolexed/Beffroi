using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Contracts;
using Beffroi.Core.Application.Seances;
using Microsoft.AspNetCore.Mvc;

namespace Beffroi.Api.Controllers;

/// <summary>Séances du conseil municipal, rattachées à une commune.</summary>
[ApiController]
[Route("api/v1/communes/{codeInsee}/seances")]
public sealed class SeancesParCommuneController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>Séances de la commune, de la plus récente à la plus ancienne.</summary>
    /// <response code="404">Code INSEE inconnu ou mal formé.</response>
    [HttpGet(Name = "ListerSeances")]
    [ProducesResponseType<IReadOnlyList<SeanceDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SeanceDto>>> Lister(
        string codeInsee,
        [FromQuery] DateOnly? depuis,
        [FromQuery] DateOnly? jusqua,
        CancellationToken cancellationToken)
    {
        var seances = await dispatcher.QueryAsync(
            new ListerSeancesQuery(codeInsee, depuis, jusqua), cancellationToken);

        return seances is null ? NotFound() : Ok(seances);
    }
}

/// <summary>Détail d'une séance.</summary>
[ApiController]
[Route("api/v1/seances")]
public sealed class SeancesController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>
    /// Séance, ses délibérations, ses présences et l'état de publication de son procès-verbal.
    /// </summary>
    /// <response code="404">Séance inconnue.</response>
    [HttpGet("{id:guid}", Name = "ObtenirSeance")]
    [ProducesResponseType<SeanceDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeanceDetailDto>> Obtenir(Guid id, CancellationToken cancellationToken)
    {
        var seance = await dispatcher.QueryAsync(new ObtenirSeanceQuery(id), cancellationToken);
        return seance is null ? NotFound() : Ok(seance);
    }
}
