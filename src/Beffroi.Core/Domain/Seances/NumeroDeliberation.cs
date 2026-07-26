using Beffroi.Core.Domain.Common;

namespace Beffroi.Core.Domain.Seances;

/// <summary>
/// Numéro d'une délibération : une année et un rang, par exemple « 2026/051 ».
/// Le rang repart à 1 chaque année — le numéro n'est donc unique qu'au sein de son millésime,
/// jamais globalement.
/// </summary>
public sealed record NumeroDeliberation
{
    private NumeroDeliberation(int annee, int rang)
    {
        Annee = annee;
        Rang = rang;
    }

    public int Annee { get; }

    public int Rang { get; }

    public static NumeroDeliberation Create(int annee, int rang)
    {
        DomainException.ThrowIf(annee < 1800, $"Millésime de délibération invraisemblable : {annee}.");
        DomainException.ThrowIf(rang < 1, "Le rang d'une délibération commence à 1.");
        return new NumeroDeliberation(annee, rang);
    }

    public override string ToString() => $"{Annee}/{Rang:D3}";
}
