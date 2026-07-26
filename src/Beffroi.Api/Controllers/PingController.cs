using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Beffroi.Api.Controllers;

/// <summary>
/// Diagnostic de l'API.
/// </summary>
/// <remarks>
/// Le contrôleur ne contient aucune logique : il traduit une requête HTTP en message CQRS
/// et rend le résultat. C'est le rôle d'un adapter primaire.
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class PingController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>
    /// Vérifie que l'API répond et que la chaîne contrôleur → dispatcher → handler → port est câblée.
    /// </summary>
    /// <response code="200">L'API et le cœur applicatif répondent.</response>
    [HttpGet(Name = "Ping")]
    [ProducesResponseType<PingResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PingResponse>> Get(CancellationToken cancellationToken)
    {
        var response = await dispatcher.QueryAsync(new PingQuery(), cancellationToken);
        return Ok(response);
    }
}
