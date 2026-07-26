namespace Beffroi.Core.Application.Contracts;

// Contrat public de l'API. Ces types sont figés vis-à-vis des clients : le domaine peut évoluer
// derrière sans casser les consommateurs. Nommage en français, comme les routes.

/// <summary>
/// Document d'origine d'un fait. Présent sur toute donnée affirmée par Beffroi :
/// c'est la traduction, dans le contrat, de l'invariant de vérifiabilité.
/// </summary>
public sealed record SourceDto(string Url, DateOnly DatePublication, DateOnly? DateTeletransmission);

public sealed record PopulationDto(int NombreHabitants, int Millesime);

public sealed record CommuneDto(string CodeInsee, string Nom, PopulationDto Population);

public sealed record PersonneDto(Guid Id, string Nom, string Prenom, string IdentifiantRne);

/// <summary>Fonction au conseil. <c>Rang</c> n'est renseigné que pour les adjoints.</summary>
public sealed record FonctionDto(string Type, int? Rang);

public sealed record DelegationDto(string Thematique, string Libelle, DateOnly Debut, DateOnly? Fin);

public sealed record ListeElectoraleDto(Guid Id, string Nom, int NombreDeSieges, bool EstMajoritaire);

/// <summary>
/// Occupation d'un siège. <c>AppartenanceMajorite</c> vaut « majorite », « horsMajorite »
/// ou « indetermine » — surtout pas un booléen : « on ne sait pas » et « dans l'opposition »
/// sont deux réponses différentes, et les confondre serait une faute éditoriale.
/// </summary>
public sealed record SiegeDto(
    Guid Id,
    PersonneDto Titulaire,
    FonctionDto Fonction,
    ListeElectoraleDto? Liste,
    string AppartenanceMajorite,
    DateOnly Debut,
    DateOnly? Fin,
    string? MotifDeFin,
    IReadOnlyList<DelegationDto> Delegations);

public sealed record MandatureDto(
    Guid ConseilId,
    string CodeInsee,
    DateOnly Debut,
    DateOnly? Fin,
    int EffectifLegal,
    int NombreMaximalDAdjoints,
    SourceDto Source);

/// <summary>Composition du conseil telle qu'elle était à la date <c>Au</c>.</summary>
public sealed record ConseilDto(
    Guid Id,
    string CodeInsee,
    DateOnly Au,
    MandatureDto Mandature,
    int SiegesOccupes,
    IReadOnlyList<ListeElectoraleDto> Listes,
    SourceDto Source);

/// <summary>
/// État de publication du procès-verbal. Le retard est calculé à la date du jour :
/// c'est l'écart au délai légal d'une semaine après approbation (art. L2121-25 CGCT).
/// </summary>
public sealed record ProcesVerbalDto(
    bool EstApprouve,
    bool EstPublie,
    DateOnly? DateApprobation,
    DateOnly? DateLimiteDePublication,
    bool EstEnRetard,
    int JoursDeRetard,
    int? NombreDePages,
    SourceDto? Source);

public sealed record PresenceDto(Guid Elu, string Statut, Guid? PouvoirDonneA);

public sealed record DecompteParGroupeDto(string Groupe, int Pour, int Contre, int Abstentions);

public sealed record PositionIndividuelleDto(Guid Elu, string Position);

/// <summary>
/// Décompte des voix. Les positions nominatives sont souvent partielles : les procès-verbaux
/// ne nomment pas systématiquement les votants.
/// </summary>
public sealed record VoteDto(
    int TotalPour,
    int TotalContre,
    int TotalAbstentions,
    int TotalExprimes,
    bool EstUnanime,
    IReadOnlyList<DecompteParGroupeDto> Decomptes,
    IReadOnlyList<PositionIndividuelleDto> Positions);

/// <summary>
/// Mise en langage clair produite par Beffroi. <c>EstRelu</c> distingue un texte validé par
/// un humain d'un brouillon généré : l'interface doit pouvoir le signaler.
/// </summary>
public sealed record ReformulationDto(
    string Titre,
    string Resume,
    string? NoteDeVote,
    bool EstRelu,
    DateOnly? DateRelecture);

/// <summary>
/// Une décision du conseil. <c>ObjetOfficiel</c> est le verbatim de l'acte ;
/// <c>Reformulation</c> est le texte éditorial de Beffroi, distinct et signalé comme tel.
/// </summary>
public sealed record DeliberationDto(
    Guid Id,
    string Numero,
    DateOnly DateSeance,
    string ObjetOfficiel,
    ReformulationDto? Reformulation,
    string? Thematique,
    decimal? Montant,
    Guid? Rapporteur,
    string Resultat,
    bool EstUnanime,
    VoteDto? Vote,
    SourceDto Source);

public sealed record SeanceDto(
    Guid Id,
    Guid ConseilId,
    DateOnly Date,
    int NombreDeDeliberations,
    int NombreDePresents,
    ProcesVerbalDto ProcesVerbal,
    SourceDto Source);

public sealed record SeanceDetailDto(
    Guid Id,
    Guid ConseilId,
    DateOnly Date,
    int NombreDePresents,
    ProcesVerbalDto ProcesVerbal,
    IReadOnlyList<DeliberationDto> Deliberations,
    IReadOnlyList<PresenceDto> Presences,
    SourceDto Source);

public sealed record ThematiqueDto(string Code, string Libelle);

/// <summary>
/// Une thématique vue depuis une commune : ce qu'elle pèse au budget et la dernière décision
/// prise dans ce domaine. <c>Part</c> et <c>Montant</c> sont nuls si aucun budget n'est connu.
/// </summary>
public sealed record ThematiqueCommunaleDto(
    string Code,
    string Libelle,
    int? Exercice,
    decimal? Part,
    decimal? Montant,
    DeliberationDto? DerniereDecision);

public sealed record LigneBudgetaireDto(string Libelle, decimal Montant, decimal Part, string? Thematique);

public sealed record BudgetSommaireDto(
    Guid Id,
    int Exercice,
    string Nature,
    decimal Total,
    SourceDto Source);

/// <summary>
/// Un budget voté et sa ventilation. <c>PartVentilee</c> vaut moins de 1 dès qu'une ligne
/// échappe aux sept thématiques — c'est une information à afficher, pas à masquer.
/// </summary>
public sealed record BudgetDto(
    Guid Id,
    string CodeInsee,
    int Exercice,
    string Nature,
    decimal Total,
    decimal? ParHabitant,
    decimal PartVentilee,
    IReadOnlyList<LigneBudgetaireDto> Lignes,
    SourceDto Source);

/// <summary>
/// Une promesse de campagne et l'état de sa réalisation. <c>Promesse</c> est la citation
/// littérale du programme ; <c>Constat</c> est le texte de Beffroi justifiant le statut.
/// </summary>
public sealed record EngagementDto(
    Guid Id,
    string Promesse,
    string Thematique,
    string Statut,
    string Constat,
    int? Attendu,
    int? Constate,
    IReadOnlyList<Guid> Deliberations,
    SourceDto Source);

public sealed record ProgrammeDto(
    Guid Id,
    string CodeInsee,
    Guid ConseilId,
    Guid ListeId,
    string NomDeLaListe,
    decimal PartRealisee,
    IReadOnlyList<EngagementDto> Engagements,
    SourceDto Source);
