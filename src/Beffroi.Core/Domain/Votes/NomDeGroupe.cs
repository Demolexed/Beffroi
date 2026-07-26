using Beffroi.Core.Domain.Common;

namespace Beffroi.Core.Domain.Votes;

/// <summary>
/// Nom du groupe auquel un décompte de voix est attribué, tel qu'il figure au procès-verbal.
///
/// Une chaîne, et non une référence à une <c>ListeElectorale</c>, pour deux raisons :
/// le décompte appartient à l'agrégat séance et ne doit pas dépendre de l'agrégat conseil ;
/// et un élu peut changer de groupe en cours de mandat sans que sa liste d'élection bouge.
/// Le rapprochement groupe ↔ liste est une opération de lecture, pas une contrainte du modèle.
/// </summary>
public sealed record NomDeGroupe
{
    private NomDeGroupe(string valeur) => Valeur = valeur;

    public string Valeur { get; }

    public static NomDeGroupe Create(string valeur)
    {
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(valeur),
            "Le nom du groupe est obligatoire pour attribuer un décompte de voix.");

        return new NomDeGroupe(valeur.Trim());
    }

    public override string ToString() => Valeur;
}
