# Modèle objet

[← Retour à l'index](README.md)

Le modèle de domaine vit dans `src/SeriousGame.Server/Domain/` (namespaces `Server.Domain*`) — c'est un concern purement serveur (voir [architecture.md](architecture.md)). Les entités décrites ici portent l'état du jeu ; ce qui transite sur le fil est un sous-ensemble aplati en DTOs (`SeriousGame.Shared/Models/Dtos/`), les seuls types réellement partagés entre Client et Server — jamais ces classes de domaine directement.

> Le modèle est en cours de construction : le flux de lobby est implémenté, le cœur de jeu (tours, appels d'offres, formations) est modélisé mais pas encore câblé côté serveur. Les règles de jeu de référence sont dans [`openspec/cadrage.md`](../openspec/cadrage.md) (non versionné).

## Deux phases

Le modèle couvre deux moments distincts, ce qui explique certaines cohabitations :

- **Phase lobby** — avant le démarrage. `Game` rassemble des `Player` (identités de connexion) dans une salle d'attente. Aucune `Company` n'existe encore.
- **Phase partie** — au démarrage, le serveur crée **une `Company` par `Player`** et les rattache au `Game`. À partir de là, `Game.Companies` est la liste des participants faisant autorité ; `Game.Players` reste la vue connexion/reconnexion.

## Groupes d'entités

### Identité et lobby

- **`Player`** — identité de connexion, **persistante** au-delà d'une partie : `Id`, `Nickname`, `ConnectionId`, `IsActive`. Ne référence pas sa `Company` (voir [décisions](#décisions-de-conception)).
- **`Game`** — la partie : paramètres (`MinimumPlayers`, `MaximumPlayers`, `RoundsNumber` — posés par `GameFactory` depuis la config `Game`/`GameOptions`, pas de défaut codé dans l'entité), `IsInProgress`, `Owner`, la collection lobby `Players`, la collection de jeu `Companies`, et l'historique des `Rounds`.

### Entreprise

- **`Company`** — entité de jeu scoped à une partie : `Name`, `PlayerOwner` (le joueur qui la pilote), `Treasury` (évolue via `Deposit`/`Withdraw`, pas de setter public), son `Staff` de consultants, et son état persistant entre tours : `Contracts` (appels d'offres en exécution) et `TrainingEnrollments` (consultants en formation).
- **`Consultant`** — membre du staff : `Firstname`/`Lastname`, `SalaryRequirement` (fixée/renégociée via `SetSalaryRequirement`), `Company`, et ses `Skills`. Un consultant est **« occupé »** s'il figure dans un `Contract` actif ou un `TrainingEnrollment` en cours — état **dérivé**, non stocké.
- **`Skill`** — compétence : `Id`, `Name`, `Level` (progresse d'un cran via `LevelUp`, typiquement à la fin d'une formation). Utilisée à la fois comme compétence *possédée* (par un consultant) et *exigée* (par un tender) — distinction encore à trancher (voir [points ouverts](#points-encore-ouverts)).

### Catalogue d'un tour

- **`Round`** — un tour de la partie : `Order`, `IsCompleted`, le `Game` parent, le catalogue proposé ce tour (`Tenders` + `Trainings`), et les décisions des entreprises (`Applications`).
- **`Tender`** — **définition** d'un appel d'offres (immuable) : `Name`, `Skills` exigés, `Budget`, `RoundsNumber` (durée d'exécution). Ne porte aucun état d'attribution : le gagnant et l'avancement vivent sur `Contract`.
- **`Training`** — définition d'une formation : `Name`, `Skill` produite, `Cost`, `RoundsNumber` (durée).

### Décisions et exécution

- **`TenderApplication`** — candidature d'une `Company` à un `Tender` pour un `Round` : les `AssignedConsultants` engagés et un `Status` (`Pending` → `Won`/`Lost`). Éphémère : le serveur la résout au calcul de tour.
- **`Contract`** — appel d'offres **remporté et en cours** : `Company`, `Tender`, `AssignedConsultants` immobilisés, `RemainingRounds`, `Status` (`Active` → `Completed`). À la fin, le `Budget` du tender est versé à la trésorerie.
- **`TrainingEnrollment`** — consultant **en formation** : `Company`, `Consultant`, `Training`, `RemainingRounds`, `Status` (`InProgress` → `Completed`). À la fin, la compétence est accordée (certifiée) au consultant.

### Socle

- **`BaseModel`** — `Id` (`Guid` en `string`) hérité par la plupart des entités. `Player` et `Skill` ne l'héritent pas (identité fournie en propre).
- **Énumérations** — `Level` (Zero → Expert), `ApplicationStatus`, `ContractStatus`, `EnrollmentStatus`.

## Relations

```
Game
 ├─ Players ............ Player*              (roster lobby)
 ├─ Companies .......... Company*
 │   ├─ PlayerOwner .... Player  (1)
 │   ├─ Staff .......... Consultant*
 │   │   └─ Skills ..... Skill*
 │   ├─ Contracts ...... Contract*
 │   │   ├─ Tender ..... Tender  (1)
 │   │   └─ AssignedConsultants ... Consultant*
 │   └─ TrainingEnrollments ....... TrainingEnrollment*
 │       ├─ Consultant . Consultant  (1)
 │       └─ Training ... Training  (1)
 └─ Rounds ............. Round*
     ├─ Tenders ........ Tender*    (catalogue)  ──▶ Skills exigés : Skill*
     ├─ Trainings ...... Training*  (catalogue)  ──▶ Skill produite : Skill (1)
     └─ Applications ... TenderApplication*
         ├─ Company .... Company  (1)
         ├─ Tender ..... Tender  (1)
         └─ AssignedConsultants ... Consultant*
```

Arêtes clés : `Company ──PlayerOwner──▶ Player` (jamais l'inverse) ; `TenderApplication` gagnante engendre un `Contract` porté par la `Company`.

## Cycle de vie d'un tour

1. **Génération** — le serveur ouvre un `Round` avec un catalogue de `Tenders` et de `Trainings`.
2. **Décisions** — chaque `Company` soumet des `TenderApplication` (candidatures avec consultants engagés) et met des consultants en formation (`TrainingEnrollment`).
3. **Calcul de tour** (côté serveur) :
   - résoudre les `Applications` selon le critère d'attribution → `Won`/`Lost` ; chaque gagnante engendre un `Contract` sur la `Company`.
   - décrémenter `RemainingRounds` des `Contracts` et `TrainingEnrollments` actifs ; à 0, clôturer — verser le `Budget` (contrat) ou accorder la compétence (formation).
   - mettre à jour les trésoreries.
4. **Fin de partie** — après `Game.RoundsNumber` tours, l'entreprise au plus grand chiffre d'affaires gagne.

```
┌──────────────────────────────────────────────────┐
│ 1. Génération                                      │
│    Round + catalogue (Tenders / Trainings)         │
└───────────────────────┬────────────────────────────┘
                        ▼
┌──────────────────────────────────────────────────┐
│ 2. Décisions des entreprises                       │
│    • TenderApplication  (candidatures + staff)     │
│    • TrainingEnrollment (consultants en formation) │
└───────────────────────┬────────────────────────────┘
                        ▼
┌──────────────────────────────────────────────────┐
│ 3. Calcul de tour (serveur)                        │
│    • résout candidatures → Won / Lost              │
│         Won → crée un Contract sur la Company      │
│    • RemainingRounds-- (Contracts / Enrollments)   │
│         == 0 → verse Budget / accorde Skill        │
│    • met à jour les trésoreries                    │
└───────────────────────┬────────────────────────────┘
                        ▼
                tour < Game.RoundsNumber ?
                 ├─ oui ─▶ retour à 1. Génération
                 └─ non ─▶ 4. Fin de partie (plus gros CA gagne)
```

## Décisions de conception

- **`Game → Company → Player`, jamais `Player → Company`.** `Player` est une identité lobby persistante ; `Company` est scoped à une partie. Le lien ne va que de `Company` vers son `PlayerOwner`, pour ne pas coupler l'identité durable à l'état d'une partie précise.
- **« Une company par joueur » est un invariant de génération, pas un type.** `Game.Companies` est une collection : le serveur en crée une par joueur au démarrage, mais la structure autorise déjà le multi-company sans migration si une extension future le veut.
- **`Tender` immuable, `Contract` porte l'exécution.** La définition de l'appel d'offres est séparée de son instance remportée : gagnant, statut et avancement vivent sur `Contract`, jamais sur `Tender`.
- **« Consultant occupé » est dérivé.** Pas de flag stocké : l'occupation se calcule depuis les `Contracts` actifs et `TrainingEnrollments` en cours, pour éviter toute désynchronisation.
- **`Round` = décisions du tour ; `Company` = état persistant entre tours.** Les candidatures éphémères vivent sur le `Round` ; les contrats et formations qui s'étalent sur plusieurs tours vivent sur la `Company`.

## Points encore ouverts

Suivis dans [`openspec/cadrage.md`](../openspec/cadrage.md), à trancher avant/pendant l'implémentation du cœur de jeu :

- **Critère d'attribution** d'un tender (moins-disant / meilleure couverture / premier arrivé). Un choix « moins-disant » ajouterait un champ prix (`Bid`) sur `TenderApplication` ; les autres critères ne changent rien au modèle.
- **`Skill` possédé vs exigé** : la même classe sert de compétence détenue par un consultant et de compétence requise par un tender, avec un `Level` mutable — à scinder (définition de référentiel vs niveau possédé/requis).
