using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Communes;
using Beffroi.Core.Domain.Conseils;
using Beffroi.Core.Domain.Elus;
using Beffroi.Core.Domain.Seances;
using Beffroi.Core.Domain.Seances.Enums;
using Beffroi.Core.Domain.Thematiques.Enums;
using Beffroi.Core.Domain.Votes;

namespace Beffroi.Infrastructure.Adapters.InMemory;

/// <summary>
/// Jeu de démonstration en mémoire, remis à zéro à chaque démarrage.
///
/// RÈGLE : uniquement des faits constatés dans les documents publiés par la commune.
/// Les informations que ces documents ne donnent pas — rattachement des élus à une liste,
/// rang des adjoints, délégations — sont volontairement <b>absentes</b> plutôt qu'inventées.
/// L'API répondra donc « indetermine » sur l'appartenance à la majorité : c'est le comportement
/// correct, et il vaut mieux le voir tout de suite que de le masquer par des données fabriquées.
///
/// Sources : procès-verbal et liste des délibérations de la séance du 25 juin 2026,
/// portail Qualigraf de la ville de Sèvres.
/// </summary>
internal static class JeuSevres
{
    private const string PortailQualigraf = "https://ville-sevres.kiosk.qualigraf.fr/app/public/agenda/";

    internal static Jeu Construire()
    {
        var codeSevres = CodeInsee.Create("92072");

        // Population renvoyée par l'API Géo. Le millésime n'a pas été vérifié : à confirmer
        // auprès des populations légales INSEE avant tout usage en production.
        var sevres = Commune.Create(codeSevres, "Sèvres", PopulationMunicipale.Create(22_303, 2022));

        // Le conseil issu des municipales de mars 2026. Première séance connue sur le portail :
        // 27 mars 2026. Effectif de 35 confirmé par le procès-verbal du 25 juin 2026.
        var debutMandature = new DateOnly(2026, 3, 27);
        var mandature = Mandature.Create(codeSevres, Period.Open(debutMandature), sevres.Population);

        var sourceConseil = Source.Create(
            new Uri(PortailQualigraf),
            new DateOnly(2026, 7, 3),
            new DateOnly(2026, 7, 3));

        var conseil = ConseilMunicipal.Constituer(mandature, sourceConseil);
        var mandat = Period.Open(debutMandature);

        // Aucune liste électorale n'est déclarée : les documents consultés n'indiquent pas
        // le rattachement des élus. Le modèle accepte l'absence de liste, l'API la signale.

        var elus = new Dictionary<string, Siege>(StringComparer.OrdinalIgnoreCase);

        Siege Sieger(string nom, string prenom, Fonction fonction)
        {
            var personne = Personne.Create(IdentifiantRne.Create($"SEVRES-{nom}-{prenom}"), nom, prenom);
            var siege = conseil.AttribuerSiege(personne, fonction, null, mandat);
            elus[nom] = siege;
            return siege;
        }

        // Maire : seule fonction que le procès-verbal établit explicitement.
        var maire = Sieger("de LA RONCIÈRE", "Grégoire", new Fonction.Maire());

        // Les 29 autres élus présents à la séance du 25 juin 2026. Faute de source sur les
        // fonctions d'adjoint, tous sont enregistrés comme conseillers municipaux.
        string[][] presents =
        [
            ["TEXIER", "Anne"], ["HUBERT", "Olivier"], ["BOZIO-MADE", "Emilie"],
            ["HAZARD", "Philippe"], ["IDORANE", "Nadia"], ["LASSAGNE", "Loïc"],
            ["BEAUREPAIRE", "Arthur"], ["MARLE", "Catherine"], ["MOREL", "Franck-Eric"],
            ["PARPEX", "Pascale"], ["DE CONINCK", "Yves"], ["CHABOUD", "Christophe"],
            ["GUILMET", "Arnaud"], ["SAIDI", "Saïd"], ["BARBE-COLLIS", "Christine"],
            ["FOFANA", "Koly"], ["DELANNOY", "Tiphaine"], ["CHARTIER", "Florence"],
            ["VOLANTE", "Lena"], ["WALTER", "Baptiste"], ["CORRUBLE", "Adèle"],
            ["MORON", "Denis"], ["D'ALMEIDA ARAUJO", "Anne"], ["WAERNIERS", "Francis"],
            ["GALLAIS", "Marie-Agnès"], ["BEAUGRAND GERIN", "Ghislain"], ["PUZIN", "Frédéric"],
            ["de LONGEVIALLE-MOULAÏ", "Anne-Marie"], ["CANDELIER", "Catherine"]
        ];

        foreach (var elu in presents)
        {
            Sieger(elu[0], elu[1], new Fonction.ConseillerMunicipal());
        }

        // Les quatre élus ayant donné pouvoir, et l'élue excusée.
        var demeaulte = Sieger("DEMÉAULTE", "Maïlys", new Fonction.ConseillerMunicipal());
        var dulac = Sieger("DULAC", "Olivier", new Fonction.ConseillerMunicipal());
        var flamant = Sieger("FLAMANT", "Pascale", new Fonction.ConseillerMunicipal());
        var ndiaye = Sieger("NDIAYE", "Thierno-Babacar", new Fonction.ConseillerMunicipal());
        var daSilva = Sieger("DA SILVA", "Marlène", new Fonction.ConseillerMunicipal());

        var seance = ConstruireSeance(conseil, elus, maire, demeaulte, dulac, flamant, ndiaye, daSilva);

        return new Jeu(sevres, [conseil], [seance], [], []);
    }

