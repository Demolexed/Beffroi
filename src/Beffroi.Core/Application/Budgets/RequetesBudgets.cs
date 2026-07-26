using Beffroi.Core.Application.Abstractions.Messaging;
using Beffroi.Core.Application.Communes;
using Beffroi.Core.Application.Contracts;
using Beffroi.Core.Application.Ports;

namespace Beffroi.Core.Application.Budgets;

/// <summary>Exercices budgétaires connus pour une commune, du plus récent au plus ancien.</summary>
public sealed record ListerBudgetsQuery(string CodeInsee) : IQuery<IReadOnlyList<BudgetSommaireDto>?>;

internal sealed class ListerBudgetsQueryHandler(IBudgetRepository budgets)
    : IQueryHandler<ListerBudgetsQuery, IReadOnlyList<BudgetSommaireDto>?>
{
    public async Task<IReadOnlyList<BudgetSommaireDto>?> HandleAsync(
        ListerBudgetsQuery query,
        CancellationToken cancellationToken)
    {
        if (!CodeInseeValide.Essayer(query.CodeInsee, out var code))
        {
            return null;
        }

        var resultats = await budgets.ListerAsync(code, cancellationToken);
        return [.. resultats.Select(budget => budget.VersSommaireDto())];
    }
}

/// <summary>
/// Détail d'un budget : ses lignes, leur part, et la dépense par habitant.
/// La population vient de l'agrégat commune, d'où la double lecture.
/// </summary>
public sealed record ObtenirBudgetQuery(string CodeInsee, int Exercice) : IQuery<BudgetDto?>;

internal sealed class ObtenirBudgetQueryHandler(IBudgetRepository budgets, ICommuneRepository communes)
    : IQueryHandler<ObtenirBudgetQuery, BudgetDto?>
{
    public async Task<BudgetDto?> HandleAsync(ObtenirBudgetQuery query, CancellationToken cancellationToken)
    {
        if (!CodeInseeValide.Essayer(query.CodeInsee, out var code))
        {
            return null;
        }

        var budget = await budgets.ObtenirAsync(code, query.Exercice, cancellationToken);
        if (budget is null)
        {
            return null;
        }

        var commune = await communes.ObtenirParCodeAsync(code, cancellationToken);
        return budget.VersDto(commune?.Population);
    }
}
