using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Contracts;
using Beffroi.Core.Application.Programmes;
using Microsoft.AspNetCore.Mvc;

namespace Beffroi.Api.Controllers;

/// <summary>Programmes électoraux d'une commune.</summary>
[ApiController]
[Route("api/v1/communes/{codeInsee}/programmes")]
public sealed class ProgrammesController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>Programmes connus, engagements compris.</summary>
    /// <response code="404">Code INSEE inconnu ou mal formé.</response>
    [HttpGet(Name = "ListerProgrammes")]
    [ProducesResponseType<IReadOnlyList<ProgrammeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProgrammeDto>>> Lister(
        string codeInsee,
        CancellationToken cancellationToken)
    {
        var programmes = await dispatcher.QueryAsync(new ListerProgrammesQuery(codeInsee), cancellationToken);
        return programmes is null ? NotFound() : Ok(programmes);
    }
}

/// <summary>Engagements de campagne et état de leur réalisation.</summary>
[ApiController]
[Route("api/v1/communes/{codeInsee}/engagements")]
public sealed class EngagementsParCommuneController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>Engagements de la commune, tous programmes confondus.</summary>
    /// <param name="codeInsee">Code INSEE sur 5 caractères.</param>
    /// <param name="thematique">Code de thématique, par exemple « education ».</param>
    /// <param name="statut">Realise, PartiellementRealise, VoteNonRealise ou SansTrace.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="404">Code INSEE, thématique ou statut inconnus.</response>
    [HttpGet(Name = "ListerEngagements")]
    [ProducesResponseType<IReadOnlyList<EngagementDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<EngagementDto>>> Lister(
        string codeInsee,
        [FromQuery] string? thematique,
        [FromQuery] string? statut,
        CancellationToken cancellationToken)
    {
        var engagements = await dispatcher.QueryAsync(
            new ListerEngagementsQuery(codeInsee, thematique, statut), cancellationToken);

        return engagements is null ? NotFound() : Ok(engagements);
    }
}

/// <summary>Détail d'un engagement.</summary>
[ApiController]
[Route("api/v1/engagements")]
public sealed class EngagementsController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>
    /// Engagement, son statut et les délibérations sur lesquelles le constat s'appuie.
    /// </summary>
    /// <response code="404">Engagement inconnu.</response>
    [HttpGet("{id:guid}", Name = "ObtenirEngagement")]
    [ProducesResponseType<EngagementDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EngagementDto>> Obtenir(Guid id, CancellationToken cancellationToken)
    {
        var engagement = await dispatcher.QueryAsync(new ObtenirEngagementQuery(id), cancellationToken);
        return engagement is null ? NotFound() : Ok(engagement);
    }
}
