namespace Beffroi.Core.Domain.Programmes.Enums;

/// <summary>
/// Où en est un engagement de campagne, confronté aux délibérations et au budget.
///
/// C'est le seul endroit du domaine où Beffroi porte un <b>jugement</b> et non un constat.
/// Chaque statut doit donc pouvoir s'adosser à une source datée — y compris
/// <see cref="SansTrace"/>, qui affirme une absence et engage donc autant que les autres.
/// </summary>
public enum StatutEngagement
{
    /// <summary>Voté et exécution constatée.</summary>
    Realise,

    /// <summary>Engagement chiffré dont une partie seulement est tenue.</summary>
    PartiellementRealise,

    /// <summary>Délibération ou crédits votés, sans réalisation constatée à ce jour.</summary>
    VoteNonRealise,

    /// <summary>Aucune délibération ni ligne budgétaire trouvée dans le corpus examiné.</summary>
    SansTrace
}