    private static Seance ConstruireSeance(
        ConseilMunicipal conseil,
        IReadOnlyDictionary<string, Siege> elus,
        Siege maire,
        Siege demeaulte,
        Siege dulac,
        Siege flamant,
        Siege ndiaye,
        Siege daSilva)
    {
        var dateSeance = new DateOnly(2026, 6, 25);

        var seance = Seance.Tenir(
            conseil.Id,
            dateSeance,
            Source.Create(new Uri(PortailQualigraf), new DateOnly(2026, 6, 12)));

        // Présences relevées à l'appel : 30 présents, 4 pouvoirs, 1 excusée, sur 35 membres.
        foreach (var siege in conseil.CompositionAu(dateSeance))
        {
            if (siege.Id == demeaulte.Id || siege.Id == dulac.Id
                || siege.Id == flamant.Id || siege.Id == ndiaye.Id || siege.Id == daSilva.Id)
            {
                continue;
            }

            seance.ConstaterPresence(PresenceEnSeance.Present(siege.Titulaire.Id));
        }

        seance.ConstaterPresence(PresenceEnSeance.Represente(demeaulte.Titulaire.Id, elus["TEXIER"].Titulaire.Id));
        seance.ConstaterPresence(PresenceEnSeance.Represente(dulac.Titulaire.Id, maire.Titulaire.Id));
        seance.ConstaterPresence(PresenceEnSeance.Represente(flamant.Titulaire.Id, elus["HUBERT"].Titulaire.Id));
        seance.ConstaterPresence(PresenceEnSeance.Represente(ndiaye.Titulaire.Id, elus["WALTER"].Titulaire.Id));
        seance.ConstaterPresence(PresenceEnSeance.Excuse(daSilva.Titulaire.Id));

        // Le procès-verbal de cette séance n'a pas encore été approuvé au moment de la collecte.
        // Les trois délibérations ci-dessous sont celles dont la liste publiée donne
        // explicitement le résultat du vote.
        var unanimite = Vote.Create([DecompteParGroupe.Create(NomDeGroupe.Create("Conseil municipal"), 34, 0, 0)]);

        seance.Inscrire(Deliberer(
            51,
            "Election des membres de la commission de délégation de service public - "
            + "Approbation des conditions de dépôt des listes.",
            unanimite,
            maire.Id));

        seance.Inscrire(Deliberer(
            52,
            "Commission Consultative des Services Publics Locaux (CCSPL) - "
            + "Désignation des nouveaux représentants d'associations locales d'usagers",
            unanimite,
            maire.Id));

        var velo = Deliberer(
            53,
            "Désignation d'un représentant de la Ville au Réseau vélo et marche",
            unanimite,
            maire.Id);
        velo.Classer(Thematique.TransportsEtVoirie);
        seance.Inscrire(velo);

        return seance;
    }

    private static Deliberation Deliberer(int rang, string objet, Vote vote, SiegeId rapporteur)
    {
        var deliberation = Deliberation.Create(
            NumeroDeliberation.Create(2026, rang),
            objet,
            ResultatDeliberation.Adoptee,
            Source.Create(new Uri(PortailQualigraf), new DateOnly(2026, 7, 3), new DateOnly(2026, 7, 3)),
            vote);

        deliberation.AttribuerAuRapporteur(rapporteur);
        return deliberation;
    }
}
