using Beffroi.Core.Domain.Common;

namespace Beffroi.Core.Domain.Conseils;

/// <summary>
/// Fonction exercée au titre d'un siège. Hiérarchie fermée : le constructeur privé
/// interdit toute variante déclarée ailleurs, ce qui rend le filtrage exhaustif.
/// </summary>
public abstract record Fonction
{
    private Fonction()
    {
    }

    /// <summary>Élu par le conseil municipal en son sein (art. L2122-4 CGCT). Un seul à la fois.</summary>
    public sealed record Maire : Fonction;

    /// <summary>
    /// Adjoint au maire. Le rang n'est pas décoratif : il fixe l'ordre de suppléance
    /// (le premier adjoint remplace le maire empêché).
    /// </summary>
    public sealed record Adjoint : Fonction
    {
        public Adjoint(int rang)
        {
            DomainException.ThrowIf(rang < 1, $"Le rang d'un adjoint commence à 1, reçu {rang}.");
            Rang = rang;
        }

        public int Rang { get; }
    }

    /// <summary>
    /// Conseiller sans fonction exécutive.
    /// Note : le « conseiller délégué » n'est pas une fonction distincte — c'est un conseiller
    /// municipal titulaire d'une délégation du maire. À modéliser séparément si le besoin apparaît.
    /// </summary>
    public sealed record ConseillerMunicipal : Fonction;
}
