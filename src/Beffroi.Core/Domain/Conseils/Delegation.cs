using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Thematiques.Enums;

namespace Beffroi.Core.Domain.Conseils;

/// <summary>
/// Délégation confiée par le maire au titulaire d'un siège (art. L2122-18 CGCT).
///
/// Datée, car une délégation se retire en cours de mandat sans que le siège soit perdu —
/// et ce retrait est précisément le genre de fait qu'une plateforme de transparence doit
/// pouvoir restituer.
///
/// La <see cref="Thematiques.Enums.Thematique"/> rattache la délégation au même référentiel que les
/// délibérations : c'est ce qui permet de répondre « qui porte l'éducation dans cette commune,
/// et qu'a voté le conseil sur le sujet ».
/// </summary>
public sealed record Delegation
{
    private Delegation(Thematique thematique, string libelle, Period periode)
    {
        Thematique = thematique;
        Libelle = libelle;
        Periode = periode;
    }

    public Thematique Thematique { get; }

    /// <summary>Intitulé officiel, tel que porté par l'arrêté de délégation.</summary>
    public string Libelle { get; }

    public Period Periode { get; }

    public static Delegation Create(Thematique thematique, string libelle, Period periode)
    {
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(libelle),
            "Le libellé de la délégation est obligatoire.");

        return new Delegation(thematique, libelle.Trim(), periode);
    }

    public bool EstActiveAu(DateOnly date) => Periode.Contains(date);

    public override string ToString() => $"{Libelle} ({Periode})";
}
