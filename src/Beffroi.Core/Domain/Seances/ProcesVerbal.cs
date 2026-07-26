using Beffroi.Core.Domain.Common;

namespace Beffroi.Core.Domain.Seances;

/// <summary>
/// Procès-verbal d'une séance, publié ou non.
///
/// Ce type existe surtout pour représenter <b>l'absence</b> de PV. Depuis l'ordonnance
/// n°2021-1310, en vigueur au 1er juillet 2022, le procès-verbal intégral doit être mis en
/// ligne dans la semaine suivant son approbation (art. L2121-25 CGCT). Un manquement est un
/// fait d'intérêt public, pas un trou dans la base : il doit être exprimable et datable.
/// </summary>
public sealed class ProcesVerbal
{
    private const int DelaiLegalDePublicationEnJours = 7;

    private ProcesVerbal(DateOnly? dateApprobation, Source? source, int? nombreDePages)
    {
        DateApprobation = dateApprobation;
        Source = source;
        NombreDePages = nombreDePages;
    }

    /// <summary>
    /// Date d'approbation par le conseil. Un PV est approuvé lors d'une séance ultérieure :
    /// c'est cette date, et non celle de la séance, qui déclenche le délai légal.
    /// </summary>
    public DateOnly? DateApprobation { get; private set; }

    /// <summary>Document publié. <c>null</c> tant que le PV n'est pas en ligne.</summary>
    public Source? Source { get; private set; }

    public int? NombreDePages { get; private set; }

    public bool EstApprouve => DateApprobation is not null;

    public bool EstPublie => Source is not null;

    /// <summary>Échéance légale de mise en ligne, connue une fois le PV approuvé.</summary>
    public DateOnly? DateLimiteDePublication
        => DateApprobation?.AddDays(DelaiLegalDePublicationEnJours);

    /// <summary>PV attendu, ni approuvé ni publié.</summary>
    public static ProcesVerbal Attendu() => new(null, null, null);

    public static ProcesVerbal Approuve(DateOnly dateApprobation) => new(dateApprobation, null, null);

    public static ProcesVerbal Publie(DateOnly dateApprobation, Source source, int? nombreDePages = null)
    {
        DomainException.ThrowIf(
            nombreDePages is <= 0,
            "Un procès-verbal publié compte au moins une page.");

        return new ProcesVerbal(dateApprobation, source, nombreDePages);
    }

    public void Approuver(DateOnly dateApprobation)
    {
        DomainException.ThrowIf(EstApprouve, "Ce procès-verbal est déjà approuvé.");
        DateApprobation = dateApprobation;
    }

    public void Publier(Source source, int? nombreDePages = null)
    {
        DomainException.ThrowIf(
            !EstApprouve,
            "Un procès-verbal se publie après son approbation par le conseil.");
        DomainException.ThrowIf(EstPublie, "Ce procès-verbal est déjà publié.");
        DomainException.ThrowIf(nombreDePages is <= 0, "Un procès-verbal compte au moins une page.");

        Source = source;
        NombreDePages = nombreDePages;
    }

    /// <summary>
    /// Vrai si, à la date considérée, le délai légal de publication est dépassé
    /// alors que le PV n'est toujours pas en ligne.
    /// </summary>
    public bool EstEnRetardAu(DateOnly date)
        => !EstPublie && DateLimiteDePublication is { } limite && date > limite;

    /// <summary>Nombre de jours de retard à la date considérée, 0 si le délai est tenu.</summary>
    public int JoursDeRetardAu(DateOnly date)
        => EstEnRetardAu(date) ? date.DayNumber - DateLimiteDePublication!.Value.DayNumber : 0;
}
