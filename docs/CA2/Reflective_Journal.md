# Reflective Development Journal: CA2 Vertical Slice

**Project:** Turn Based Tactics (Option 3, micro turn-based tactics)
**Device:** Samsung Galaxy A15 (SM-A155F), Android 14, Vulkan, 1080×2340, 90 Hz
**Engine:** Unity 6.6, IL2CPP, ARM64
**Release tag:** `v0.2-CA2` <!-- TODO: confirm tag name -->

> DRAFT. Items marked `[TODO]` need real numbers from the device before submission. Do not submit with placeholders.

---

## Part 1: Weekly log (W5 to W9)

### W5 (5 to 9 Oct)
- Started refactoring the board into a `Tile` component with its own coordinate and highlight state (commit `dbc4bba`), replacing the single loose grid script from W3.
- Reason: the turn system needs to ask a tile "what are you showing?", and that was impossible when colours were set directly from `GridManager`.

### W6 (12 to 16 Oct)
- Designed the economy loop in `dev-journal.md`: earn coins per kill, spend on a wallet level, each level adds +1 attack.


### W7 (19 to 23 Oct)
- Planned: Adaptive Performance and a thermal soak. 

### Reading week (26 Oct to 1 Nov)
- Alternating light/dark tile colours (`2e02dd5`) so grid distance can be read at a glance on a small screen.

### W8 (2 to 6 Nov)
- Added a local telemetry stub (`77a8d6e`) that writes to logcat and to `telemetry.log` in `persistentDataPath`.
- Added the `Highlight` enum (`a3cf542`), unit rendering (`c13162c`) and the full `TurnManager` (`4edcef4`): select, move, attack, End Turn, plus enemy AI that approaches the nearest player and hits the weakest one.

### W9 (9 to 13 Nov)
- Health numbers drawn over units (`7f9d6cb`). Profiler and thermal captures taken (`245c45f`). Wallet, shop stub and HUD integrated (`185af5a`).
- 11 Nov, showcase prep:
  - Fixed a compile break. An uncommitted edit had deleted `Wallet.Instance`, and `WalletHud`, `TurnManager`, `WalletPersistence` and `CoinShop` all depend on it. I restored the singleton (`eaf9e9a`).
  - Pacing: `coinsPerKill` raised from 10 to 40 (`3c99052`). At 10, a win paid 30 coins, so the first upgrade (100) took about 4 battles, which missed the 2:00 target in `dev-journal.md`. At 40, one win pays 120, so the upgrade can be bought after the first battle.
  - Removed a stray default "New Text" label from the `WalletHud` object, and resized the coin and upgrade labels.
- [TODO: device-tier presets, showcase outcome, final docs]

---

## Part 2: Structured reflection

The reflection uses Gibbs' (1988) cycle, shortened to **Description → Evaluation → Analysis → Action**.

### 2.1 What changed in scope or design, and why?

**Description.** The CA1 pitch was a linear dungeon of several encounters with a party of 3 to 4 units, loot rewards, and per-character stats such as speed. The CA2 slice is one 6×8 battle, three player units against three enemies, with coins as the only reward and one global upgrade (+1 ATK per level).

**Evaluation.** The cut was the right call for a vertical slice. The brief asks for "production-ready feel for one level/encounter", and a single battle that plays well matches that better than a thin dungeon would. The cost is that the economy has only one sink, so the "reward → spend → upgrade" loop pays off only across replays, not within a single run.

**Analysis.** Two pieces of evidence drove the cut. First, the commit history shows a gap of about three weeks with no commits (8 Oct to 1 Nov). Most of the core loop (`Highlight`, `Unit`, `TurnManager`) was committed between 2 and 5 Nov. With that little time left, adding more encounters would have meant shipping a weaker loop. Second, the board size was set by the screen, not by design. `GridManager.CenterCamera` fits the board to portrait width with `orthographicSize = max(h/2 + 1, (w/2 + 0.5)/aspect)`. On a 1080×2340 screen a 6-wide board gives tiles of about [TODO] dp, well above the 44 dp minimum touch target (Apple, 2024). A wider board would push tiles towards that limit, so 6×8 was a constraint, not a free choice. Fixing upgrades at +1 ATK against 5 HP enemies means one upgrade turns a 3-hit kill into a 2-hit kill (base ATK 2 → 3), a change the player can actually see.

