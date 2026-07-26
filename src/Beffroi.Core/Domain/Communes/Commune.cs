using Beffroi.Core.Domain.Common;

namespace Beffroi.Core.Domain.Communes;

/// <summary>
/// Racine d'agrégat : identité administrative d'une commune.
///
/// Volontairement pauvre. La commune est un point d'ancrage stable dans le temps ;
/// tout ce qui varie (conseil, budgets, délibérations) vit dans d'autres agrégats
/// qui la référencent par son <see cref="CodeInsee"/>.
/// </summary>
public sealed class Commune
{
    private Commune(CodeInsee code, string nom, PopulationMunicipale population)
    {
        Code = code;
        Nom = nom;
        Population = population;
    }

    public CodeInsee Code { get; }

    public string Nom { get; }

    /// <summary>
    /// Population municipale légale. Détermine l'effectif du conseil (art. L2121-2 CGCT),
    /// d'où la nécessité de conserver son millésime : un effectif se justifie par un chiffre daté.
    /// </summary>
    public PopulationMunicipale Population { get; }

    public static Commune Create(CodeInsee code, string nom, PopulationMunicipale population)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(nom), "Le nom de la commune est obligatoire.");
        return new Commune(code, nom.Trim(), population);
    }

    public override string ToString() => $"{Nom} ({Code})";
}

/// <summary>
/// Chiffre de population légale et son millésime de référence.
/// </summary>
public sealed record PopulationMunicipale
{
    private PopulationMunicipale(int nombreHabitants, int millesime)
    {
        NombreHabitants = nombreHabitants;
        Millesime = millesime;
    }

    public int NombreHabitants { get; }

    /// <summary>Année de référence du recensement (les populations légales sont millésimées).</summary>
    public int Millesime { get; }

    public static PopulationMunicipale Create(int nombreHabitants, int millesime)
    {
        DomainException.ThrowIf(nombreHabitants < 0, "Une population ne peut pas être négative.");
        DomainException.ThrowIf(millesime < 1800, $"Millésime de recensement invraisemblable : {millesime}.");
        return new PopulationMunicipale(nombreHabitants, millesime);
    }
}
