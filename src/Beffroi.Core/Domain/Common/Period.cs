namespace Beffroi.Core.Domain.Common;

/// <summary>
/// Intervalle de dates, borne de fin optionnelle (période en cours).
/// Les deux bornes sont <b>incluses</b> : une période close le 15 mars contient le 15 mars.
/// </summary>
public sealed record Period
{
    private Period(DateOnly start, DateOnly? end)
    {
        Start = start;
        End = end;
    }

    public DateOnly Start { get; }

    public DateOnly? End { get; }

    public bool IsOpen => End is null;

    /// <summary>Période commencée, pas encore terminée.</summary>
    public static Period Open(DateOnly start) => new(start, null);

    /// <summary>Période bornée aux deux extrémités.</summary>
    public static Period Closed(DateOnly start, DateOnly end)
    {
        DomainException.ThrowIf(end < start, $"La fin ({end}) précède le début ({start}).");
        return new Period(start, end);
    }

    public bool Contains(DateOnly date) => date >= Start && (End is null || date <= End);

    /// <summary>Vrai si les deux périodes partagent au moins un jour.</summary>
    public bool Overlaps(Period other)
        => Start <= (other.End ?? DateOnly.MaxValue) && other.Start <= (End ?? DateOnly.MaxValue);

    /// <summary>Vrai si cette période est entièrement contenue dans <paramref name="outer"/>.</summary>
    public bool IsWithin(Period outer)
        => Start >= outer.Start
           && (outer.End is null || (End is not null && End <= outer.End));

    public Period Close(DateOnly end) => Closed(Start, end);

    public override string ToString() => End is null ? $"depuis le {Start:d}" : $"du {Start:d} au {End:d}";
}
