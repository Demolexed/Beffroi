using Beffroi.Core.Domain.Budgets;
using Beffroi.Core.Domain.Budgets.Enums;
using Beffroi.Core.Domain.Common;
using Beffroi.Core.Domain.Communes;
using Beffroi.Core.Domain.Conseils;
using Beffroi.Core.Domain.Conseils.Enums;
using Beffroi.Core.Domain.Elus;
using Beffroi.Core.Domain.Programmes;
using Beffroi.Core.Domain.Programmes.Enums;
using Beffroi.Core.Domain.Seances;
using Beffroi.Core.Domain.Seances.Enums;
using Beffroi.Core.Domain.Thematiques.Enums;
using Beffroi.Core.Domain.Votes;

namespace Beffroi.Infrastructure.Adapters.InMemory;

/// <summary>
/// Commune de démonstration, entièrement fictive.
///
/// Elle existe parce que le jeu de Sèvres, volontairement limité aux faits vérifiés, ne peut
/// pas illustrer ce que les documents publiés ne disent pas : rattachement des élus à une
/// liste, délégations, budget, programme électoral. Fabriquer ces données sur des personnes
/// réelles serait exactement ce que le projet combat ; les fabriquer sur une commune
/// ouvertement fictive ne trompe personne.
///
/// Aucune ressemblance avec une commune existante n'est voulue. Le code INSEE 99000 n'est
/// attribué à aucune commune française.
/// </summary>
internal static class JeuSaintAmand
{
    private const string SourceDemo = "https://exemple.invalid/saint-amand/deliberations";

    private static readonly DateOnly DebutMandature = new(2020, 3, 15);