**Action.** For CA3, add a second encounter only if pacing data shows players buy their first upgrade before the run ends. Otherwise the economy has nowhere to go.

### 2.2 Key problem → approach → outcome

**Problem (Description).** The thermal-soak capture (`docs/CA2/baseline/thermal-soak/before.data`) shows a **median frame time of 22.18 ms** (about 45 fps, against a 60 fps target) and a **worst frame of 938.3 ms** (frame 762). The top self-time markers on that frame are `IMGUIModule GUIStyle…`, `EngineJob` (450.5 ms), `FontEngine.LoadFontFace_From_File` (61.9 ms), `File.Read` (58.8 ms) and `FontEngine.Get_GPOS_Lookups` (53.1 ms). In CA1, the worst main-thread frame was 20 ms, with Rendering as the tallest marker.

**Evaluation.** A single frame close to one second is a visible hitch, which is the "stable frame times" problem the guardrails warn about. It matters more than the median, because players notice one long stutter more than an average that is slightly slow (Unity Technologies, 2024a).

**Analysis.** The markers point to **a font file being opened and its kerning tables read at runtime, on the main thread, during play**. It is not rendering cost. `LoadFontFace_From_File` and `File.Read` mean TextMesh Pro is reading a font file from disk, and `Get_GPOS_Lookups` means it is reading the font's GPOS (kerning) table.

My first guess was that the game's font atlas was *dynamic* and was filling itself in as new characters appeared. Checking the project disproved this:
- `LiberationSans SDF` is already a **static** atlas with 250 characters, covering all printable ASCII.
- Every string the game shows is plain ASCII: the 12 scene labels plus the text set from `TurnManager`, `Unit` and `WalletHud`.

So no glyph was ever missing. The file reads came from two project settings:
1. `GetFontFeaturesAtRuntime` was enabled in `TMP Settings`. This makes TextMesh Pro load the source font and read its kerning data at runtime, even though the static asset already stores its kerning pairs.
2. The static font listed a **dynamic fallback** (`LiberationSans SDF - Fallback`) as a backup font, set to clear its data on build. Any lookup that reached it had to load the raw `.ttf` from disk and build its tables from scratch.

Either one explains a one-off stall the first time text is laid out. No game script uses `OnGUI`, so the IMGUI marker probably comes from the development-build overlay, and it should be checked in a release build before it is blamed on the game.

A second, smaller cause: `Telemetry.Log` called `File.AppendAllText` synchronously for every event. It only fires on events, not every frame, but it is blocking I/O on the main thread and lands on the same frames as `level_start` and `level_complete`.

**Approach.** I made three changes, each aimed at a single cause:
1. **Turned off runtime font features** (`TMP Settings.asset`: `GetFontFeaturesAtRuntime` 1 → 0). Kerning still works, because the static asset already contains the kerning pairs, so the font file never needs opening during play (Unity Technologies, 2024b).
2. **Removed the dynamic fallback** from the static font's fallback list (`LiberationSans SDF.asset`). The static atlas is now the only font used at runtime. The trade-off is deliberate: a character outside the atlas now shows a □ instead of quietly loading a font file. That is acceptable while all game text is ASCII, and it makes any future missing character visible during testing instead of a hidden stutter.
3. **Buffered telemetry** (`Telemetry.cs`). `Log()` still writes to logcat straight away. The file line now goes into an in-memory buffer instead of to disk on that frame. `Flush()` writes to `telemetry.log` on `OnApplicationPause(true)`, on quit, or after 64 lines. A hidden object that persists across scene loads handles this, so it keeps working after Retry. I chose pause over level end because Android usually kills a backgrounded app without calling `OnApplicationQuit`, so pause is the last reliable chance to write. The trade-off is that a hard crash loses the unflushed lines, which is acceptable for a local, debug-only log.

