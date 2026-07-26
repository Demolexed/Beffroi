using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Communes;
using Beffroi.Core.Domain.Conseils;
using Beffroi.Core.Domain.Programmes.Enums;

namespace Beffroi.Core.Domain.Programmes;

/// <summary>
/// Racine d'agrégat : le programme électoral d'une liste, et le suivi de ses engagements.
///
/// Rattaché à une <see cref="ListeElectorale"/> et non au conseil : un programme est le fait
/// d'une liste candidate, pas de l'institution. Une commune peut donc en héberger plusieurs
/// pour une même mandature — celui de la majorité comme ceux des listes minoritaires.
/// </summary>
public sealed class Programme
{
    private readonly List<Engagement> _engagements = [];

    private Programme(
        ProgrammeId id,
        CodeInsee commune,
        ConseilMunicipalId conseil,
        ListeElectoraleId liste,
        string nomDeLaListe,
        Source source)
    {
        Id = id;
        Commune = commune;
        Conseil = conseil;
        Liste = liste;
        NomDeLaListe = nomDeLaListe;
        Source = source;
    }

    public ProgrammeId Id { get; }

    public CodeInsee Commune { get; }

    /// <summary>Mandature au titre de laquelle le programme a été présenté.</summary>
    public ConseilMunicipalId Conseil { get; }

    public ListeElectoraleId Liste { get; }

    /// <summary>Nom de la liste, repris pour l'affichage sans avoir à traverser l'agrégat conseil.</summary>
    public string NomDeLaListe { get; }

    /// <summary>Document de campagne d'où les engagements sont tirés.</summary>
    public Source Source { get; }

    public IReadOnlyList<Engagement> Engagements => _engagements;

    public static Programme Presenter(
        CodeInsee commune,
        ConseilMunicipalId conseil,
        ListeElectoraleId liste,
        string nomDeLaListe,
        Source source)
        => Presenter(ProgrammeId.New(), commune, conseil, liste, nomDeLaListe, source);

    /// <summary>Reconstitue un programme dont l'identité est déjà connue.</summary>
    public static Programme Presenter(
        ProgrammeId id,
        CodeInsee commune,
        ConseilMunicipalId conseil,
        ListeElectoraleId liste,
        string nomDeLaListe,
        Source source)
    {
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(nomDeLaListe),
            "Le nom de la liste est obligatoire.");

        return new Programme(id, commune, conseil, liste, nomDeLaListe.Trim(), source);
    }

    public Engagement Inscrire(Engagement engagement)
    {
        DomainException.ThrowIf(
            _engagements.Any(existant => existant.Id == engagement.Id),
            "Cet engagement est déjà inscrit au programme.");

        _engagements.Add(engagement);
        return engagement;
    }

    public IReadOnlyList<Engagement> ParStatut(StatutEngagement statut)
        => [.. _engagements.Where(engagement => engagement.Statut == statut)];

    /// <summary>
    /// Part des engagements tenus. Volontairement brute : les engagements partiellement
    /// réalisés ne comptent pas pour moitié, ils sont exclus du numérateur et signalés à part.
    /// Pondérer reviendrait à porter un jugement supplémentaire, non sourçable.
    /// </summary>
    public decimal PartRealisee
        => _engagements.Count == 0
            ? 0
            : (decimal)ParStatut(StatutEngagement.Realise).Count / _engagements.Count;

    public override string ToString() => $"Programme « {NomDeLaListe} » — {_engagements.Count} engagements";
}

/// <summary>Identité technique d'un <see cref="Programme"/>.</summary>
public readonly record struct ProgrammeId(Guid Valeur)
{
    public static ProgrammeId New() => new(Guid.CreateVersion7());

    public override string ToString() => Valeur.ToString();
}
