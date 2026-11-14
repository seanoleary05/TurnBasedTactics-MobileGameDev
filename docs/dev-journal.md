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

### Pacing pass (calculated from the code values)

These figures are calculated from the values in the code: 3 enemies per battle, `coinsPerKill` set in the scene, and the cost curve above. They count battles won, not minutes, because I haven't done a timed stopwatch run on the phone yet. Coins carry over between battles because `Wallet` persists across Retry and is saved in PlayerPrefs.

Cumulative cost of upgrades: 100, 250, 475, 812, 1,318 coins.

| | Earn per kill | Coins per win | 1st upgrade after | 2nd | 3rd | 5th |
|---|---|---|---|---|---|---|
| Before | 10 | 30 | 4th win | 9th win | 16th win | 44th win |
| After  | 40 | 120 | 1st win | 3rd win | 4th win | 11th win |

**Tuning change (11 Nov):** `coinsPerKill` 10 -> 40 (`3c99052`). At 10, the first upgrade needed four wins, which missed the target of a first upgrade within about 2 minutes. At 40, it comes straight after the first battle, so the reward → spend → upgrade loop can be seen in a single demo. The cost curve is unchanged, so later upgrades still slow down (the 5th needs 11 wins).

Target: first upgrade inside 2:00, about the 5th by 10:00. The first part is met as long as one battle takes under 2 minutes. The second would need a battle of under a minute, so the late curve is probably still too slow. That needs a timed run to confirm.

**Why the upgrade is felt:** player units start at ATK 2 and enemies have 5 HP (`Unit.prefab`). An enemy takes 3 hits at base, and 2 hits after the first upgrade (ATK 3). The second upgrade (ATK 4) still needs 2 hits, and the third (ATK 5) makes every hit a kill. So upgrades 1 and 3 are the ones that visibly shorten battles, and upgrade 2 adds nothing the player can see. That is worth fixing in CA3 by tuning enemy HP or the bonus per level.

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
  - Fixed a compile break before the showcase. An uncommitted edit had removed `Wallet.Instance`, which `WalletHud`, `TurnManager`, `WalletPersistence` and `CoinShop` all use. Restored the singleton (`eaf9e9a`).
  - Raised `coinsPerKill` from 10 to 40 so the first upgrade fits in a demo. Removed a stray "New Text" label from `WalletHud` and resized the HUD labels (`3c99052`).
  - Built `releases/turnbasedtactics-0.3.0.apk` (33 MB) for the showcase. I forgot to bump the version code: it is still 2, the same as v0.2.0.
  - Revised the reflective journal (`653c276`).

## Week 10 (Sat 14 Nov): Settings and bottleneck fix

- **Haptics toggle** (`961cbf3`): added a button (`DisableHaptics`) that switches vibration on and off. The setting is saved in PlayerPrefs (`haptics` key) by `Haptics.Enabled`, so it survives a relaunch. This covers the "vibration optional" requirement in the brief.
- **Font stutter fix** (`f1b07d9`): looked at the 938.3 ms worst frame from the 9 Nov capture again. The markers were `FontEngine.LoadFontFace_From_File`, `File.Read` and `FontEngine.Get_GPOS_Lookups`, so TextMesh Pro was opening the font file during play. I first assumed the font atlas was dynamic, but `LiberationSans SDF` is already static and covers all ASCII, and all game text is ASCII. The real causes were two settings:
  - `GetFontFeaturesAtRuntime` was on in `TMP Settings`, so TextMesh Pro read kerning data from the font file at runtime. I turned it off. Kerning still works because the static asset already stores its kerning pairs.
  - The static font had a dynamic fallback font (`LiberationSans SDF - Fallback`), which loads the raw font file when it is used. I removed it from the fallback list. Any missing character now shows as □ instead of loading a font.
- **Telemetry buffering** (same commit): `Telemetry.Log` used to call `File.AppendAllText` on every event, which is blocking disk I/O on the main thread. Lines are now buffered in memory. They are written on `OnApplicationPause(true)`, on quit, or after 64 lines, by a hidden flusher object that persists across scene loads. Logcat output is unchanged.
- Checked that the scripts compile (`dotnet build`, no errors), wrote up the analysis and approach in section 2.2 of the reflective journal, and pushed to GitHub.
- The fix hasn't been measured on the device yet, so section 2.2 of the reflective journal has no "after" numbers.