I rejected **warming up the text during a loading screen**. It would only move the stall to a loading screen that the slice doesn't have, instead of removing it.

**Outcome.**

| Metric | Before | After |
|---|---|---|
| Median frame time (ms) | 22.18 | [TODO] |
| p99 frame time (ms, `FrameTimeSampler`) | [TODO] | [TODO] |
| Worst frame (ms) | 938.3 | [TODO] |
| Worst-frame top marker | `FontEngine` / `File.Read` | [TODO] |
| GC alloc per frame in steady play (B) | [TODO] | [TODO] |

The fix succeeds if the worst frame no longer shows any `FontEngine.*` or `File.Read` markers. If a large spike remains, the next suspect is the IMGUI marker, checked in a release build. The median is a separate problem (see 2.3) and was not expected to change much.

**Side-effect checks:**
- [TODO] Text checked at all three `TextScale` sizes, and on the Victory and Defeat panels: no □ boxes.
- [TODO] APK size before/after. I expect almost no change, because the fallback asset and its font file sit in a `Resources` folder and Unity always includes those in the build. Moving the fallback out of `Resources` would remove it from the build.
- [TODO] `telemetry.log` pulled with adb after pressing Home: every event from the session is present.

### 2.3 What next for CA3 (based on evidence)

**Action.** Each item is tied to a measurement, not a preference:

1. **Fix the median, not just the spike.** 22.18 ms is above the 16.7 ms budget for 60 fps. Next, run `RenderScaleProbe` at 0.5×. If frame time drops sharply, the game is GPU-bound and the fix is render scale or overdraw. If it barely moves, look at CPU work, starting with `GridManager.ClearHighlights`. That function repaints all 48 tiles on every tap, and the cost can be confirmed with a profiler marker.
2. **Thermal tiers.** [TODO: start temperature and Δ after 10 minutes.] If the A15 throttles, use Adaptive Performance to drop to 30 fps during the enemy turn, which is mostly coroutine waits (`enemyThinkDelay` 0.35 s, `enemyActionDelay` 0.4 s) and doesn't need 60 fps (Unity Technologies, 2024c).
3. **Economy tuning from telemetry.** Add a `coins` parameter to `level_complete` and log `upgrade_purchased` with level and cost. Then pull `telemetry.log` from 5+ peer QA runs and check whether the 2:00 first-upgrade target is met before changing `coinsPerKill`.
4. **Pooling** only if the profiler shows a cost. Today, units are created once per battle and the scene reloads on Retry. If CA3 adds more encounters, a pool of `Unit` objects replaces per-encounter `Instantiate`. [TODO: record the Retry scene-reload time first.]

---

## AI assistance
This draft was structured with help from Claude (Anthropic, 2026). The draft was generated from my repository history, scripts and profiler screenshots. I checked every number against my own captures and replaced the placeholders with my own measurements.

## References
Anthropic (2026) *Claude* [Large language model]. Available at: https://claude.ai (Accessed: 10 November 2026).

Apple (2024) *Human Interface Guidelines: Accessibility*. Available at: https://developer.apple.com/design/human-interface-guidelines/accessibility (Accessed: [TODO]).

Gibbs, G. (1988) *Learning by Doing: A Guide to Teaching and Learning Methods*. Oxford: Further Education Unit, Oxford Polytechnic.

Unity Technologies (2024a) *Unity Manual: Profiler overview*. Available at: https://docs.unity3d.com/Manual/Profiler.html (Accessed: [TODO]).

Unity Technologies (2024b) *TextMesh Pro: Font Assets and Settings*. Available at: https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest (Accessed: [TODO]).

Unity Technologies (2024c) *Adaptive Performance package documentation*. Available at: https://docs.unity3d.com/Packages/com.unity.adaptiveperformance@latest (Accessed: [TODO]).
