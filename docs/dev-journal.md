# Dev Journal

Entries are rebuilt from the commit history (`git log`). Commit hashes are given in brackets so each entry can be checked.

## Week 2 (14 to 18 Sep): Project setup and platform basics

- **Mon 14 Sep:** Created the Unity 6.6 2D URP project and the GitHub repo (`d26a295`). Set up `.gitignore` and `docs/README.md`. Added `MobileBootstrap`, which sets 60 fps and logs the device, OS, graphics API and resolution at boot.
- **Tue 15 Sep:** Wrote the MDA one-pager and the scope lock (`2320013`, `a9e49fa`). Confirmed that adb detects the phone and that the project runs on the Galaxy A15.
- **Tue 15 Sep (Lab A, PR #1):** Added tap and swipe input (`TapSwipeInput`), safe-area handling (`SafeArea`), and the Android Dev and Release build profiles (`fc67392`). Took the first profiler capture and the launch logcat for the CA1 evidence.
- **Wed 16 Sep:** Added pause and lifecycle handling (`0138cab`). `LifecycleGuard` pauses the game when it loses focus, and `PauseMenu` shows a Resume panel. Escape also toggles the pause in the editor. Added the first grid script (`GridInital`) and a placeholder prefab.
- **Thu 17 Sep:** Noted haptics as the next task (`5df1f43`).

## Week 3 (21 to 25 Sep): Grid, haptics and text scaling

- **Mon 21 Sep:** Started the grid layout and added `Haptics` for a short vibration on hit (`a27f2db`).
- **Thu 24 Sep:** Added Small, Medium and Large text-size options (`TextScale` plus three buttons) for accessibility (`ffc06fb`). Merged as PR #2 on Sun 27 Sep (`70e7c2b`).

## Week 4 (28 Sep to 2 Oct): Performance baseline and release pipeline

- **Mon 28 Sep:** Profiled on the device. The worst main-thread frame was 20 ms, the tallest marker was Rendering, and there were no GC collects (recorded in `docs/CA1/journal.md`).
- **Tue 29 Sep:** Added two profiling tools:
  - `RenderScaleProbe` (`68b6e97`) toggles the URP render scale to 0.5× to test whether the game is GPU-bound or CPU-bound.
  - `FrameTimeSampler` (`6f45791`) logs the average and p99 frame time every 600 frames.
- **Wed 30 Sep:** Started the baseline sheet in `docs/CA2/baseline/` (`d3226e2`, `12fe6bd`).
- **Fri 2 Oct:** Documented the keystore. It is kept outside the repo, with the alias `mygame` and a 50-year validity (`2399f1f`, `b12c200`). Hardened the release pipeline: v0.2.0, versionCode 2 (`3e533b8`).

## CA1 submission (3 to 4 Oct)

- Added the privacy statement, store descriptions, store-assets checklist, device screenshot and install proof (`402f360`, `cb3fd00`).
- Packaged the CA1 journal (`30177be`), and added the AI-use addendum and game details to the README (`bea2369`, `47ffe87`).
- Changed `.gitignore` so APKs are committed (`0bcbdf2` to `3ad13bf`).
- **Note:** commit `402f360` also committed the IL2CPP backup folder from a build (`*_BackUpThisFolder_ButDontShipItWithYourGame`). That folder is large generated output and shouldn't be in the repo.

## Week 5 (5 to 9 Oct): Tile refactor

- **Thu 8 Oct:** Began refactoring the grid into a `Tile` prefab and component that stores its own coordinate (`dbc4bba`). The aim was for the turn system to ask a tile what it is showing, instead of `GridManager` setting colours directly.

## Week 6 Lab A: Economy loop (Mon 12 Oct 2026)

**Core verb -> earn:** defeating an enemy unit pays `coinsPerKill` (TurnManager inspector).
**Upgrade:** each wallet level adds +1 damage to every player attack (base ATK 2, enemy HP 5).
**Cost curve:** `100 * 1.5^level` -> 100, 150, 225, 337, 506.
**Store:** `CoinShop` -> `IPurchaseProvider` -> `FakePurchaseProvider` (logs `[iap-stub]`), Result panel only.
**Persistence:** PlayerPrefs `wallet.coins`, `wallet.level`, saved on every change and on pause.

This was the design. The code was committed in Week 9 (`185af5a`).

### Pacing pass (3 min, phone, stopwatch, fresh PlayerPrefs)

| | Earn per kill | First upgrade at | Coins at 3:00 | Upgrades bought |
|---|---|---|---|---|
| Before | 10 | TODO | TODO | TODO |
| After  | 40 | TODO | TODO | TODO |

**Tuning change (11 Nov):** `coinsPerKill` 10 -> 40. At 10, a full win (3 kills) paid 30 coins, so the first upgrade (100) took about 4 wins and missed the 2:00 target. At 40 one win pays 120, so the first upgrade comes after the first battle. The cost curve is unchanged.

Target: first upgrade inside 2:00, about the 5th by 10:00.

**Why the upgrade is felt:** TODO (e.g. at +1 ATK a 5 HP enemy drops in 2 hits instead of 3, so each battle takes visibly fewer turns).

## Weeks 6 to 7 and reading week (12 Oct to 1 Nov): Gap

- There are no commits between 8 Oct and 1 Nov. The economy design above was written in this period, but it wasn't implemented until Week 9.
- **Sun 1 Nov:** Added alternating light and dark tile colours, so distance on the grid is easier to read on a small screen (`2e02dd5`).

## Week 8 (2 to 6 Nov): Core battle loop

- **Mon 2 Nov:** Continued the `Tile` work (`181864e`).
- **Wed 4 Nov:** Added `Telemetry`, which writes events to logcat and to `telemetry.log` in `persistentDataPath`. Renamed `GridInital` to `GridManager` (`77a8d6e`).
- **Thu 5 Nov:** Built the core loop in one afternoon:
  - `Highlight` enum so tiles show Selected, Move or Attack (`a3cf542`)
  - `Unit` prefab and rendering (`c13162c`)
  - `TurnManager` (`4edcef4`): spawns three player units and three enemies, handles select, move, attack and End Turn, and runs the enemy AI. Each enemy walks towards the nearest player unit and attacks the weakest adjacent one.

## Week 9 (9 to 13 Nov): Polish, profiling, economy and showcase

- **Mon 9 Nov:**
  - Health numbers shown over units. Added `DebugSpawn` as a test harness for movement and attack ranges (`7f9d6cb`).
  - Took the thermal-soak profiler capture (`245c45f`). Median frame time 22.18 ms; the worst frame was 938.3 ms, caused by TextMesh Pro loading a font at runtime. Analysis is in `docs/CA2/Reflective_Journal.md`.
  - Implemented the economy from Week 6 (`185af5a`): `Wallet`, `WalletHud`, `WalletPersistence`/`WalletSave`, and `CoinShop` with the `IPurchaseProvider` stub. Kills now pay coins in `TurnManager`, and each upgrade adds +1 ATK.
- **Tue 10 Nov:** Drafted the CA2 reflective journal (`286c65f`).
- **Wed 11 Nov (showcase day):**
  - Raised `coinsPerKill` from 10 to 40 so the first upgrade fits in a demo. Removed a stray "New Text" label from `WalletHud` and resized the HUD labels (`3c99052`).
  - TODO: release build v0.3.1 (versionCode 4), install on the A15, run the showcase, and record how it went.