    internal static Jeu Construire()
    {
        var code = CodeInsee.Create("99000");

        // 28 400 habitants placent la commune dans la tranche 20 000–29 999, soit 35 sièges
        // au barème de l'article L2121-2. La maquette en annonce 33 : on seme 33 sièges pour
        // coller à l'écran, l'effectif légal calculé reste 35. L'écart est visible dans l'API,
        // et c'est volontaire.
        var commune = Commune.Create(code, "Saint-Amand (commune de démonstration)",
            PopulationMunicipale.Create(28_400, 2022));

        var source = Source.Create(new Uri(SourceDemo), new DateOnly(2026, 3, 20));
        var mandature = Mandature.Create(code, Period.Open(DebutMandature), commune.Population);
        var conseil = ConseilMunicipal.Constituer(mandature, source);

        var ensemble = conseil.DeclarerListe("Saint-Amand ensemble", 25);
        var autrement = conseil.DeclarerListe("Saint-Amand autrement", 6);
        var vivre = conseil.DeclarerListe("Vivre Saint-Amand", 2);

        var mandat = Period.Open(DebutMandature);
        var sieges = new Dictionary<string, Siege>(StringComparer.OrdinalIgnoreCase);

        Siege Sieger(string prenom, string nom, Fonction fonction, ListeElectorale liste, Period periode)
        {
            var personne = Personne.Create(IdentifiantRne.Create($"DEMO-{nom}-{prenom}"), nom, prenom);
            var siege = conseil.AttribuerSiege(personne, fonction, liste, periode);
            sieges[nom] = siege;
            return siege;
        }

        // Exécutif municipal.
        var maire = Sieger("Hélène", "Marchand", new Fonction.Maire(), ensemble, mandat);

        (string Prenom, string Nom, int Rang, Thematique Theme, string Libelle)[] adjoints =
        [
            ("Karim", "Belhadj", 1, Thematique.Urbanisme, "Urbanisme"),
            ("Sylvie", "Nguyen", 2, Thematique.Education, "Éducation"),
            ("Bruno", "Lefèvre", 3, Thematique.TransportsEtVoirie, "Transports et voirie"),
            ("Aïcha", "Fournier", 4, Thematique.Environnement, "Environnement"),
            ("Marc", "Ollivier", 5, Thematique.Securite, "Sécurité"),
            ("Claire", "Dubois", 6, Thematique.CultureEtSport, "Culture et sport"),
            ("Julien", "Ricard", 7, Thematique.Finances, "Finances")
        ];

        foreach (var (prenom, nom, rang, theme, libelle) in adjoints)
        {
            var siege = Sieger(prenom, nom, new Fonction.Adjoint(rang), ensemble, mandat);
            conseil.ConfierDelegation(siege.Id, Delegation.Create(theme, libelle, mandat));
        }

        // Conseillers de la majorité : 25 sièges au total, 8 déjà pourvus ci-dessus.
        string[][] majorite =
        [
            ["Farid", "Amrani"], ["Laure", "Bertrand"], ["Yannick", "Colin"], ["Sonia", "Delmas"],
            ["Hugo", "Estève"], ["Naïma", "Guerra"], ["Paul", "Humbert"], ["Léa", "Jacquot"],
            ["Rémi", "Klein"], ["Aurore", "Lemoine"], ["Samir", "Meziane"], ["Chloé", "Noury"],
            ["Vincent", "Perrot"], ["Estelle", "Quentin"], ["Malik", "Rahmani"], ["Juliette", "Sabatier"],
            ["Olivier", "Tessier"]
        ];

        foreach (var elu in majorite)
        {
            Sieger(elu[0], elu[1], new Fonction.ConseillerMunicipal(), ensemble, mandat);
        }

        // Première liste minoritaire : 6 sièges, dont un renouvelé en cours de mandat.
        Sieger("Pierre", "Vasseur", new Fonction.ConseillerMunicipal(), autrement, mandat);
        Sieger("Nadia", "Ferrand", new Fonction.ConseillerMunicipal(), autrement, mandat);
        Sieger("Antoine", "Bouvier", new Fonction.ConseillerMunicipal(), autrement, mandat);
        Sieger("Fatou", "Diallo", new Fonction.ConseillerMunicipal(), autrement, mandat);
        Sieger("Gérard", "Mongin", new Fonction.ConseillerMunicipal(), autrement, mandat);

        // Une démission en cours de mandat, remplacée par le suivant de liste
        // (art. L270 du code électoral). C'est ce qui rend « ?au= » démontrable.
        var demissionnaire = Sieger("Sabine", "Roux", new Fonction.ConseillerMunicipal(), autrement, mandat);
        conseil.CloreSiege(demissionnaire.Id, new DateOnly(2025, 9, 30), MotifDeFin.Demission);
        Sieger("Idriss", "Kaboré", new Fonction.ConseillerMunicipal(), autrement,
            Period.Open(new DateOnly(2025, 10, 1)));

        // Seconde liste minoritaire : 2 sièges.
        Sieger("Thomas", "Reboul", new Fonction.ConseillerMunicipal(), vivre, mandat);
        Sieger("Margot", "Sylvestre", new Fonction.ConseillerMunicipal(), vivre, mandat);

        var seances = ConstruireSeances(conseil, maire, sieges);
        var budget = ConstruireBudget(code);
        var programme = ConstruireProgramme(code, conseil, ensemble, seances);

        return new Jeu(commune, [conseil], seances, [budget], [programme]);
    }

