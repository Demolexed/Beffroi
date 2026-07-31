import type { ReactNode } from "react";

import styles from "./Etiquette.module.css";

/**
 * `thematique` prend la couleur du conteneur porteur de data-thematique.
 * `alerte` est réservée aux points de désaccord et aux signalements — le rouge n'est pas décoratif.
 * `neutre` est une étiquette contournée, sans charge.
 */
export type VarianteEtiquette =
  | "thematique"
  | "thematiqueSurSombre"
  | "alerte"
  | "neutre";

type Props = {
  variante?: VarianteEtiquette;
  children: ReactNode;
};

export function Etiquette({ variante = "neutre", children }: Props) {
  return (
    <span className={`${styles.etiquette} ${styles[variante]}`}>{children}</span>
  );
}
