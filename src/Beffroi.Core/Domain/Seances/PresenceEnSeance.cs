using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Elus;
using Beffroi.Core.Domain.Seances.Enums;

namespace Beffroi.Core.Domain.Seances;

/// <summary>
/// Constat de présence d'un élu à une séance.
/// </summary>
public sealed record PresenceEnSeance
{
    private PresenceEnSeance(PersonneId elu, StatutDePresence statut, PersonneId? pouvoirDonneA)
    {
        Elu = elu;
        Statut = statut;
        PouvoirDonneA = pouvoirDonneA;
    }

    public PersonneId Elu { get; }

    public StatutDePresence Statut { get; }

    /// <summary>Mandataire à qui le pouvoir a été donné, si l'élu est représenté.</summary>
    public PersonneId? PouvoirDonneA { get; }

    public static PresenceEnSeance Present(PersonneId elu)
        => new(elu, StatutDePresence.Present, null);

    public static PresenceEnSeance Represente(PersonneId elu, PersonneId mandataire)
    {
        DomainException.ThrowIf(
            elu == mandataire,
            "Un élu ne peut pas se donner pouvoir à lui-même.");

        return new PresenceEnSeance(elu, StatutDePresence.Represente, mandataire);
    }

    public static PresenceEnSeance Excuse(PersonneId elu) => new(elu, StatutDePresence.Excuse, null);

    public static PresenceEnSeance Absent(PersonneId elu) => new(elu, StatutDePresence.Absent, null);

    /// <summary>Compte pour le quorum : seule la présence physique compte (art. L2121-17 CGCT).</summary>
    public bool CompteDansLeQuorum => Statut == StatutDePresence.Present;
}
