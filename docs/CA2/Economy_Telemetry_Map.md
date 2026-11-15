# Economy and Telemetry Map (CA2)

## Economy loop: reward → spend → upgrade

| Step | What happens | Code |
|---|---|---|
| **Reward** | Each enemy defeated pays **40 coins** (`coinsPerKill`, set in the scene). A won battle pays 120. | `TurnManager.OnUnitDied` → `Wallet.Earn` |
| **Spend** | **Upgrade** costs `100 × 1.5^level`: 100, 150, 225, 337, 506. The button is disabled until you can afford it. | `WalletHud.OnUpgradePressed` → `Wallet.TryUpgrade` |
| **Upgrade** | Each level adds **+1 attack** to every player attack (base ATK 2; enemies have 5 HP). | `TurnManager.UpgradeAtkBonus` |
| **Persist** | Coins and level are saved in PlayerPrefs (`wallet.coins`, `wallet.level`) on every change and on pause. The wallet survives Retry and app restarts. | `WalletPersistence`, `WalletSave` |
| **Store stub** | "Buy pack" on the result panel calls a fake purchase provider that grants 500 coins after 0.5 s. No real billing. | `CoinShop` → `FakePurchaseProvider` |

**Demo path:** win one battle (120 coins) → Upgrade (100) → Retry → attacks now deal 3 damage.

## Telemetry events

Every event is printed to logcat as `[telemetry] <time> <session> <event> key=value ...` and buffered to `telemetry.log` (written on pause, on quit, or every 64 lines). Each line includes the seconds since launch and an 8-character session ID. No third-party SDK is used.

| Event | Parameters | Fires when | Source |
|---|---|---|---|
| `session_start` | `app_version`, `device_model`, `android` | The app boots (battle scene's bootstrap object wakes) | `MobileBootstrap.Awake` |
| `info_opened` | `device_model` | The player opens the Info panel on the main menu | `Info.Show` |
| `menu_play` | `scene` | The player taps Play on the main menu | `StartGame.OnPlayPressed` |
| `level_start` | `level`, `players`, `enemies` | A battle begins, after units spawn | `TurnManager.Start` |
| `level_complete` | `level`, `turns`, `survivors`, `seconds` | All enemies are defeated | `TurnManager.EndBattle(true)` |
| `level_fail` | `level`, `turns`, `survivors`, `seconds` | All player units are defeated | `TurnManager.EndBattle(false)` |
| `level_retry` | `level` | The player taps Retry on the result panel | `TurnManager.Restart` |
| `upgrade_bought` | `level`, `cost`, `coins_left`, `t` | An upgrade purchase succeeds | `WalletHud.OnUpgradePressed` |

**Line format (illustrative values; replace with real lines from your logcat):**
```
[telemetry] 41.37 3f9a1c2e level_complete level=battle_01 turns=4 survivors=2 seconds=38.6
[telemetry] 44.02 3f9a1c2e upgrade_bought level=1 cost=100 coins_left=20 t=44.0
```

**How to view:** `adb logcat -s Unity | grep telemetry`. The file is at `/sdcard/Android/data/com.seanoleary.turnbasedtactics/files/telemetry.log`.

**Note:** `session_start` is logged by `MobileBootstrap`, which lives in the battle scene, so it fires when the battle loads rather than at the main menu.
