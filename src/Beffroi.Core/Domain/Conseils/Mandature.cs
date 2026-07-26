using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Communes;

namespace Beffroi.Core.Domain.Conseils;

/// <summary>
/// Un mandat municipal complet pour une commune : la période et l'effectif légal du conseil.
/// Durée de droit commun : six ans (art. L227 du code électoral).
/// </summary>
public sealed class Mandature
{
    private Mandature(CodeInsee commune, Period periode, int effectifLegal)
    {
        Commune = commune;
        Periode = periode;
        EffectifLegal = effectifLegal;
    }

    public CodeInsee Commune { get; }

    public Period Periode { get; }

    /// <summary>Nombre de sièges que compte le conseil municipal.</summary>
    public int EffectifLegal { get; }

    /// <summary>
    /// Nombre maximal d'adjoints : 30 % de l'effectif légal, arrondi à l'entier inférieur
    /// (art. L2122-2 CGCT).
    /// </summary>
    public int NombreMaximalDAdjoints => (int)Math.Floor(EffectifLegal * 0.30);

    /// <summary>
    /// Constitue une mandature en déduisant l'effectif légal de la population.
    /// </summary>
    public static Mandature Create(CodeInsee commune, Period periode, PopulationMunicipale population)
        => new(commune, periode, EffectifLegalPour(population.NombreHabitants));

    /// <summary>
    /// Constitue une mandature avec un effectif imposé.
    /// Nécessaire pour les régimes dérogatoires que le barème ne couvre pas — Paris, Lyon
    /// et Marseille relèvent de la loi PLM et ne sont pas modélisés à ce stade.
    /// </summary>
    public static Mandature CreateWithEffectifLegal(CodeInsee commune, Period periode, int effectifLegal)
    {
        DomainException.ThrowIf(effectifLegal < 1, "L'effectif légal d'un conseil est au moins de 1.");
        return new Mandature(commune, periode, effectifLegal);
    }

    /// <summary>
    /// Barème de l'article L2121-2 du CGCT : l'effectif du conseil municipal par tranche
    /// de population.
    /// </summary>
    /// <remarks>
    /// Point de contrôle : 22 303 habitants (Sèvres) donne 35, ce qui correspond au chiffre
    /// porté par les procès-verbaux réels de la commune. Le reste du barème n'a pas été
    /// vérifié tranche par tranche — à confronter à Légifrance avant tout usage national.
    /// </remarks>
    public static int EffectifLegalPour(int nombreHabitants) => nombreHabitants switch
    {
        < 0 => throw new DomainException("Une population ne peut pas être négative."),
        < 100 => 7,
        < 500 => 11,
        < 1_500 => 15,
        < 2_500 => 19,
        < 3_500 => 23,
        < 5_000 => 27,
        < 10_000 => 29,
        < 20_000 => 33,
        < 30_000 => 35,
        < 40_000 => 39,
        < 50_000 => 43,
        < 60_000 => 45,
        < 80_000 => 49,
        < 100_000 => 53,
        < 150_000 => 55,
        < 200_000 => 59,
        < 250_000 => 61,
        < 300_000 => 65,
        _ => 69
    };

    public override string ToString() => $"Mandature {Commune} {Periode} ({EffectifLegal} sièges)";
}
