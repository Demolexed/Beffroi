using Beffroi.Core.Domain.Budgets;
using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Communes;
using Beffroi.Core.Domain.Conseils;
using Beffroi.Core.Domain.Programmes;
using Beffroi.Core.Domain.Seances;
using Beffroi.Core.Domain.Thematiques.Enums;
using Beffroi.Core.Domain.Votes;

namespace Beffroi.Core.Application.Contracts;

/// <summary>
/// Traduction du domaine vers le contrat public. Isolée ici pour que les entités du domaine
/// ne se retrouvent jamais sérialisées telles quelles : leur forme doit pouvoir changer sans
/// casser les clients de l'API.
/// </summary>
public static class Projections
{
    public const string AppartenanceMajorite = "majorite";
    public const string AppartenanceHorsMajorite = "horsMajorite";
    public const string AppartenanceIndeterminee = "indetermine";

    public static SourceDto VersDto(this Source source)
        => new(source.Url.ToString(), source.DatePublication, source.DateTeletransmission);

    public static CommuneDto VersDto(this Commune commune)
        => new(
            commune.Code.Valeur,
            commune.Nom,
            new PopulationDto(commune.Population.NombreHabitants, commune.Population.Millesime));

    public static ThematiqueDto VersDto(this Thematique thematique)
        => new(Code(thematique), Libelle(thematique));

    public static MandatureDto VersDto(this ConseilMunicipal conseil)
        => new(
            conseil.Id.Valeur,
            conseil.Mandature.Commune.Valeur,
            conseil.Mandature.Periode.Start,
            conseil.Mandature.Periode.End,
            conseil.Mandature.EffectifLegal,
            conseil.Mandature.NombreMaximalDAdjoints,
            conseil.Source.VersDto());

    public static ConseilDto VersDto(this ConseilMunicipal conseil, DateOnly au)
        => new(
            conseil.Id.Valeur,
            conseil.Mandature.Commune.Valeur,
            au,
            conseil.VersDto(),
            conseil.CompositionAu(au).Count,
            [.. conseil.Listes.Select(liste => liste.VersDto(conseil, au))],
            conseil.Source.VersDto());

    public static ListeElectoraleDto VersDto(this ListeElectorale liste, ConseilMunicipal conseil, DateOnly au)
        => new(
            liste.Id.Valeur,
            liste.Nom,
            liste.NombreDeSieges,
            conseil.ListeMajoritaireAu(au)?.Id == liste.Id);

    public static SiegeDto VersDto(this Siege siege, ConseilMunicipal conseil, DateOnly au)
        => new(
            siege.Id.Valeur,
            new PersonneDto(
                siege.Titulaire.Id.Valeur,
                siege.Titulaire.Nom,
                siege.Titulaire.Prenom,
                siege.Titulaire.Identifiant.Valeur),
            siege.Fonction.VersDto(),
            siege.Liste?.VersDto(conseil, au),
            Appartenance(conseil.AppartientALaMajoriteAu(siege.Id, au)),
            siege.Periode.Start,
            siege.Periode.End,
            siege.MotifDeFin?.ToString(),
            [.. siege.DelegationsAu(au).Select(VersDto)]);

    public static FonctionDto VersDto(this Fonction fonction) => fonction switch
    {
        Fonction.Maire => new FonctionDto("maire", null),
        Fonction.Adjoint adjoint => new FonctionDto("adjoint", adjoint.Rang),
        _ => new FonctionDto("conseillerMunicipal", null)
    };

    public static DelegationDto VersDto(this Delegation delegation)
        => new(
            Code(delegation.Thematique),
            delegation.Libelle,
            delegation.Periode.Start,
            delegation.Periode.End);

    public static ProcesVerbalDto VersDto(this ProcesVerbal pv, DateOnly aujourdhui)
        => new(
            pv.EstApprouve,
            pv.EstPublie,
            pv.DateApprobation,
            pv.DateLimiteDePublication,
            pv.EstEnRetardAu(aujourdhui),
            pv.JoursDeRetardAu(aujourdhui),
            pv.NombreDePages,
            pv.Source?.VersDto());

    public static PresenceDto VersDto(this PresenceEnSeance presence)
        => new(presence.Elu.Valeur, presence.Statut.ToString(), presence.PouvoirDonneA?.Valeur);

    public static VoteDto VersDto(this Vote vote)
        => new(
            vote.TotalPour,
            vote.TotalContre,
            vote.TotalAbstentions,
            vote.TotalExprimes,
            vote.EstUnanime,
            [.. vote.Decomptes.Select(d => new DecompteParGroupeDto(d.Groupe.Valeur, d.Pour, d.Contre, d.Abstentions))],
            [.. vote.Positions.Select(p => new PositionIndividuelleDto(p.Elu.Valeur, p.Position.ToString()))]);