    private static IReadOnlyList<Seance> ConstruireSeances(
        ConseilMunicipal conseil,
        Siege maire,
        IReadOnlyDictionary<string, Siege> sieges)
    {
        var ensemble = NomDeGroupe.Create("Saint-Amand ensemble");
        var autrement = NomDeGroupe.Create("Saint-Amand autrement");
        var vivre = NomDeGroupe.Create("Vivre Saint-Amand");

        // --- 29 janvier 2026 : unanimité -----------------------------------------------------
        var janvier = Seance.Tenir(conseil.Id, new DateOnly(2026, 1, 29), Document(new DateOnly(2026, 1, 20)));
        Emarger(janvier, conseil, new DateOnly(2026, 1, 29));

        var transports = Deliberation.Create(
            NumeroDeliberation.Create(2026, 4),
            "Attribution du contrat de concession de service public de transport urbain",
            ResultatDeliberation.Adoptee,
            Document(new DateOnly(2026, 2, 5)),
            Vote.Create(
            [
                DecompteParGroupe.Create(ensemble, 25, 0, 0),
                DecompteParGroupe.Create(autrement, 6, 0, 0),
                DecompteParGroupe.Create(vivre, 2, 0, 0)
            ]));
        transports.Classer(Thematique.TransportsEtVoirie);
        transports.AttribuerAuRapporteur(sieges["Lefèvre"].Id);
        transports.Reformuler(Reformulation.Relue(
            "Le réseau de bus change d'exploitant, avec deux lignes prolongées vers les zones d'activité.",
            "Le contrat de délégation de service public du réseau urbain est confié à un nouvel "
            + "exploitant pour sept ans. Les fréquences du samedi augmentent, le prix du ticket "
            + "reste à 1,20 €.",
            new DateOnly(2026, 2, 9)));
        janvier.Inscrire(transports);
        janvier.ProcesVerbal.Approuver(new DateOnly(2026, 2, 12));
        janvier.ProcesVerbal.Publier(Document(new DateOnly(2026, 2, 16)), 31);

        // --- 12 février 2026 : vote très contesté ---------------------------------------------
        var fevrier = Seance.Tenir(conseil.Id, new DateOnly(2026, 2, 12), Document(new DateOnly(2026, 2, 3)));
        Emarger(fevrier, conseil, new DateOnly(2026, 2, 12));

        var plu = Deliberation.Create(
            NumeroDeliberation.Create(2026, 9),
            "Approbation de la modification n° 3 du plan local d'urbanisme, secteur Gare-Nord",
            ResultatDeliberation.Adoptee,
            Document(new DateOnly(2026, 2, 19)),
            Vote.Create(
            [
                DecompteParGroupe.Create(ensemble, 21, 0, 4),
                DecompteParGroupe.Create(autrement, 0, 6, 0),
                DecompteParGroupe.Create(vivre, 0, 2, 0)
            ]));
        plu.Classer(Thematique.Urbanisme);
        plu.AttribuerAuRapporteur(sieges["Belhadj"].Id);
        plu.Reformuler(Reformulation.Relue(
            "Le quartier de la Gare pourra accueillir des immeubles jusqu'à six étages.",
            "La modification du plan local d'urbanisme relève la hauteur autorisée sur douze "
            + "hectares autour de la gare, en vue de 480 logements dont 30 % de logements sociaux.",
            new DateOnly(2026, 2, 23),
            "Les huit élus minoritaires ont voté contre, invoquant l'absence d'étude sur la "
            + "circulation. Quatre abstentions dans la majorité."));
        fevrier.Inscrire(plu);
        fevrier.ProcesVerbal.Approuver(new DateOnly(2026, 3, 14));

        // --- 14 mars 2026 : procès-verbal en retard --------------------------------------------
        var mars = Seance.Tenir(conseil.Id, new DateOnly(2026, 3, 14), Document(new DateOnly(2026, 3, 5)));
        Emarger(mars, conseil, new DateOnly(2026, 3, 14));

        var ecole = Deliberation.Create(
            NumeroDeliberation.Create(2026, 18),
            "Autorisation de programme, groupe scolaire Jules-Ferry, phase 1",
            ResultatDeliberation.Adoptee,
            Document(new DateOnly(2026, 3, 20)),
            Vote.Create(
            [
                DecompteParGroupe.Create(ensemble, 25, 0, 0),
                DecompteParGroupe.Create(autrement, 0, 3, 3),
                DecompteParGroupe.Create(vivre, 0, 0, 2)
            ]));
        ecole.Classer(Thematique.Education);
        ecole.ChiffrerA(Montant.EnEuros(3_400_000));
        ecole.AttribuerAuRapporteur(sieges["Nguyen"].Id);
        ecole.Reformuler(Reformulation.Relue(
            "L'école Jules-Ferry sera rénovée pour 3,4 M€, avec des travaux pendant deux étés.",
            "Le conseil a validé le financement de la rénovation thermique et de la mise en "
            + "accessibilité de l'école élémentaire. Les classes resteront ouvertes, les travaux "
            + "étant répartis sur les vacances d'été 2026 et 2027.",
            new DateOnly(2026, 3, 24),
            "Trois élus du groupe « Saint-Amand autrement » ont voté contre, jugeant le coût par "
            + "élève trop élevé face à l'école des Tilleuls, non traitée. Cinq abstentions."));
        mars.Inscrire(ecole);

        // Approuvé mais jamais mis en ligne : le retard de publication devient visible dans l'API.
        mars.ProcesVerbal.Approuver(new DateOnly(2026, 4, 16));

        // Le dépôt trie de la plus récente à la plus ancienne ; on rend déjà dans cet ordre.
        return [mars, fevrier, janvier];
    }

