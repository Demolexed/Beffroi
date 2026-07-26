using Beffroi.Core.Domain.Budgets.Enums;
using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Communes;
using Beffroi.Core.Domain.Thematiques.Enums;

namespace Beffroi.Core.Domain.Budgets;

/// <summary>
/// Racine d'agrégat : un document budgétaire voté par la commune pour un exercice.
///
/// Ce que la commune dépense est la moitié de ce qu'elle décide — l'autre moitié étant les
/// délibérations. Les deux se lisent ensemble : une promesse sans ligne budgétaire n'a pas
/// commencé à exister.
/// </summary>
public sealed class Budget
{
    private readonly List<LigneBudgetaire> _lignes = [];

    private Budget(BudgetId id, CodeInsee commune, int exercice, NatureDeBudget nature, Source source)
    {
        Id = id;
        Commune = commune;
        Exercice = exercice;
        Nature = nature;
        Source = source;
    }

    public BudgetId Id { get; }

    public CodeInsee Commune { get; }

    /// <summary>Année de l'exercice budgétaire.</summary>
    public int Exercice { get; }

    public NatureDeBudget Nature { get; }

    public Source Source { get; }

    public IReadOnlyList<LigneBudgetaire> Lignes => _lignes;

    /// <summary>Somme de toutes les lignes, ventilées ou non.</summary>
    public Montant Total => Montant.EnEuros(_lignes.Sum(ligne => ligne.Montant.Euros));

    /// <summary>
    /// Part des dépenses effectivement rattachée à une thématique. Inférieure à 100 % dès
    /// qu'une ligne échappe à la grille : à afficher, pas à masquer.
    /// </summary>
    public decimal PartVentilee
    {
        get
        {
            var total = Total.Euros;
            if (total == 0)
            {
                return 0;
            }

            var ventile = _lignes.Where(ligne => ligne.Thematique is not null).Sum(ligne => ligne.Montant.Euros);
            return ventile / total;
        }
    }

    public static Budget Voter(CodeInsee commune, int exercice, NatureDeBudget nature, Source source)
        => Voter(BudgetId.New(), commune, exercice, nature, source);

    /// <summary>Reconstitue un budget dont l'identité est déjà connue.</summary>
    public static Budget Voter(
        BudgetId id,
        CodeInsee commune,
        int exercice,
        NatureDeBudget nature,
        Source source)
    {
        DomainException.ThrowIf(exercice < 1800, $"Exercice budgétaire invraisemblable : {exercice}.");
        return new Budget(id, commune, exercice, nature, source);
    }

    public LigneBudgetaire Inscrire(LigneBudgetaire ligne)
    {
        DomainException.ThrowIf(
            _lignes.Any(existante => existante.Libelle.Equals(ligne.Libelle, StringComparison.OrdinalIgnoreCase)),
            $"La ligne « {ligne.Libelle} » est déjà inscrite à ce budget.");

        _lignes.Add(ligne);
        return ligne;
    }

    /// <summary>Total des lignes rattachées à une thématique donnée.</summary>
    public Montant TotalPour(Thematique thematique)
        => Montant.EnEuros(_lignes
            .Where(ligne => ligne.Thematique == thematique)
            .Sum(ligne => ligne.Montant.Euros));

    /// <summary>
    /// Part du budget consacrée à une thématique. Rapportée au total, y compris les lignes
    /// non ventilées : c'est la lecture honnête, « 16 % du budget » et non « 16 % de ce qu'on
    /// a su classer ».
    /// </summary>
    public decimal PartPour(Thematique thematique)
    {
        var total = Total.Euros;
        return total == 0 ? 0 : TotalPour(thematique).Euros / total;
    }

    /// <summary>
    /// Dépense par habitant. Nécessite la population : le budget ne la connaît pas,
    /// elle appartient à l'agrégat commune.
    /// </summary>
    public Montant ParHabitant(PopulationMunicipale population)
    {
        DomainException.ThrowIf(
            population.NombreHabitants <= 0,
            "Le montant par habitant n'a pas de sens sans population.");

        return Montant.EnEuros(Total.Euros / population.NombreHabitants);
    }

    public override string ToString() => $"{Nature} {Exercice} — {Commune} — {Total}";
}

/// <summary>Identité technique d'un <see cref="Budget"/>.</summary>
public readonly record struct BudgetId(Guid Valeur)
{
    public static BudgetId New() => new(Guid.CreateVersion7());

    public override string ToString() => Valeur.ToString();
}
