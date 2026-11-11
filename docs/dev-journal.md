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
| After  | 40 | TODO | TODO | TODO |

**Tuning change (11 Nov):** `coinsPerKill` 10 -> 40. At 10, a full win (3 kills) paid 30 coins, so the first upgrade (100) took about 4 wins and missed the 2:00 target. At 40 one win pays 120, so the first upgrade comes after the first battle. The cost curve is unchanged.

Target: first upgrade inside 2:00, about the 5th by 10:00.

**Why the upgrade is felt:** TODO (e.g. at +1 ATK a 5 HP enemy drops in 2 hits instead of 3, so each battle takes visibly fewer turns).
