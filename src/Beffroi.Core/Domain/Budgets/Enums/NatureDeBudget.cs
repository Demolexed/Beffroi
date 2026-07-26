namespace Beffroi.Core.Domain.Budgets.Enums;

/// <summary>
/// Nature d'un document budgétaire. La distinction n'est pas comptable mais éditoriale :
/// le budget primitif dit ce que la commune <b>prévoit</b> de dépenser, le compte administratif
/// ce qu'elle a <b>réellement</b> dépensé. Les confondre reviendrait à présenter une intention
/// comme un fait accompli.
/// </summary>
public enum NatureDeBudget
{
    /// <summary>Prévision votée en début d'exercice.</summary>
    Primitif,

    /// <summary>Modification en cours d'exercice.</summary>
    BudgetSupplementaire,

    /// <summary>Exécution constatée, votée après la clôture de l'exercice.</summary>
    CompteAdministratif
}
