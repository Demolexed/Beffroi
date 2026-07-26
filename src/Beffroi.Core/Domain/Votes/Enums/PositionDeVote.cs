namespace Beffroi.Core.Domain.Votes.Enums;

/// <summary>
/// Position exprimée par un élu sur une délibération.
/// </summary>
public enum PositionDeVote
{
    Pour,

    Contre,

    Abstention,

    /// <summary>
    /// Ne prend pas part au vote. Distinct de l'abstention : c'est le cas notamment
    /// de l'élu intéressé à l'affaire, qui doit se retirer.
    /// </summary>
    NePrendPasPart
}