    /// <summary>
    /// La date vient de la séance : une délibération ne la porte pas, elle la tient de la
    /// réunion qui l'a examinée.
    /// </summary>
    public static DeliberationDto VersDto(this Deliberation deliberation, DateOnly dateSeance)
        => new(
            deliberation.Id.Valeur,
            deliberation.Numero.ToString(),
            dateSeance,
            deliberation.ObjetOfficiel,
            deliberation.Reformulation is { } reformulation
                ? new ReformulationDto(
                    reformulation.Titre,
                    reformulation.Resume,
                    reformulation.NoteDeVote,
                    reformulation.EstRelu,
                    reformulation.DateRelecture)
                : null,
            deliberation.Thematique is { } thematique ? Code(thematique) : null,
            deliberation.Montant?.Euros,
            deliberation.Rapporteur?.Valeur,
            deliberation.Resultat.ToString(),
            deliberation.EstUnanime,
            deliberation.Vote?.VersDto(),
            deliberation.Source.VersDto());

    public static SeanceDto VersDto(this Seance seance, DateOnly aujourdhui)
        => new(
            seance.Id.Valeur,
            seance.Conseil.Valeur,
            seance.Date,
            seance.Deliberations.Count,
            seance.NombreDePresents,
            seance.ProcesVerbal.VersDto(aujourdhui),
            seance.Source.VersDto());

    public static SeanceDetailDto VersDetailDto(this Seance seance, DateOnly aujourdhui)
        => new(
            seance.Id.Valeur,
            seance.Conseil.Valeur,
            seance.Date,
            seance.NombreDePresents,
            seance.ProcesVerbal.VersDto(aujourdhui),
            [.. seance.Deliberations.Select(deliberation => deliberation.VersDto(seance.Date))],
            [.. seance.Presences.Select(VersDto)],
            seance.Source.VersDto());

    public static BudgetSommaireDto VersSommaireDto(this Budget budget)
        => new(budget.Id.Valeur, budget.Exercice, budget.Nature.ToString(), budget.Total.Euros, budget.Source.VersDto());

    public static BudgetDto VersDto(this Budget budget, PopulationMunicipale? population)
    {
        var total = budget.Total.Euros;

        return new BudgetDto(
            budget.Id.Valeur,
            budget.Commune.Valeur,
            budget.Exercice,
            budget.Nature.ToString(),
            total,
            population is { NombreHabitants: > 0 } ? budget.ParHabitant(population).Euros : null,
            budget.PartVentilee,
            [.. budget.Lignes.Select(ligne => new LigneBudgetaireDto(
                ligne.Libelle,
                ligne.Montant.Euros,
                total == 0 ? 0 : ligne.Montant.Euros / total,
                ligne.Thematique is { } thematique ? Code(thematique) : null))],
            budget.Source.VersDto());
    }

    public static EngagementDto VersDto(this Engagement engagement)
        => new(
            engagement.Id.Valeur,
            engagement.Promesse,
            Code(engagement.Thematique),
            engagement.Statut.ToString(),
            engagement.Constat,
            engagement.Attendu,
            engagement.Constate,
            [.. engagement.Deliberations.Select(deliberation => deliberation.Valeur)],
            engagement.Source.VersDto());

    public static ProgrammeDto VersDto(this Programme programme)
        => new(
            programme.Id.Valeur,
            programme.Commune.Valeur,
            programme.Conseil.Valeur,
            programme.Liste.Valeur,
            programme.NomDeLaListe,
            programme.PartRealisee,
            [.. programme.Engagements.Select(VersDto)],
            programme.Source.VersDto());

    private static string Appartenance(bool? appartient) => appartient switch
    {
        true => AppartenanceMajorite,
        false => AppartenanceHorsMajorite,
        null => AppartenanceIndeterminee
    };

    public static string Code(Thematique thematique) => thematique switch
    {
        Thematique.Education => "education",
        Thematique.TransportsEtVoirie => "transports-et-voirie",
        Thematique.Environnement => "environnement",
        Thematique.Finances => "finances",
        Thematique.Urbanisme => "urbanisme",
        Thematique.Securite => "securite",
        _ => "culture-et-sport"
    };

    public static string Libelle(Thematique thematique) => thematique switch
    {
        Thematique.Education => "Éducation",
        Thematique.TransportsEtVoirie => "Transports et voirie",
        Thematique.Environnement => "Environnement",
        Thematique.Finances => "Finances",
        Thematique.Urbanisme => "Urbanisme",
        Thematique.Securite => "Sécurité",
        _ => "Culture et sport"
    };

    /// <summary>Résout le code d'URL d'une thématique. <c>null</c> si le code est inconnu.</summary>
    public static Thematique? DepuisCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        foreach (var thematique in Enum.GetValues<Thematique>())
        {
            if (Code(thematique).Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                return thematique;
            }
        }

        return null;
    }
}
