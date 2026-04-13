# Sprint 2 — Reponses

## Exercice 1 — Cartographie

### 1.1 Classes et interfaces publiques

Types publics du projet `Hotel` :

- Interfaces :
  - `IRoomRepository`
  - `IReservationRepository`
  - `IConfirmationSender`
  - `ICleaningPolicy`
  - `ICleaningNotifier`
  - `IPricingStrategy`
  - `ICancellationPolicy`
- Classes :
  - `BillingService`
  - `BookingService`
  - `CleaningTask`
  - `EmailSender`
  - `FlexibleCancellationPolicy`
  - `HousekeepingScheduler`
  - `InMemoryReservationStore`
  - `InMemoryRoomStore`
  - `Invoice`
  - `InvoiceGenerator`
  - `InvoiceLine`
  - `ModerateCancellationPolicy`
  - `NonRefundableCancellationPolicy`
  - `PricingStrategyFactory`
  - `Reservation`
  - `Room`
  - `RoomAssigner`
  - `SmsSender`
  - `StandardCleaningPolicy`
  - `StandardPricingStrategy`
  - `StrictCancellationPolicy`
  - `SuitePricingStrategy`
  - `TaxCalculator`
  - `VipCleaningPolicy`
  - `FamilyPricingStrategy`
- Enum :
  - `RoomType`

Constat : toute la surface du projet est exposee. Meme des details d'implementation comme `InMemoryRoomStore`, `TaxCalculator`, `PricingStrategyFactory` ou les policies concretes sont accessibles depuis l'exterieur.

### 1.2 Graphe de dependances

Vue simplifiee des dependances de composition et d'usage :

```text
Program
  -> InMemoryReservationStore : IReservationRepository
  -> InMemoryRoomStore : IRoomRepository
  -> EmailSender : IConfirmationSender
  -> SmsSender : ICleaningNotifier
  -> RoomAssigner
  -> PricingStrategyFactory
  -> TaxCalculator
  -> InvoiceGenerator
  -> BookingService
  -> BillingService
  -> HousekeepingScheduler
  -> StandardCleaningPolicy

BookingService
  -> IReservationRepository
  -> RoomAssigner
  -> IConfirmationSender
  -> Reservation

RoomAssigner
  -> IRoomRepository
  -> IReservationRepository
  -> Room
  -> Reservation

BillingService
  -> IReservationRepository
  -> InvoiceGenerator
  -> Invoice

InvoiceGenerator
  -> IRoomRepository
  -> PricingStrategyFactory
  -> IPricingStrategy
  -> TaxCalculator
  -> Reservation
  -> Room
  -> Invoice
  -> InvoiceLine

PricingStrategyFactory
  -> StandardPricingStrategy
  -> SuitePricingStrategy
  -> FamilyPricingStrategy
  -> RoomType

StandardPricingStrategy / SuitePricingStrategy / FamilyPricingStrategy
  -> Room

HousekeepingScheduler
  -> IReservationRepository
  -> ICleaningPolicy
  -> ICleaningNotifier
  -> Reservation
  -> CleaningTask

StandardCleaningPolicy / VipCleaningPolicy
  -> Reservation
  -> CleaningTask

SmsSender
  -> CleaningTask

EmailSender
  -> aucun type metier supplementaire

InMemoryReservationStore
  -> Reservation

InMemoryRoomStore
  -> Room
  -> Reservation

Invoice
  -> InvoiceLine

Reservation
  -> RoomType
```

Relations d'implementation :

- `InMemoryReservationStore` implemente `IReservationRepository`
- `InMemoryRoomStore` implemente `IRoomRepository`
- `EmailSender` implemente `IConfirmationSender`
- `SmsSender` implemente `ICleaningNotifier`
- `StandardCleaningPolicy` et `VipCleaningPolicy` implementent `ICleaningPolicy`
- `StandardPricingStrategy`, `SuitePricingStrategy` et `FamilyPricingStrategy` implementent `IPricingStrategy`
- `FlexibleCancellationPolicy`, `ModerateCancellationPolicy`, `StrictCancellationPolicy` et `NonRefundableCancellationPolicy` implementent `ICancellationPolicy`

Point important : `ICancellationPolicy` et ses implementations sont publiques mais ne sont actuellement utilisees par aucun service ni instanciees dans `Program`. Elles font donc deja partie d'une surface publique inutilement large.

### 1.3 Clusters identifies

- Cluster 1 : Reservation / front desk
  - Classes concernees : `BookingService`, `RoomAssigner`, `Reservation`, `Room`, `RoomType`, `IReservationRepository`, `IRoomRepository`, `InMemoryReservationStore`, `InMemoryRoomStore`, `IConfirmationSender`, `EmailSender`, `ICancellationPolicy` et ses implementations.
  - Justification : ces types portent les regles de reservation, d'affectation de chambre, de validation des sejours et de confirmation client. Un changement sur la prise de reservation, la disponibilite, la politique d'annulation ou le canal de confirmation toucherait principalement ce groupe.
- Cluster 2 : Facturation
  - Classes concernees : `BillingService`, `InvoiceGenerator`, `Invoice`, `InvoiceLine`, `TaxCalculator`, `PricingStrategyFactory`, `IPricingStrategy`, `StandardPricingStrategy`, `SuitePricingStrategy`, `FamilyPricingStrategy`.
  - Justification : ces types changent ensemble quand on modifie les regles de prix, la TVA, la taxe de sejour ou le format de facture. C'est le sous-domaine comptable/financier.
- Cluster 3 : Housekeeping
  - Classes concernees : `HousekeepingScheduler`, `CleaningTask`, `ICleaningPolicy`, `StandardCleaningPolicy`, `VipCleaningPolicy`, `ICleaningNotifier`, `SmsSender`.
  - Justification : ces types portent la planification du menage et la notification des equipes. Un changement de frequence de menage ou de mode de notification touche surtout ce groupe.

Observation de decoupage : `Reservation`, `Room` et les repositories sont aujourd'hui partages par plusieurs clusters. C'est pratique dans le monolithe, mais cela augmente le couplage entre reservation, facturation et housekeeping.

---

## Exercice 2 — Decoupage

### Modules crees

| Module | Justification |
|-------|---------------|
| ... | ... |

### Justification par principe

- **CCP** : (expliquez pourquoi vous avez regroupe certaines classes)
- **CRP** : (expliquez pourquoi vous avez separe certaines classes)
- **REP** : (expliquez la coherence de chaque module)

---

## Exercice 3 — Test de la modification

### Scenario A — Politique de menage

- Fichiers modifies : ...
- Modules impactes : ...
- Principe en jeu : ...

### Scenario B — Taux de TVA

- Fichiers modifies : ...
- Modules impactes : ...

### Scenario C — Push notification

- Fichiers crees : ...
- Fichiers modifies : ...
- Modules metier impactes : ...
- Principe en jeu : ...

### Comparaison avec le code de depart

(Paragraphe d'analyse)
