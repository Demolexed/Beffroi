using Beffroi.Core.Domain.Common;

namespace Beffroi.Core.Domain.Seances;

/// <summary>
/// Mise en langage clair d'une délibération : ce que Beffroi écrit, par opposition à ce que
/// la commune a voté.
///
/// Trois textes, de granularité croissante :
/// le <see cref="Titre"/> tient en une phrase et dit ce qui change concrètement ;
/// le <see cref="Resume"/> développe en un paragraphe ;
/// la <see cref="NoteDeVote"/> explique, quand il y a lieu, pourquoi des élus se sont opposés
/// ou abstenus.
///
/// Séparée de l'objet officiel, et jamais à sa place : le verbatim reste opposable, ceci est
/// un produit éditorial. Si les deux fusionnaient, plus personne ne pourrait vérifier ce qui
/// a été réécrit.
///
/// <see cref="EstRelu"/> matérialise la relecture humaine d'un texte généré : une reformulation
/// non relue peut exister, mais l'interface doit pouvoir la signaler comme telle.
/// </summary>
public sealed record Reformulation
{
    private Reformulation(string titre, string resume, string? noteDeVote, DateOnly? dateRelecture)
    {
        Titre = titre;
        Resume = resume;
        NoteDeVote = noteDeVote;
        DateRelecture = dateRelecture;
    }

    /// <summary>Une phrase : ce que la décision change concrètement.</summary>
    public string Titre { get; }

    /// <summary>Un paragraphe de contexte, sans jargon administratif.</summary>
    public string Resume { get; }

    /// <summary>
    /// Explication des voix contre et des abstentions. Absente quand le vote n'appelle aucun
    /// commentaire — une unanimité sans débat n'a pas à être glosée.
    /// </summary>
    public string? NoteDeVote { get; }

    public DateOnly? DateRelecture { get; }

    public bool EstRelu => DateRelecture is not null;

    /// <summary>Reformulation produite mais pas encore validée par un humain.</summary>
    public static Reformulation Brouillon(string titre, string resume, string? noteDeVote = null)
        => new(Valider(titre, "titre"), Valider(resume, "résumé"), Nettoyer(noteDeVote), null);

    public static Reformulation Relue(
        string titre,
        string resume,
        DateOnly dateRelecture,
        string? noteDeVote = null)
        => new(Valider(titre, "titre"), Valider(resume, "résumé"), Nettoyer(noteDeVote), dateRelecture);

    /// <summary>Valide la reformulation à une date donnée.</summary>
    public Reformulation Relire(DateOnly dateRelecture)
    {
        DomainException.ThrowIf(EstRelu, "Cette reformulation est déjà relue.");
        return new Reformulation(Titre, Resume, NoteDeVote, dateRelecture);
    }

    private static string Valider(string texte, string champ)
    {
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(texte),
            $"Une reformulation sans {champ} n'a pas de sens : ne pas en créer.");

        return texte.Trim();
    }

    private static string? Nettoyer(string? texte)
        => string.IsNullOrWhiteSpace(texte) ? null : texte.Trim();

    public override string ToString() => Titre;
}
