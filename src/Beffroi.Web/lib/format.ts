/**
 * Formatage francophone. Node embarque l'ICU complet, contrairement à une image conteneur .NET
 * minimale — d'où l'usage d'`Intl` ici alors que les libellés de sources côté API écrivent les
 * mois en dur.
 */

const DATE_LONGUE = new Intl.DateTimeFormat("fr-FR", {
  day: "numeric",
  month: "long",
  year: "numeric",
});

const EUROS = new Intl.NumberFormat("fr-FR", {
  style: "currency",
  currency: "EUR",
  maximumFractionDigits: 0,
});

const MILLIONS = new Intl.NumberFormat("fr-FR", {
  minimumFractionDigits: 1,
  maximumFractionDigits: 1,
});

/** Une `DateOnly` .NET ("2026-03-14") en toutes lettres. */
export function dateEnFrancais(dateOnly: string): string {
  const [annee, mois, jour] = dateOnly.split("-").map(Number);
  return DATE_LONGUE.format(new Date(annee, mois - 1, jour));
}

/**
 * Montant lisible. Au-delà du million on abrège : « 3,4 M€ » se compare d'un coup d'œil là où
 * « 3 400 000 € » demande de compter les zéros.
 */
export function montant(euros: number): string {
  if (Math.abs(euros) >= 1_000_000) {
    return `${MILLIONS.format(euros / 1_000_000)} M€`;
  }

  return EUROS.format(euros);
}
