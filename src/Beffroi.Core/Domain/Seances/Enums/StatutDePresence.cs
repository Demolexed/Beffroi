namespace Beffroi.Core.Domain.Seances.Enums;

/// <summary>
/// Statut d'un élu à une séance, tel que constaté à l'appel.
/// Reprend les rubriques que les procès-verbaux emploient réellement :
/// PRÉSENTS, AVAIENT DONNÉ POUVOIR, ÉTAIT EXCUSÉ.
/// </summary>
public enum StatutDePresence
{
    Present,

    /// <summary>Absent mais représenté par un mandataire (art. L2121-20 CGCT).</summary>
    Represente,

    /// <summary>Absence excusée, sans pouvoir donné.</summary>
    Excuse,

    Absent
}
