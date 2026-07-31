import type { Metadata } from "next";

import { CarteDeliberation } from "@/components/CarteDeliberation";
import { Etiquette } from "@/components/Etiquette";
import { CODES_THEMATIQUES, libelleThematique } from "@/lib/thematiques";

import {
  AVEC_RESUME_NON_RELU,
  AVEC_RESUME_RELU,
  BRUTE,
  SANS_DECOMPTE,
} from "./fixtures";
import styles from "./page.module.css";

export const metadata: Metadata = {
  title: "Banc d'essai — Beffroi",
  robots: { index: false, follow: false },
};

/**
 * Banc d'essai des composants, sur données figées. Ce n'est pas une page du site : elle existe
 * pour voir chaque composant dans ses cas dégradés, que des données réelles ne produisent pas
 * toujours au bon moment. À supprimer le jour où des tests de rendu la remplacent.
 */
export default function BancDEssai() {
  return (
    <main className={styles.page}>
      <div className={styles.avertissement}>
        Banc d&apos;essai interne — données figées, ce n&apos;est pas une page
        du site
      </div>

      <h1 className={styles.titre}>Composants</h1>
      <p className={styles.intro}>
        Chaque composant est montré dans son cas nominal puis dans les cas que
        le contrat de l&apos;API autorise : reformulation absente, vote non
        chiffré, thématique nulle.
      </p>

      <h2 className={styles.cas}>Jetons thématiques</h2>
      <div className={styles.palette}>
        {[...CODES_THEMATIQUES, "neutre"].map((code) => (
          <div className={styles.pastille} data-thematique={code} key={code}>
            <div className={styles.echantillon} />
            <div className={styles.echantillonClair} />
            <div>
              {code === "neutre"
                ? "Hors thématique"
                : libelleThematique(code)}
            </div>
            <div className={styles.code}>{code}</div>
          </div>
        ))}
      </div>

      <h2 className={styles.cas}>Étiquettes</h2>
      <div className={styles.etiquettes} data-thematique="urbanisme">
        <Etiquette variante="thematique">Urbanisme</Etiquette>
        <Etiquette variante="alerte">Brouillon non relu</Etiquette>
        <Etiquette variante="neutre">Voté, non réalisé</Etiquette>
      </div>

      <h2 className={styles.cas}>Cartes de délibération</h2>
      <div className={styles.pile}>
        <CarteDeliberation deliberation={AVEC_RESUME_RELU} />
        <CarteDeliberation deliberation={AVEC_RESUME_NON_RELU} />
        <CarteDeliberation deliberation={SANS_DECOMPTE} />
        <CarteDeliberation deliberation={BRUTE} />
      </div>
    </main>
  );
}