    private static void Emarger(Seance seance, ConseilMunicipal conseil, DateOnly date)
    {
        foreach (var siege in conseil.CompositionAu(date))
        {
            seance.ConstaterPresence(PresenceEnSeance.Present(siege.Titulaire.Id));
        }
    }

    private static Budget ConstruireBudget(CodeInsee code)
    {
        var budget = Budget.Voter(code, 2026, NatureDeBudget.Primitif, Document(new DateOnly(2026, 1, 29)));

        // La ligne « action sociale » n'a pas d'équivalent dans les sept thématiques : elle reste
        // non ventilée, et l'API l'annonce par PartVentilee.
        (string Libelle, decimal Montant, Thematique? Theme)[] lignes =
        [
            ("Finances et administration générale", 7_100_000m, Thematique.Finances),
            ("Éducation", 6_200_000m, Thematique.Education),
            ("Action sociale et autres dépenses", 5_800_000m, null),
            ("Transports et voirie", 5_400_000m, Thematique.TransportsEtVoirie),
            ("Urbanisme et logement", 4_600_000m, Thematique.Urbanisme),
            ("Culture et sport", 3_800_000m, Thematique.CultureEtSport),
            ("Environnement", 2_900_000m, Thematique.Environnement),
            ("Sécurité", 2_200_000m, Thematique.Securite)
        ];

        foreach (var (libelle, montant, theme) in lignes)
        {
            budget.Inscrire(LigneBudgetaire.Create(libelle, Montant.EnEuros(montant), theme));
        }

        return budget;
    }

    private static Programme ConstruireProgramme(
        CodeInsee code,
        ConseilMunicipal conseil,
        ListeElectorale majorite,
        IReadOnlyList<Seance> seances)
    {
        var programme = Programme.Presenter(
            code, conseil.Id, majorite.Id, majorite.Nom, Document(new DateOnly(2020, 2, 28)));

        var ecole = seances
            .SelectMany(seance => seance.Deliberations)
            .Single(deliberation => deliberation.Thematique == Thematique.Education);

        var ecoles = Engagement.Create(
            "Rénover deux écoles par mandat",
            Thematique.Education,
            StatutEngagement.PartiellementRealise,
            "Une rénovation votée et engagée (Jules-Ferry). La seconde école n'apparaît dans "
            + "aucune délibération à ce jour.",
            Document(new DateOnly(2026, 3, 24)));
        ecoles.Chiffrer(attendu: 2, constate: 1);
        ecoles.Rattacher(ecole.Id);
        programme.Inscrire(ecoles);

        var arbres = Engagement.Create(
            "Planter 5 000 arbres",
            Thematique.Environnement,
            StatutEngagement.VoteNonRealise,
            "Crédits inscrits au budget 2026 pour 1 800 plantations. Aucun décompte de "
            + "plantations effectives publié.",
            Document(new DateOnly(2026, 1, 29)));
        arbres.Chiffrer(attendu: 5_000, constate: 0);
        programme.Inscrire(arbres);

        programme.Inscrire(Engagement.Create(
            "Créer une brigade de nuit",
            Thematique.Securite,
            StatutEngagement.SansTrace,
            "Aucune délibération ni ligne budgétaire ne mentionne cette création depuis le début "
            + "du mandat, sur l'ensemble des séances publiées à ce jour.",
            Document(new DateOnly(2026, 3, 24))));

        return programme;
    }

    /// <summary>Raccourci : tous les documents de la commune fictive pointent vers la même URL.</summary>
    private static Source Document(DateOnly publication)
        => Source.Create(new Uri(SourceDemo), publication);
}
