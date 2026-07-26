namespace Beffroi.Core.Domain.Seances.Enums;

/// <summary>
/// Sort réservé à une délibération inscrite à l'ordre du jour.
/// « Unanimité » n'en fait pas partie : c'est une propriété du vote, pas du résultat.
/// </summary>
public enum ResultatDeliberation
{
    Adoptee,

    Rejetee,

    /// <summary>Retirée de l'ordre du jour avant examen.</summary>
    Retiree,

    /// <summary>Examen reporté à une séance ultérieure.</summary>
    Ajournee,

    /// <summary>Le conseil prend acte sans voter (rapports d'activité, communications).</summary>
    PriseDActe
}
