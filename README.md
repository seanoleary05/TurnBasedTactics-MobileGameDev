# Turn Based Tactics (CA2 Vertical Slice)

A micro turn-based tactics game for Android. You command a squad of three units on a 6×8 board against three enemies. Defeating enemies earns coins, which you spend on upgrades that make your squad hit harder in the next battle.

- **Release tag:** `v0.2-CA2`
- **Version:** 0.3.0
- **Engine:** Unity 6.6 (6000.6.0f1), 2D URP
- **Platform:** Android only, sideloaded (no store upload)

## Device targets

| | |
|---|---|
| Test device | Samsung Galaxy A15 (SM-A155F), Android 14 |
| Minimum Android | 8.0 (API 26) |
| Target API | 36 |
| Architecture | ARM64 only, IL2CPP |
| Graphics API | Vulkan |
| Orientation | Portrait |
| Frame-rate target | 60 fps |
| Internet | Not needed. The game makes no network calls and has no analytics or ads SDKs. |

## Controls

| Action | How |
|---|---|
| Start the game | Main menu: tap **Play** |
| Device info | Main menu: tap **Info**; tap **Close** or press Back to close it |
| Select a unit | Tap one of your (blue) units. Its tile turns yellow. |
| Move | Tap a blue tile |
| Attack | Tap a red tile (an enemy in range) |
| Wait / deselect | Tap anywhere else. A unit that has moved is then done for the turn. |
| End your turn | Tap **End Turn** (the turn also ends when every unit has acted) |
| Pause / resume | Android Back button, or leaving the app (it pauses automatically). Tap **Resume** to continue. |
| Upgrade | Tap **Upgrade (cost)** when you have enough coins. Each level gives +1 attack. |
| Replay | Tap **Retry** on the Victory/Defeat panel |

**Settings (battle screen):** text size (Small, Medium, Large) and a haptics on/off toggle. Both are saved.

## Build steps

1. Install Unity **6000.6.0f1** with Android Build Support (OpenJDK, Android SDK and NDK).
2. Clone the repo and check out the tag:
   ```
   git clone https://github.com/seanoleary05/TurnBasedTactics-MobileGameDev.git
   cd TurnBasedTactics-MobileGameDev
   git checkout v0.2-CA2
   ```
3. Open the folder in Unity Hub.
4. Open **File > Build Profiles** and select **Android Release**. Check that the Scene List is `MainMenu` (0) then `SampleScene` (1).
5. In **Player Settings > Publishing Settings**, point the keystore at `~/user.keystore` (alias `mygame`). The keystore is not in the repo; see `docs/README.md`.
6. Click **Build** and save the APK to `releases/`.
7. Install: copy the APK to the phone and open it (allow installs from unknown sources), or run `adb install -r releases/<name>.apk`.

## Project layout

| Path | Contents |
|---|---|
| `Assets/Scenes/` | `MainMenu` (boot scene) and `SampleScene` (the battle) |
| `Assets/Scripts/` | Gameplay (`TurnManager`, `GridManager`, `Unit`, `Tile`), input, lifecycle, safe area, haptics, text scaling |
| `Assets/Scripts/Economy/` | Wallet, save/load (PlayerPrefs), HUD, stub shop |
| `Assets/Scripts/Telemetry/` | Local event log (logcat plus `telemetry.log`) |
| `docs/CA2/` | Performance baseline, economy and telemetry map, reflective journal |
| `releases/` | Release-signed APKs |

## Save data and telemetry

- Coins, upgrade level, text size and the haptics setting are saved in PlayerPrefs. The wallet saves on every change and when the app is paused.
- Telemetry is local only. Events print to logcat with the tag `[telemetry]` and are appended to `telemetry.log` in the app's persistent data folder. See `docs/CA2/Economy_Telemetry_Map.md`.
  ```
  adb logcat -s Unity | grep telemetry
  ```

## Known issues

See [KnownIssues.md](KnownIssues.md).

## AI assistance

I used Claude (Anthropic) during development. I asked it to:
- Assess the project against the CA2 rubric.
- Draft `Info.cs` (device info panel) and `StartGame.cs` (async scene load from the main menu).
- Diagnose why the Info button disappeared at runtime (a second `Info` component was hiding its own button).
- Help structure and edit the reflective journal, and draft this README, `KnownIssues.md`, the performance baseline and the telemetry map from my code, commits and profiler captures.

I checked the code by compiling it and testing it in the editor and on my phone. I wired the main menu scene myself. I checked every figure in the documents against my own captures; values that were not measured are marked as not measured.
