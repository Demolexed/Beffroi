using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Budgets;
using Beffroi.Core.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Beffroi.Api.Controllers;

/// <summary>Budgets votés par une commune.</summary>
[ApiController]
[Route("api/v1/communes/{codeInsee}/budgets")]
public sealed class BudgetsController(IDispatcher dispatcher) : ControllerBase
{
    /// <summary>Exercices budgétaires connus, du plus récent au plus ancien.</summary>
    /// <response code="404">Code INSEE inconnu ou mal formé.</response>
    [HttpGet(Name = "ListerBudgets")]
    [ProducesResponseType<IReadOnlyList<BudgetSommaireDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<BudgetSommaireDto>>> Lister(
        string codeInsee,
        CancellationToken cancellationToken)
    {
        var budgets = await dispatcher.QueryAsync(new ListerBudgetsQuery(codeInsee), cancellationToken);
        return budgets is null ? NotFound() : Ok(budgets);
    }

    /// <summary>
    /// Détail d'un exercice : lignes, part de chacune, total et dépense par habitant.
    /// </summary>
    /// <remarks>
    /// <c>partVentilee</c> indique quelle proportion du budget a pu être rattachée aux sept
    /// thématiques. Elle est inférieure à 1 dès qu'une ligne y échappe — c'est une limite de
    /// la nomenclature, pas une erreur.
    /// </remarks>
    /// <response code="404">Commune inconnue, ou aucun budget pour cet exercice.</response>
    [HttpGet("{exercice:int}", Name = "ObtenirBudget")]
    [ProducesResponseType<BudgetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BudgetDto>> Obtenir(
        string codeInsee,
        int exercice,
        CancellationToken cancellationToken)
    {
        var budget = await dispatcher.QueryAsync(new ObtenirBudgetQuery(codeInsee, exercice), cancellationToken);
        return budget is null ? NotFound() : Ok(budget);
    }
}
