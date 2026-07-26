namespace Beffroi.Core.Domain.Conseils.Enums;

/// <summary>
/// Raison pour laquelle un siège a cessé d'être occupé.
/// Distinguer la fin normale des départs anticipés est ce qui permet, plus tard,
/// de restituer honnêtement la stabilité (ou l'instabilité) d'un conseil.
/// </summary>
public enum MotifDeFin
{
    /// <summary>Terme normal de la mandature.</summary>
    FinDeMandature,

    /// <summary>Démission volontaire (art. L2121-4 CGCT).</summary>
    Demission,

    /// <summary>Décès.</summary>
    Deces,

    /// <summary>Démission d'office, inéligibilité, annulation de l'élection.</summary>
    CessationDOffice,

    Autre
}
