# Dev Journal

## Week 6 Lab A: Economy loop (Mon 12 Oct 2026)

**Core verb -> earn:** defeating an enemy unit pays `coinsPerKill` (TurnManager inspector).
**Upgrade:** each wallet level adds +1 damage to every player attack (base ATK 2, enemy HP 5).
**Cost curve:** `100 * 1.5^level` -> 100, 150, 225, 337, 506.
**Store:** `CoinShop` -> `IPurchaseProvider` -> `FakePurchaseProvider` (logs `[iap-stub]`), Result panel only.
**Persistence:** PlayerPrefs `wallet.coins`, `wallet.level`, saved on every change and on pause.

### Pacing pass (3 min, phone, stopwatch, fresh PlayerPrefs)

| | Earn per kill | First upgrade at | Coins at 3:00 | Upgrades bought |
|---|---|---|---|---|
| Before | 10 | TODO | TODO | TODO |
| After  | TODO | TODO | TODO | TODO |

Target: first upgrade inside 2:00, about the 5th by 10:00.

**Why the upgrade is felt:** TODO (e.g. at +1 ATK a 5 HP enemy drops in 2 hits instead of 3, so each battle takes visibly fewer turns).
