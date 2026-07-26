using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Contracts;
using Beffroi.Core.Application.Deliberations;
using Microsoft.AspNetCore.Mvc;

namespace Beffroi.Api.Controllers;

/// <summary>Délibérations d'une commune.</summary>
[ApiController]
[Route("api/v1/communes/{codeInsee}/deliberations")]
public sealed class DeliberationsParCommuneController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>Délibérations de la commune, filtrables.</summary>
    /// <param name="codeInsee">Code INSEE sur 5 caractères.</param>
    /// <param name="thematique">Code de thématique, par exemple « transports-et-voirie ».</param>
    /// <param name="depuis">Borne basse sur la date de séance.</param>
    /// <param name="jusqua">Borne haute sur la date de séance.</param>
    /// <param name="nonUnanimes">Ne retenir que les décisions ayant recueilli des voix contre ou des abstentions.</param>
    /// <param name="limite">Nombre maximal de résultats, les plus récentes d'abord. Entre 1 et 200.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="404">Code INSEE, thématique ou limite invalides.</response>
    [HttpGet(Name = "ListerDeliberations")]
    [ProducesResponseType<IReadOnlyList<DeliberationDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DeliberationDto>>> Lister(
        string codeInsee,
        [FromQuery] string? thematique,
        [FromQuery] DateOnly? depuis,
        [FromQuery] DateOnly? jusqua,
        [FromQuery] bool? nonUnanimes,
        [FromQuery] int? limite,
        CancellationToken cancellationToken)
    {
        var deliberations = await dispatcher.QueryAsync(
            new ListerDeliberationsQuery(codeInsee, thematique, depuis, jusqua, nonUnanimes, limite),
            cancellationToken);

        return deliberations is null ? NotFound() : Ok(deliberations);
    }
}

/// <summary>Détail d'une délibération.</summary>
[ApiController]
[Route("api/v1/deliberations")]
public sealed class DeliberationsController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>Délibération, sa reformulation éventuelle et le décompte des voix.</summary>
    /// <response code="404">Délibération inconnue.</response>
    [HttpGet("{id:guid}", Name = "ObtenirDeliberation")]
    [ProducesResponseType<DeliberationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeliberationDto>> Obtenir(Guid id, CancellationToken cancellationToken)
    {
        var deliberation = await dispatcher.QueryAsync(new ObtenirDeliberationQuery(id), cancellationToken);
        return deliberation is null ? NotFound() : Ok(deliberation);
    }
}
