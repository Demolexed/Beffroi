using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Thematiques.Enums;

namespace Beffroi.Core.Domain.Budgets;

/// <summary>
/// Une ligne du document budgétaire, telle qu'elle y figure.
///
/// <see cref="Thematique"/> est <b>volontairement optionnelle</b> : la nomenclature
/// fonctionnelle des collectivités ne se superpose pas aux sept thématiques de Beffroi.
/// Une ligne « action sociale » ou « administration générale » n'a pas d'équivalent dans la
/// grille, et forcer un rattachement fabriquerait de la donnée. Le total du budget reste
/// juste dans tous les cas, seule la ventilation par thématique est partielle — et doit
/// être présentée comme telle.
/// </summary>
public sealed record LigneBudgetaire
{
    private LigneBudgetaire(string libelle, Montant montant, Thematique? thematique)
    {
        Libelle = libelle;
        Montant = montant;
        Thematique = thematique;
    }

    /// <summary>Libellé tel qu'il figure au document, sans reformulation.</summary>
    public string Libelle { get; }

    public Montant Montant { get; }

    /// <summary>Thématique Beffroi rattachée, quand la ligne en admet une.</summary>
    public Thematique? Thematique { get; }

    public static LigneBudgetaire Create(string libelle, Montant montant, Thematique? thematique = null)
    {
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(libelle),
            "Le libellé d'une ligne budgétaire est obligatoire.");

        DomainException.ThrowIf(
            montant.Euros == 0,
            $"La ligne « {libelle} » est à zéro : ne pas créer de ligne vide.");

        return new LigneBudgetaire(libelle.Trim(), montant, thematique);
    }

    public override string ToString() => $"{Libelle} — {Montant}";
}
