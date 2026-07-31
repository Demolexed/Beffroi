/**
 * Miroir TypeScript du contrat public de Beffroi.Api
 * (src/Beffroi.Core/Application/Contracts/Contrats.cs).
 *
 * Écrit à la main plutôt que généré depuis le document OpenAPI : tant que le contrat bouge à
 * chaque nouvel agrégat, une génération ajouterait une étape de build sans supprimer la
 * relecture. À reconsidérer quand l'API se stabilisera — `openapi-typescript` lit déjà le
 * document que l'API expose.
 *
 * Les `DateOnly` de .NET arrivent en JSON sous la forme "2026-03-14".
 */

export type SourceDto = {
  /** Description prête à afficher, composée côté serveur. Ex. « Budget primitif 2026 ». */
  libelle: string;
  url: string;
  datePublication: string;
  dateTeletransmission: string | null;
};

/**
 * Mise en langage clair produite par Beffroi. `estRelu` distingue un texte validé par un humain
 * d'un brouillon généré — l'interface doit le signaler dans les deux sens.
 */
export type ReformulationDto = {
  titre: string;
  resume: string;
  noteDeVote: string | null;
  estRelu: boolean;
  dateRelecture: string | null;
};

export type DecompteParGroupeDto = {
  groupe: string;
  pour: number;
  contre: number;
  abstentions: number;
};

export type PositionIndividuelleDto = {
  elu: string;
  position: string;
};

export type VoteDto = {
  totalPour: number;
  totalContre: number;
  totalAbstentions: number;
  totalExprimes: number;
  estUnanime: boolean;
  decomptes: DecompteParGroupeDto[];
  positions: PositionIndividuelleDto[];
};

/**
 * Une décision du conseil. `objetOfficiel` est le verbatim de l'acte ; `reformulation` est le
 * texte éditorial de Beffroi, distinct et signalé comme tel.
 *
 * `reformulation`, `vote` et `thematique` sont nullables et le sont souvent en pratique : les
 * procès-verbaux ne chiffrent pas tous les votes.
 */
export type DeliberationDto = {
  id: string;
  numero: string;
  dateSeance: string;
  objetOfficiel: string;
  reformulation: ReformulationDto | null;
  thematique: string | null;
  montant: number | null;
  rapporteur: string | null;
  resultat: string;
  estUnanime: boolean;
  vote: VoteDto | null;
  source: SourceDto;
};
