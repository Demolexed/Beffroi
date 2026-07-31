import type { VoteDto } from "@/lib/contrats";

import styles from "./ResultatDeVote.module.css";

/** Miroir de l'enum ResultatDeliberation. « Unanimité » n'y figure pas : c'est une propriété du vote. */
const RESULTATS: Record<string, string> = {
  Adoptee: "Adoptée",
  Rejetee: "Rejetée",
  Retiree: "Retirée de l'ordre du jour",
  Ajournee: "Ajournée",
  PriseDActe: "Prise d'acte, sans vote",
};

type Props = {
  vote: VoteDto | null;
  resultat: string;
  estUnanime: boolean;
  /** Commentaire éditorial sur le vote, tiré de la reformulation. */
  note?: string | null;
};

/**
 * Décompte des voix, ou repli lisible quand le procès-verbal ne le donne pas — cas fréquent :
 * les PV ne chiffrent pas systématiquement les votes. On n'invente jamais de décompte, on dit
 * ce qu'on sait et on s'arrête là.
 */
export function ResultatDeVote({ vote, resultat, estUnanime, note }: Props) {
  const libelleResultat = RESULTATS[resultat] ?? resultat;

  return (
    <div className={styles.bloc}>
      <div className={styles.intitule}>Vote du conseil</div>

      {vote === null ? (
        <>
          <div className={styles.resultat}>{libelleResultat}</div>
          <p className={styles.sansDecompte}>
            {estUnanime
              ? "À l'unanimité. Le procès-verbal ne publie pas le décompte des voix."
              : "Le procès-verbal ne publie pas le décompte des voix."}
          </p>
        </>
      ) : (
        <>
          <div className={styles.decompte}>
            <div className={styles.ligne}>
              <span className={styles.nombre}>{vote.totalPour}</span>
              <span>pour</span>
            </div>
            <div className={`${styles.ligne} ${styles.contre}`}>
              <span className={styles.nombre}>{vote.totalContre}</span>
              <span>contre</span>
            </div>
            <div className={`${styles.ligne} ${styles.abstentions}`}>
              <span className={styles.nombre}>{vote.totalAbstentions}</span>
              <span>
                {vote.totalAbstentions === 1 ? "abstention" : "abstentions"}
              </span>
            </div>
          </div>
          {note ? <p className={styles.note}>{note}</p> : null}
        </>
      )}
    </div>
  );
}
