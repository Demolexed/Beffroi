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
