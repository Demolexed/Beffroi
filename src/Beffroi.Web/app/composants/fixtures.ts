import type { DeliberationDto } from "@/lib/contrats";

/**
 * Jeux de démonstration du banc d'essai. Calqués sur le jeu Saint-Amand de
 * Beffroi.Infrastructure, avec en plus les cas dégradés que le contrat autorise et que la
 * maquette de référence n'affichait pas.
 */

export const AVEC_RESUME_RELU: DeliberationDto = {
  id: "1e0d9a4c-0000-4000-8000-000000000001",
  numero: "2026/018",
  dateSeance: "2026-03-14",
  objetOfficiel:
    "autorisation de programme, groupe scolaire Jules-Ferry, phase 1",
  reformulation: {
    titre:
      "L'école Jules-Ferry sera rénovée pour 3,4 M€, avec des travaux pendant deux étés.",
    resume:
      "Le conseil a validé le financement de la rénovation thermique et de la mise en accessibilité de l'école élémentaire. Les classes resteront ouvertes, les travaux étant répartis sur les vacances d'été 2026 et 2027.",
    noteDeVote:
      "Trois élus du groupe « Saint-Amand autrement » ont voté contre, jugeant le coût par élève trop élevé face à l'école des Tilleuls, non traitée.",
    estRelu: true,
    dateRelecture: "2026-03-20",
  },
  thematique: "education",
  montant: 3_400_000,
  rapporteur: null,
  resultat: "Adoptee",
  estUnanime: false,
  vote: {
    totalPour: 25,
    totalContre: 3,
    totalAbstentions: 5,
    totalExprimes: 33,
    estUnanime: false,
    decomptes: [],
    positions: [],
  },
  source: {
    libelle: "Procès-verbal de la séance du 14 mars 2026 (42 p.)",
    url: "https://exemple.invalid/saint-amand/pv-2026-03-14",
    datePublication: "2026-03-28",
    dateTeletransmission: "2026-03-20",
  },
};

/** Résumé généré mais pas encore relu : l'interface doit le dire, pas le taire. */
export const AVEC_RESUME_NON_RELU: DeliberationDto = {
  ...AVEC_RESUME_RELU,
  id: "1e0d9a4c-0000-4000-8000-000000000002",
  numero: "2026/009",
  dateSeance: "2026-02-12",
  objetOfficiel:
    "approbation de la modification n° 3 du PLU, secteur Gare-Nord",
  reformulation: {
    titre:
      "Le quartier de la Gare pourra accueillir des immeubles jusqu'à six étages.",
    resume:
      "La modification du plan local d'urbanisme relève la hauteur autorisée sur douze hectares autour de la gare, en vue de 480 logements dont 30 % de logements sociaux.",
    noteDeVote:
      "Les huit élus minoritaires ont voté contre, invoquant l'absence d'étude sur la circulation.",
    estRelu: false,
    dateRelecture: null,
  },
  thematique: "urbanisme",
  montant: null,
  vote: {
    totalPour: 21,
    totalContre: 8,
    totalAbstentions: 4,
    totalExprimes: 33,
    estUnanime: false,
    decomptes: [],
    positions: [],
  },
  source: {
    libelle: "Procès-verbal de la séance du 12 février 2026 (58 p.)",
    url: "https://exemple.invalid/saint-amand/pv-2026-02-12",
    datePublication: "2026-02-26",
    dateTeletransmission: "2026-02-18",
  },
};

/** Cas fréquent en production : le PV ne chiffre pas le vote. */
export const SANS_DECOMPTE: DeliberationDto = {
  ...AVEC_RESUME_RELU,
  id: "1e0d9a4c-0000-4000-8000-000000000003",
  numero: "2026/004",
  dateSeance: "2026-01-29",
  objetOfficiel:
    "attribution du contrat de concession de service public de transport urbain",
  reformulation: {
    titre:
      "Le réseau de bus change d'exploitant, avec deux lignes prolongées vers les zones d'activité.",
    resume:
      "Le contrat de délégation de service public du réseau urbain est confié à un nouvel exploitant pour sept ans. Les fréquences du samedi augmentent, le prix du ticket reste à 1,20 €.",
    noteDeVote: null,
    estRelu: true,
    dateRelecture: "2026-02-04",
  },
  thematique: "transports-et-voirie",
  montant: null,
  estUnanime: true,
  vote: null,
  source: {
    libelle: "Procès-verbal de la séance du 29 janvier 2026 (31 p.)",
    url: "https://exemple.invalid/saint-amand/pv-2026-01-29",
    datePublication: "2026-02-12",
    dateTeletransmission: "2026-02-03",
  },
};

/** Ni reformulation, ni thématique, ni vote : le pire cas que le contrat autorise. */
export const BRUTE: DeliberationDto = {
  id: "1e0d9a4c-0000-4000-8000-000000000004",
  numero: "2026/021",
  dateSeance: "2026-03-14",
  objetOfficiel:
    "Convention constitutive de groupement de commandes relative à la fourniture de gaz naturel et services associés — adhésion de la commune au groupement coordonné par le SIGERLy",
  reformulation: null,
  thematique: null,
  montant: null,
  rapporteur: null,
  resultat: "PriseDActe",
  estUnanime: false,
  vote: null,
  source: {
    libelle: "Procès-verbal de la séance du 14 mars 2026 (42 p.)",
    url: "https://exemple.invalid/saint-amand/pv-2026-03-14",
    datePublication: "2026-03-28",
    dateTeletransmission: "2026-03-20",
  },
};
