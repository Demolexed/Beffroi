/**
 * Table de présentation des sept thématiques.
 *
 * Les codes reproduisent `Projections.Code()` côté .NET et servent de clé aux tokens CSS de
 * `app/tokens.css`. Les libellés y sont dupliqués sciemment : l'enum est fermé et court, et
 * ajouter une valeur est une décision éditoriale, pas une évolution technique. Les recharger
 * depuis /api/v1/thematiques à chaque carte coûterait plus que ce que la duplication risque.
 */

export const CODES_THEMATIQUES = [
  "education",
  "transports-et-voirie",
  "environnement",
  "finances",
  "urbanisme",
  "securite",
  "culture-et-sport",
] as const;

export type CodeThematique = (typeof CODES_THEMATIQUES)[number];

/** Valeur de `data-thematique` quand la thématique est nulle. Voir `app/tokens.css`. */
export const THEMATIQUE_NEUTRE = "neutre";

const LIBELLES: Record<CodeThematique, string> = {
  education: "Éducation",
  "transports-et-voirie": "Transports et voirie",
  environnement: "Environnement",
  finances: "Finances",
  urbanisme: "Urbanisme",
  securite: "Sécurité",
  "culture-et-sport": "Culture et sport",
};

function estCodeConnu(code: string): code is CodeThematique {
  return (CODES_THEMATIQUES as readonly string[]).includes(code);
}

/**
 * Résout la valeur de `data-thematique`. Un code inconnu — thématique ajoutée côté API avant
 * que le front ne la connaisse — retombe sur le neutre plutôt que d'emprunter une couleur.
 */
export function jetonThematique(code: string | null): string {
  return code !== null && estCodeConnu(code) ? code : THEMATIQUE_NEUTRE;
}

/** Libellé affichable. `null` quand la délibération n'est rattachée à aucune thématique. */
export function libelleThematique(code: string | null): string {
  if (code === null) {
    return "Hors thématique";
  }

  return estCodeConnu(code) ? LIBELLES[code] : code;
}
