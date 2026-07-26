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
    private Personne(PersonneId id, IdentifiantRne identifiant, string nom, string prenom)
    {
        Id = id;
        Identifiant = identifiant;
        Nom = nom;
        Prenom = prenom;
    }

    /// <summary>Identité technique : c'est elle que référencent présences, votes et sièges.</summary>
    public PersonneId Id { get; }

    /// <summary>
    /// Identifiant métier issu du répertoire national. Sert à dédupliquer entre deux imports,
    /// pas à lier les objets entre eux — ce rôle revient à <see cref="Id"/>.
    /// </summary>
    public IdentifiantRne Identifiant { get; }

    public string Nom { get; }

    public string Prenom { get; }

    public static Personne Create(IdentifiantRne identifiant, string nom, string prenom)
        => Create(PersonneId.New(), identifiant, nom, prenom);

    /// <summary>Reconstitue une personne dont l'identité est déjà connue.</summary>
    public static Personne Create(PersonneId id, IdentifiantRne identifiant, string nom, string prenom)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(nom), "Le nom est obligatoire.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(prenom), "Le prénom est obligatoire.");
        return new Personne(id, identifiant, nom.Trim(), prenom.Trim());
    }

    public override string ToString() => $"{Prenom} {Nom}";
}

/// <summary>Identité technique d'une <see cref="Personne"/>.</summary>
public readonly record struct PersonneId(Guid Valeur)
{
    public static PersonneId New() => new(Guid.CreateVersion7());

    public override string ToString() => Valeur.ToString();
}
