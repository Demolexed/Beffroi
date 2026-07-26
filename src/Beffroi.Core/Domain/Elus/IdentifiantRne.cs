using Beffroi.Core.Domain.Common;

namespace Beffroi.Core.Domain.Elus;

/// <summary>
/// Identifiant stable d'une personne dans le Répertoire National des Élus.
/// </summary>
/// <remarks>
/// ⚠️ À CONFIRMER avant le premier import réel : il n'est pas acquis que le RNE publie
/// un identifiant stable par élu dans ses exports ouverts. Si ce n'est pas le cas, deux
/// options — et le choix ne doit pas être fait implicitement :
///   1. dériver un identifiant déterministe (commune + nom + prénom + date de début de mandat) ;
///   2. faire porter l'identité à la personne par la commune et le nom, en acceptant
///      l'homonymie nationale.
/// Le type existe pour que ce choix reste localisé à un seul endroit du code.
/// </remarks>
public sealed record IdentifiantRne
{
    private IdentifiantRne(string valeur) => Valeur = valeur;

    public string Valeur { get; }

    public static IdentifiantRne Create(string valeur)
    {
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(valeur),
            "L'identifiant RNE est obligatoire pour identifier une personne.");

        return new IdentifiantRne(valeur.Trim());
    }

    public override string ToString() => Valeur;
}
