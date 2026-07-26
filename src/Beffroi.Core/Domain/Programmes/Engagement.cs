using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Programmes.Enums;
using Beffroi.Core.Domain.Seances;
using Beffroi.Core.Domain.Thematiques.Enums;

namespace Beffroi.Core.Domain.Programmes;

/// <summary>
/// Une promesse de campagne et l'état de sa réalisation.
///
/// <see cref="Promesse"/> est une <b>citation littérale</b> du programme, jamais reformulée :
/// c'est ce qui permet au lecteur de vérifier qu'on n'a pas déplacé la cible.
/// <see cref="Constat"/>, à l'inverse, est le texte de Beffroi — il explique pourquoi tel
/// statut a été retenu.
/// </summary>
public sealed class Engagement
{
    private readonly List<DeliberationId> _deliberations = [];

    private Engagement(
        EngagementId id,
        string promesse,
        Thematique thematique,
        StatutEngagement statut,
        string constat,
        Source source)
    {
        Id = id;
        Promesse = promesse;
        Thematique = thematique;
        Statut = statut;
        Constat = constat;
        Source = source;
    }

    public EngagementId Id { get; }

    /// <summary>Citation exacte du programme électoral.</summary>
    public string Promesse { get; }

    public Thematique Thematique { get; }

    public StatutEngagement Statut { get; private set; }

    /// <summary>Justification du statut, rédigée par Beffroi.</summary>
    public string Constat { get; private set; }

    /// <summary>Objectif chiffré annoncé, quand la promesse en porte un (« deux écoles »).</summary>
    public int? Attendu { get; private set; }

    /// <summary>Nombre effectivement constaté, à rapporter à <see cref="Attendu"/>.</summary>
    public int? Constate { get; private set; }

    /// <summary>Délibérations sur lesquelles le constat s'appuie.</summary>
    public IReadOnlyList<DeliberationId> Deliberations => _deliberations;

    /// <summary>
    /// Source du constat. Obligatoire : un statut sans source serait une opinion,
    /// et « sans trace » sans corpus daté serait une accusation.
    /// </summary>
    public Source Source { get; }

    public static Engagement Create(
        string promesse,
        Thematique thematique,
        StatutEngagement statut,
        string constat,
        Source source)
        => Create(EngagementId.New(), promesse, thematique, statut, constat, source);

    /// <summary>Reconstitue un engagement dont l'identité est déjà connue.</summary>
    public static Engagement Create(
        EngagementId id,
        string promesse,
        Thematique thematique,
        StatutEngagement statut,
        string constat,
        Source source)
    {
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(promesse),
            "La citation de la promesse est obligatoire : c'est elle qui rend le constat vérifiable.");

        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(constat),
            "Un statut d'engagement doit être justifié par un constat écrit.");

        return new Engagement(id, promesse.Trim(), thematique, statut, constat.Trim(), source);
    }

    /// <summary>Attache un objectif chiffré et son avancement (« 1 sur 2 »).</summary>
    public void Chiffrer(int attendu, int constate)
    {
        DomainException.ThrowIf(attendu < 1, "Un objectif chiffré porte sur au moins une unité.");
        DomainException.ThrowIf(constate < 0, "Un décompte constaté ne peut pas être négatif.");
        DomainException.ThrowIf(
            constate > attendu,
            $"Constat ({constate}) supérieur à l'objectif annoncé ({attendu}) : vérifier la source.");

        Attendu = attendu;
        Constate = constate;
    }

    public void Rattacher(DeliberationId deliberation)
    {
        DomainException.ThrowIf(
            _deliberations.Contains(deliberation),
            "Cette délibération est déjà rattachée à l'engagement.");

        _deliberations.Add(deliberation);
    }

    /// <summary>Révise le statut au vu de nouveaux éléments, avec sa justification.</summary>
    public void Reevaluer(StatutEngagement statut, string constat)
    {
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(constat),
            "Une réévaluation doit être justifiée.");

        Statut = statut;
        Constat = constat.Trim();
    }

    public override string ToString() => $"« {Promesse} » — {Statut}";
}

/// <summary>Identité technique d'un <see cref="Engagement"/>.</summary>
public readonly record struct EngagementId(Guid Valeur)
{
    public static EngagementId New() => new(Guid.CreateVersion7());

    public override string ToString() => Valeur.ToString();
}
