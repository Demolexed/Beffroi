import type { DeliberationDto } from "@/lib/contrats";
import { dateEnFrancais, montant } from "@/lib/format";
import { jetonThematique, libelleThematique } from "@/lib/thematiques";

import { Etiquette } from "./Etiquette";
import { ResultatDeVote } from "./ResultatDeVote";
import styles from "./CarteDeliberation.module.css";

type Props = {
  deliberation: DeliberationDto;
};

/**
 * Une décision du conseil.
 *
 * Deux principes qui expliquent la forme du composant :
 *
 * - Le texte éditorial de Beffroi et le verbatim de l'acte ne se confondent jamais. Quand la
 *   reformulation manque, on affiche `objetOfficiel` tel quel, dans un style qui dit que c'est
 *   du langage administratif — plutôt que de fabriquer un titre.
 * - La relecture humaine se signale dans les deux sens. Ne rien dire laisserait le lecteur
 *   incapable de distinguer un texte vérifié d'un texte dont on ne dit rien.
 */
export function CarteDeliberation({ deliberation }: Props) {
  const { reformulation, source } = deliberation;

  return (
    <article
      className={styles.carte}
      data-thematique={jetonThematique(deliberation.thematique)}
    >
      <div className={styles.bandeau} />

      <div className={styles.corps}>
        <div className={styles.principal}>
          <div className={styles.chapeau}>
            <Etiquette variante="thematique">
              {libelleThematique(deliberation.thematique)}
            </Etiquette>
            <span className={styles.date}>
              {dateEnFrancais(deliberation.dateSeance)}
            </span>
          </div>

          {reformulation ? (
            <>
              <h3 className={styles.titre}>{reformulation.titre}</h3>
              <p className={styles.resume}>{reformulation.resume}</p>
              <div className={styles.relecture}>
                {reformulation.estRelu ? (
                  <span className={styles.relu}>
                    Résumé relu
                    {reformulation.dateRelecture
                      ? ` le ${dateEnFrancais(reformulation.dateRelecture)}`
                      : ""}
                  </span>
                ) : (
                  <Etiquette variante="alerte">Brouillon non relu</Etiquette>
                )}
              </div>
            </>
          ) : (
            <>
              <h3 className={`${styles.titre} ${styles.titreOfficiel}`}>
                {deliberation.objetOfficiel}
              </h3>
              <p className={styles.sansResume}>
                Cette délibération n&apos;a pas encore de résumé en langage
                clair. Le texte ci-dessus est l&apos;intitulé officiel de
                l&apos;acte.
              </p>
            </>
          )}

          <div className={styles.pied}>
            <span>
              Délibération n° {deliberation.numero}
              {reformulation ? ` — ${deliberation.objetOfficiel}` : ""}
            </span>
            {deliberation.montant !== null ? (
              <span className={styles.montant}>
                {montant(deliberation.montant)}
              </span>
            ) : null}
            <a
              className={styles.source}
              href={source.url}
              rel="noreferrer"
              target="_blank"
            >
              {source.libelle}
            </a>
          </div>
        </div>

        <ResultatDeVote
          estUnanime={deliberation.estUnanime}
          note={reformulation?.noteDeVote}
          resultat={deliberation.resultat}
          vote={deliberation.vote}
        />
      </div>
    </article>
  );
}
