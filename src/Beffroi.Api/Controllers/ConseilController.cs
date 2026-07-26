using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Conseils;
using Beffroi.Core.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Beffroi.Api.Controllers;

/// <summary>
/// Conseil municipal d'une commune.
/// </summary>
/// <remarks>
/// Toutes les lectures acceptent <c>?au=</c> : c'est ce qui permet de répondre
/// « qui siégeait ce jour-là », démissions et remplacements compris. Sans ce paramètre,
/// la réponse porte sur la date du jour.
/// </remarks>
[ApiController]
[Route("api/v1/communes/{codeInsee}")]
public sealed class ConseilController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>Conseil municipal en fonction à la date demandée.</summary>
    /// <response code="404">Commune inconnue, ou aucune mandature ne couvre cette date.</response>
    [HttpGet("conseil", Name = "ObtenirConseil")]
    [ProducesResponseType<ConseilDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConseilDto>> Obtenir(
        string codeInsee,
        [FromQuery] DateOnly? au,
        CancellationToken cancellationToken)
    {
        var conseil = await dispatcher.QueryAsync(new ObtenirConseilQuery(codeInsee, au), cancellationToken);
        return conseil is null ? NotFound() : Ok(conseil);
    }

    /// <summary>Sièges occupés à la date demandée.</summary>
    /// <response code="404">Commune inconnue, ou aucune mandature ne couvre cette date.</response>
    [HttpGet("conseil/sieges", Name = "ListerSieges")]
    [ProducesResponseType<IReadOnlyList<SiegeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SiegeDto>>> ListerSieges(
        string codeInsee,
        [FromQuery] DateOnly? au,
        CancellationToken cancellationToken)
    {
        var sieges = await dispatcher.QueryAsync(new ListerSiegesQuery(codeInsee, au), cancellationToken);
        return sieges is null ? NotFound() : Ok(sieges);
    }

    /// <summary>Listes ayant obtenu des sièges sur la mandature couvrant la date demandée.</summary>
    /// <response code="404">Commune inconnue, ou aucune mandature ne couvre cette date.</response>
    [HttpGet("conseil/listes", Name = "ListerListes")]
    [ProducesResponseType<IReadOnlyList<ListeElectoraleDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ListeElectoraleDto>>> ListerListes(
        string codeInsee,
        [FromQuery] DateOnly? au,
        CancellationToken cancellationToken)
    {
        var listes = await dispatcher.QueryAsync(new ListerListesQuery(codeInsee, au), cancellationToken);
        return listes is null ? NotFound() : Ok(listes);
    }

    /// <summary>Mandatures connues, de la plus récente à la plus ancienne.</summary>
    /// <response code="404">Commune inconnue, ou aucune mandature enregistrée.</response>
    [HttpGet("mandatures", Name = "ListerMandatures")]
    [ProducesResponseType<IReadOnlyList<MandatureDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MandatureDto>>> ListerMandatures(
        string codeInsee,
        CancellationToken cancellationToken)
    {
        var mandatures = await dispatcher.QueryAsync(new ListerMandaturesQuery(codeInsee), cancellationToken);
        return mandatures is null ? NotFound() : Ok(mandatures);
    }
}
