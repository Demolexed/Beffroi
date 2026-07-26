using Beffroi.Core.Domain.Common;

namespace Beffroi.Core.Domain.Elus;

/// <summary>
/// Une personne physique susceptible d'occuper un siège.
///
/// Séparée du siège à dessein : une même personne peut siéger sur plusieurs mandatures,
/// changer de fonction, ou siéger dans deux communes différentes au fil du temps.
/// Fusionner personne et mandat rendrait ces trajectoires inexprimables.
///
/// RGPD : minimisation stricte. Les élus sont des personnes publiques dans l'exercice de leur
/// mandat, ce qui ne justifie pas de stocker plus que ce dont la plateforme a besoin.
/// Le RNE expose aussi date de naissance, sexe et catégorie socio-professionnelle :
/// ces champs sont délibérément absents ici.
/// </summary>
public sealed class Personne
{
    private Personne(IdentifiantRne identifiant, string nom, string prenom)
    {
        Identifiant = identifiant;
        Nom = nom;
        Prenom = prenom;
    }

    /// <summary>Identité de l'entité : résout les homonymies à l'échelle nationale.</summary>
    public IdentifiantRne Identifiant { get; }

    public string Nom { get; }

    public string Prenom { get; }

    public static Personne Create(IdentifiantRne identifiant, string nom, string prenom)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(nom), "Le nom est obligatoire.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(prenom), "Le prénom est obligatoire.");
        return new Personne(identifiant, nom.Trim(), prenom.Trim());
    }

    public override string ToString() => $"{Prenom} {Nom}";
}
