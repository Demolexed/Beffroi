using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Elus;

namespace Beffroi.Core.Domain.Conseils;

/// <summary>
/// Racine d'agrégat : le conseil municipal d'une commune sur une mandature.
///
/// Porte les invariants que le CGCT impose à la composition d'un conseil, et fournit
/// les lectures datées dont dépend toute la vérifiabilité du projet.
/// </summary>
public sealed class ConseilMunicipal
{
    private readonly List<ListeElectorale> _listes = [];
    private readonly List<Siege> _sieges = [];

    private ConseilMunicipal(Mandature mandature, Source source)
    {
        Mandature = mandature;
        Source = source;
    }

    public Mandature Mandature { get; }

    /// <summary>Document d'où la composition est tirée. Obligatoire : pas de fait sans source.</summary>
    public Source Source { get; }

    public IReadOnlyList<ListeElectorale> Listes => _listes;

    public IReadOnlyList<Siege> Sieges => _sieges;

    public static ConseilMunicipal Constituer(Mandature mandature, Source source)
        => new(mandature, source);

    /// <summary>
    /// Déclare une liste ayant obtenu des sièges. À appeler avant d'attribuer les sièges
    /// qui s'y rattachent.
    /// </summary>
    public ListeElectorale DeclarerListe(string nom, int nombreDeSieges)
    {
        var liste = new ListeElectorale(nom, nombreDeSieges);

        DomainException.ThrowIf(
            _listes.Any(existante => existante.Nom.Equals(liste.Nom, StringComparison.OrdinalIgnoreCase)),
            $"La liste « {liste.Nom} » est déjà déclarée sur cette mandature.");

        _listes.Add(liste);
        return liste;
    }

    /// <summary>
    /// Attribue un siège à une personne. Vérifie les invariants légaux de composition.
    /// </summary>
    public Siege AttribuerSiege(Personne titulaire, Fonction fonction, ListeElectorale? liste, Period periode)
    {
        DomainException.ThrowIf(
            !periode.IsWithin(Mandature.Periode),
            $"La période du siège ({periode}) déborde la mandature ({Mandature.Periode}).");

        DomainException.ThrowIf(
            liste is not null && !_listes.Contains(liste),
            "La liste rattachée au siège n'a pas été déclarée sur cette mandature.");

        VerifierEffectifLegal(periode);
        VerifierUniciteDuMaire(fonction, periode);
        VerifierAdjoints(fonction, periode);

        var siege = new Siege(titulaire, fonction, liste, periode);
        _sieges.Add(siege);
        return siege;
    }

    /// <summary>Met fin à l'occupation d'un siège (démission, décès, fin de mandature).</summary>
    public void CloreSiege(Siege siege, DateOnly fin, MotifDeFin motif)
    {
        DomainException.ThrowIf(
            !_sieges.Contains(siege),
            "Ce siège n'appartient pas à ce conseil municipal.");

        siege.Clore(fin, motif);
    }

    // --- Lectures datées -----------------------------------------------------------------------

    /// <summary>Composition du conseil à une date donnée.</summary>
    public IReadOnlyList<Siege> CompositionAu(DateOnly date)
        => [.. _sieges.Where(siege => siege.EstOccupeAu(date))];

    public Siege? MaireAu(DateOnly date)
        => _sieges.SingleOrDefault(siege => siege.Fonction is Fonction.Maire && siege.EstOccupeAu(date));

    /// <summary>
    /// Liste majoritaire à une date donnée : celle dont est issu le maire.
    ///
    /// Donnée <b>dérivée</b>, jamais stockée. C'est ce qui permet de rester factuel :
    /// l'appartenance à la majorité découle d'un fait électoral vérifiable, et non d'une
    /// étiquette posée par la plateforme.
    /// Renvoie <c>null</c> si aucun maire n'est connu à cette date, ou si le maire n'est
    /// rattaché à aucune liste (communes de moins de 1 000 habitants).
    /// </summary>
    public ListeElectorale? ListeMajoritaireAu(DateOnly date) => MaireAu(date)?.Liste;

    /// <summary>
    /// Vrai si le siège se rattache à la liste majoritaire à cette date.
    /// <c>null</c> quand la question n'a pas de réponse factuelle : pas de maire connu,
    /// ou aucun rattachement de liste. Ne jamais convertir ce <c>null</c> en « opposition ».
    /// </summary>
    public bool? AppartientALaMajoriteAu(Siege siege, DateOnly date)
    {
        var majoritaire = ListeMajoritaireAu(date);
        if (majoritaire is null || siege.Liste is null)
        {
            return null;
        }

        return ReferenceEquals(siege.Liste, majoritaire);
    }

    // --- Invariants ----------------------------------------------------------------------------

    private void VerifierEffectifLegal(Period periode)
    {
        var occupesAuDebut = _sieges.Count(siege => siege.EstOccupeAu(periode.Start));

        DomainException.ThrowIf(
            occupesAuDebut >= Mandature.EffectifLegal,
            $"L'effectif légal du conseil ({Mandature.EffectifLegal}) serait dépassé au {periode.Start:d}.");
    }

    private void VerifierUniciteDuMaire(Fonction fonction, Period periode)
    {
        if (fonction is not Fonction.Maire)
        {
            return;
        }

        DomainException.ThrowIf(
            _sieges.Any(siege => siege.Fonction is Fonction.Maire && siege.Periode.Overlaps(periode)),
            "Une commune ne peut avoir qu'un seul maire à la fois.");
    }

    private void VerifierAdjoints(Fonction fonction, Period periode)
    {
        if (fonction is not Fonction.Adjoint adjoint)
        {
            return;
        }

        DomainException.ThrowIf(
            _sieges.Any(siege => siege.Fonction is Fonction.Adjoint autre
                                 && autre.Rang == adjoint.Rang
                                 && siege.Periode.Overlaps(periode)),
            $"Le rang d'adjoint n°{adjoint.Rang} est déjà occupé sur cette période.");

        var adjointsAuDebut = _sieges.Count(siege => siege.Fonction is Fonction.Adjoint
                                                     && siege.EstOccupeAu(periode.Start));

        DomainException.ThrowIf(
            adjointsAuDebut >= Mandature.NombreMaximalDAdjoints,
            $"Le nombre d'adjoints ne peut excéder 30 % de l'effectif légal, "
            + $"soit {Mandature.NombreMaximalDAdjoints} (art. L2122-2 CGCT).");
    }

    public override string ToString()
        => $"Conseil municipal de {Mandature.Commune} — {Mandature.Periode} — {_sieges.Count} sièges";
}
