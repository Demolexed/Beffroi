using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Conseils;
using Beffroi.Core.Domain.Seances.Enums;
using Beffroi.Core.Domain.Thematiques.Enums;
using Beffroi.Core.Domain.Votes;

namespace Beffroi.Core.Domain.Seances;

/// <summary>
/// Une décision inscrite à l'ordre du jour d'une séance et le sort qui lui a été réservé.
/// Entité de l'agrégat <see cref="Seance"/>.
/// </summary>
public sealed class Deliberation
{
    private Deliberation(
        DeliberationId id,
        NumeroDeliberation numero,
        string objetOfficiel,
        ResultatDeliberation resultat,
        Source source)
    {
        Id = id;
        Numero = numero;
        ObjetOfficiel = objetOfficiel;
        Resultat = resultat;
        Source = source;
    }

    public DeliberationId Id { get; }

    public NumeroDeliberation Numero { get; }

    /// <summary>
    /// Objet tel qu'il figure sur l'acte. Verbatim : ce champ ne doit jamais être reformulé,
    /// c'est lui qui rend la reformulation vérifiable.
    /// </summary>
    public string ObjetOfficiel { get; }

    /// <summary>Reformulation éditoriale, absente tant qu'elle n'a pas été produite.</summary>
    public IntituleEnClair? IntituleEnClair { get; private set; }

    public Thematique? Thematique { get; private set; }

    /// <summary>Enjeu financier, quand la délibération en porte un.</summary>
    public Montant? Montant { get; private set; }

    /// <summary>Siège du rapporteur : on rapporte ès qualité, à une date donnée.</summary>
    public SiegeId? Rapporteur { get; private set; }

    public ResultatDeliberation Resultat { get; }

    /// <summary>
    /// Décompte des voix. Absent pour les délibérations retirées, ajournées ou faisant
    /// l'objet d'une simple prise d'acte.
    /// </summary>
    public Vote? Vote { get; private set; }

    public Source Source { get; }

    /// <summary>Vrai quand la délibération a été votée sans voix contre ni abstention.</summary>
    public bool EstUnanime => Vote?.EstUnanime ?? false;

    public static Deliberation Create(
        NumeroDeliberation numero,
        string objetOfficiel,
        ResultatDeliberation resultat,
        Source source,
        Vote? vote = null)
        => Create(DeliberationId.New(), numero, objetOfficiel, resultat, source, vote);

    /// <summary>Reconstitue une délibération dont l'identité est déjà connue.</summary>
    public static Deliberation Create(
        DeliberationId id,
        NumeroDeliberation numero,
        string objetOfficiel,
        ResultatDeliberation resultat,
        Source source,
        Vote? vote = null)
    {
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(objetOfficiel),
            "L'objet officiel d'une délibération est obligatoire.");

        // Une délibération adoptée ou rejetée l'a été par un vote : sans décompte, l'affirmation
        // « adoptée » ne serait adossée à rien.
        DomainException.ThrowIf(
            resultat is ResultatDeliberation.Adoptee or ResultatDeliberation.Rejetee && vote is null,
            $"Une délibération « {resultat} » doit porter le décompte du vote qui l'a produite.");

        DomainException.ThrowIf(
            resultat is ResultatDeliberation.Retiree or ResultatDeliberation.Ajournee && vote is not null,
            $"Une délibération « {resultat} » n'a pas été soumise au vote.");

        var deliberation = new Deliberation(id, numero, objetOfficiel.Trim(), resultat, source)
        {
            Vote = vote
        };

        return deliberation;
    }

    public void Classer(Thematique thematique) => Thematique = thematique;

    public void Reformuler(IntituleEnClair intitule) => IntituleEnClair = intitule;

    public void AttribuerAuRapporteur(SiegeId siege) => Rapporteur = siege;

    public void ChiffrerA(Montant montant) => Montant = montant;

    public override string ToString() => $"{Numero} — {ObjetOfficiel}";
}

/// <summary>Identité technique d'une <see cref="Deliberation"/>.</summary>
public readonly record struct DeliberationId(Guid Valeur)
{
    public static DeliberationId New() => new(Guid.CreateVersion7());

    public override string ToString() => Valeur.ToString();
}
