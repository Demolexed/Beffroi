# Beffroi.Web

Front public de Beffroi : Next.js (App Router) en TypeScript. Client HTTP de `Beffroi.Api`,
sans référence au cœur applicatif — le front consomme l'API publique comme n'importe quel
autre consommateur, ce qui la valide par l'usage.

Le rendu serveur est ici un choix fonctionnel, pas technique : les délibérations et les budgets
doivent être indexables par les moteurs de recherche pour qu'un citoyen les trouve.

## Prérequis

Node 22 (voir `.nvmrc`). Aucun SDK .NET n'est nécessaire pour travailler sur ce projet seul.

```bash
npm install
npm run dev
```

| Commande | Rôle |
| --- | --- |
| `npm run dev` | Serveur de développement |
| `npm run build` | Build de production (inclut la vérification TypeScript) |
| `npm run start` | Sert le build de production |
| `npm run lint` | ESLint |

## Règles d'affichage

Arbitrées le 2026-07-31 en confrontant une maquette de référence au contrat de l'API. Elles
portent sur des points où la maquette et le contrat divergeaient — la maquette montrait moins
que ce que l'API sait dire. Elles s'appliquent à tous les composants.

**La couleur thématique n'est jamais une prop.** Les sept thématiques ont des tokens CSS
(`--th-education`, …) plus une variante claire pour les fonds sombres et un `--th-neutre` pour
les `Thematique` nulles. Les composants posent `data-thematique={code}` — le `code` venant de
l'API, jamais d'une table côté front — et le CSS fait le reste.

**La relecture humaine se signale par délibération, dans les deux sens.** `Reformulation.EstRelu`
donne « Relu le {date} » en discret, `false` donne « Brouillon non relu » en visible. Pas de
badge global « résumés assistés par IA » : le silence ne permet pas de distinguer un texte
vérifié d'un texte dont on ne dit rien, et la crédibilité est le produit.

**Les champs nullables ont des replis explicites.** Pas de `Reformulation` → `ObjetOfficiel`
seul, sans faux résumé. Pas de `Vote` → `Resultat` et `EstUnanime` seuls, sans décompte inventé.
Ce sont les cas fréquents : les procès-verbaux ne chiffrent pas tous les votes.

**Les barres de budget sont proportionnelles au total, pas à la plus grande ligne.** Une ligne à
6 % du budget occupe 6 % de la largeur. L'espace restant matérialise `PartVentilee`, c'est-à-dire
la part qui échappe aux sept thématiques — que le contrat demande explicitement d'afficher.

## Remontées explicites de versions (`overrides`)

`package.json` n'acceptant pas de commentaires, la justification est ici. Même démarche que
la remontée de `Microsoft.OpenApi` dans `Directory.Packages.props` à la racine.

| Paquet | Imposé | Motif | À retirer quand |
| --- | --- | --- | --- |
| `sharp` | `^0.35.3` | CVE libvips héritées (GHSA-f88m-g3jw-g9cj). Seul paquet vulnérable qui tourne **en production** : `next/image` l'utilise côté serveur. | Next livrera `sharp >= 0.35` par défaut |
| `postcss` | `^8.5.25` | GHSA-qx2v-qp2m-jg93, GHSA-6g55-p6wh-862q, GHSA-r28c-9q8g-f849. Build-time uniquement. | Next livrera `postcss >= 8.5.18` par défaut |

## Alertes `npm audit` résiduelles

`npm audit` signale une chaîne `brace-expansion` → `minimatch` → outillage ESLint
(GHSA-mh99-v99m-4gvg, déni de service). Laissée en l'état, sciemment :

- `brace-expansion@1.1.18` installé est **la dernière version publiée de la ligne 1.x** :
  il n'existe aucun correctif à installer. La plage `<=5.0.7` de l'avis écrase les versions
  majeures et attrape tout le 1.x.
- Forcer la ligne 5.x dans `minimatch@3` casserait l'API attendue.
- La chaîne est une dépendance de développement : elle n'est jamais livrée en production.

⚠️ Ne pas lancer `npm audit fix --force` : il propose une rétrogradation vers `next@9.3.3`.
