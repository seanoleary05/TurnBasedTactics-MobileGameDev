# Reflective Development Journal: CA2 Vertical Slice

**Project:** Turn Based Tactics (Option 3, micro turn-based tactics)
**Device:** Samsung Galaxy A15 (SM-A155F), Android 14, Vulkan, 1080×2340, 90 Hz
**Engine:** Unity 6.6, IL2CPP, ARM64
**Release tag:** `v0.2-CA2`

---

## Part 1: Weekly log (W5 to W9)

### W5 (5 to 9 Oct)
- Refactored the board into a `Tile` component with its own coordinate and highlight state (`dbc4bba`), so the turn system can ask a tile what it shows instead of `GridManager` setting colours directly.

### W6 (12 to 16 Oct)
- Designed the economy loop in `dev-journal.md`: earn coins per kill, spend them on a wallet level, and each level adds +1 attack.

### W7 (19 to 23 Oct)
- Planned Adaptive Performance and a thermal soak but did neither. The soak capture slipped to 9 Nov, and Adaptive Performance was not integrated.

### Reading week (26 Oct to 1 Nov)
- Alternating light/dark tiles (`2e02dd5`), so grid distance reads at a glance on a small screen.

### W8 (2 to 6 Nov)
- Local telemetry stub writing to logcat and `telemetry.log` (`77a8d6e`).
- `Highlight` enum (`a3cf542`), unit rendering (`c13162c`) and the full `TurnManager` (`4edcef4`): select, move, attack, End Turn, and an enemy AI that approaches the nearest player and hits the weakest one.

### W9 (9 to 13 Nov)
- Health numbers over units (`7f9d6cb`), the thermal-soak profiler capture (`245c45f`), and the wallet, shop stub and HUD (`185af5a`).
- 11 Nov, showcase prep:
  - Restored the deleted `Wallet.Instance` singleton, which four scripts depend on (`eaf9e9a`).
  - Raised `coinsPerKill` from 10 to 40 (`3c99052`). At 10, the first upgrade (100 coins) took four wins. At 40, it comes after the first win.
  - Built the 33 MB `0.3.0` APK for the showcase, but forgot to bump the version code (still 2).
- 14 Nov: saved haptics toggle (`961cbf3`), and the font and telemetry fix (`f1b07d9`, see 2.2).
- Device-tier presets (the W9 milestone) were not implemented.

---

## Part 2: Structured reflection

The reflection follows Gibbs' (1988) cycle, shortened to **Description → Evaluation → Analysis → Action**.

### 2.1 What changed in scope or design, and why?

**Description.** The CA1 pitch was a linear dungeon of several encounters with a party of 3 to 4 units, loot, and per-character stats. The CA2 slice is one 6×8 battle (three units against three enemies) with coins as the only reward and one global upgrade (+1 ATK per level).

**Evaluation.** The cut suits a vertical slice, since the brief asks for a "production-ready feel for one level/encounter". The cost is that the economy has one sink, so reward → spend → upgrade only pays off across replays.

**Analysis.** Two pieces of evidence drove the cut. First, the commit history shows no commits from 8 Oct to 1 Nov, and most of the core loop landed on 2 to 5 Nov. With that little time left, more encounters would have meant a weaker loop. Second, the screen set the board size. `GridManager.CenterCamera` sets `orthographicSize = max(h/2 + 1, (w/2 + 0.5)/aspect)`. On the A15 (1080×2340, 450 dpi), a 6-wide board gives 7.58, so each tile is about 154 px, or 55 dp, above the 44 dp minimum touch target (Apple, 2024). The same formula gives 48 dp at 7 wide and 43 dp at 8 wide, which is below the minimum, so 6×8 was a constraint, not a free choice.

The upgrade size came from the same kind of check. With base ATK 2 against 5 HP enemies, the first upgrade turns a 3-hit kill into a 2-hit kill, which the player can see. The second (ATK 4) changes nothing visible, so the curve still needs tuning.

**Action.** For CA3, tune enemy HP so every upgrade changes hits-to-kill. Add a second encounter only if pacing data shows players reach their first upgrade within a run.

### 2.2 Key problem → approach → outcome

**Problem (Description).** The thermal-soak capture (`docs/CA2/baseline/thermal-soak/before.data`) shows a **median frame time of 22.18 ms** (about 45 fps against a 60 fps target) and a **worst frame of 938.3 ms** (frame 762). Its top self-time markers are `IMGUIModule GUIStyle…`, `EngineJob` (450.5 ms), `FontEngine.LoadFontFace_From_File` (61.9 ms), `File.Read` (58.8 ms) and `FontEngine.Get_GPOS_Lookups` (53.1 ms). In CA1 the worst frame was 20 ms, with Rendering as the tallest marker.

**Evaluation.** A near-one-second frame is a visible hitch, which is the "stable frame times" problem the guardrails warn about. It matters more than the median, because players notice one long stutter more than a slightly slow average (Unity Technologies, 2024a).

**Analysis.** The markers show **a font file being opened and its kerning (GPOS) tables read on the main thread during play**, not rendering cost. My first guess was a *dynamic* font atlas filling in new characters. The project disproved this. `LiberationSans SDF` is already a **static** atlas covering all printable ASCII, and every string in the game is ASCII, so no glyph was ever missing. The file reads came from two settings:
1. `GetFontFeaturesAtRuntime` was on in `TMP Settings`, so TextMesh Pro read kerning from the source font at runtime, although the static asset already stores those pairs.
2. The static font had a **dynamic fallback** that loads the raw `.ttf` from disk when used.

Either one explains a one-off stall the first time text is laid out. No script uses `OnGUI`, so the IMGUI marker is probably the development-build overlay, which needs checking in a release build. A smaller cause: `Telemetry.Log` called `File.AppendAllText` for every event, which is blocking I/O on the same frames as `level_start` and `level_complete`.

**Approach.** I made one change per cause:
1. **Runtime font features off** (`GetFontFeaturesAtRuntime` 1 → 0). Kerning still works from the stored pairs (Unity Technologies, 2024b).
2. **Dynamic fallback removed** from `LiberationSans SDF`. A character outside the atlas now shows □ instead of loading a font. That is acceptable while all text is ASCII, and it turns a hidden stutter into a visible bug.
3. **Buffered telemetry.** `Log()` still prints to logcat but buffers the file line. `Flush()` writes on `OnApplicationPause(true)`, on quit, or after 64 lines. I chose pause because Android usually kills a backgrounded app without calling `OnApplicationQuit`. The trade-off is that a hard crash loses unflushed lines, which is acceptable for a local debug log.

I rejected warming up text on a loading screen, because that only moves the stall rather than removing it.

**Outcome.**

| Metric | Before the fix (9 Nov capture) |
|---|---|
| Median frame time (ms) | 22.18 |
| Worst frame (ms) | 938.3 (frame 762) |
| Worst-frame top markers | `EngineJob`, `FontEngine.LoadFontFace_From_File`, `File.Read`, `FontEngine.Get_GPOS_Lookups` |
| APK size | 33 MB (v0.3.0) |

**The fix has not yet been measured.** I made it on 14 Nov, after the last device capture. My only evidence is that each removed setting accounts for one font marker, which is reasoning, not a measured result. Repeating the soak capture on the same device would confirm it. It succeeds if the worst frame contains no `FontEngine.*` or `File.Read` markers. If a spike remains, the IMGUI marker is next. The median is a separate problem (see 2.3).

**Side-effect risks** and their checks:
- *Missing glyphs:* expected none, since all text is ASCII. Check all three `TextScale` sizes and both result panels.
- *APK size:* expected near 33 MB, because the fallback still ships from its `Resources` folder. Moving it out would remove it.
- *Lost telemetry:* pull `telemetry.log` after pressing Home and confirm every event is present.

The scripts compile (`dotnet build`, no errors).

### 2.3 What next for CA3 (based on evidence)

**Action.** Each item is tied to a measurement:

1. **Fix the median, not just the spike.** 22.18 ms is above the 16.7 ms budget. Run `RenderScaleProbe` at 0.5×. If frame time drops sharply, the game is GPU-bound and the fix is render scale or overdraw. If it barely moves, profile CPU work, starting with `GridManager.ClearHighlights`, which repaints all 48 tiles on every tap.
2. **Thermal tiers.** Battery temperature started at 23.0 °C (`dumpsys battery`: 230), but I did not record the 10-minute change, so throttling is unproven. If a timed soak shows throttling, use Adaptive Performance to drop to 30 fps during the enemy turn, which is mostly coroutine waits (0.35 s and 0.4 s) (Unity Technologies, 2024c).
3. **Economy tuning from telemetry.** Add `coins` to `level_complete` and log `upgrade_purchased`. Then pull `telemetry.log` from 5+ peer QA runs and check the 2:00 first-upgrade target before changing `coinsPerKill`.
4. **Pooling only if the profiler shows a cost.** Units are created once per battle and Retry reloads the scene. Measure the reload time first, and pool `Unit` objects only if more encounters make `Instantiate` show up.

---

## AI assistance
This journal was structured and edited with help from Claude (Anthropic, 2026), working from my repository history, scripts and profiler captures. I checked each figure against my own captures. Where a measurement was not taken, the text says so rather than estimating it.

## References
Anthropic (2026) *Claude* [Large language model]. Available at: https://claude.ai (Accessed: 14 November 2026).

Apple (2024) *Human Interface Guidelines: Accessibility*. Available at: https://developer.apple.com/design/human-interface-guidelines/accessibility (Accessed: 14 November 2026).

Gibbs, G. (1988) *Learning by Doing: A Guide to Teaching and Learning Methods*. Oxford: Further Education Unit, Oxford Polytechnic.

Unity Technologies (2024a) *Unity Manual: Profiler overview*. Available at: https://docs.unity3d.com/Manual/Profiler.html (Accessed: 14 November 2026).

Unity Technologies (2024b) *TextMesh Pro: Font Assets and Settings*. Available at: https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest (Accessed: 14 November 2026).

Unity Technologies (2024c) *Adaptive Performance package documentation*. Available at: https://docs.unity3d.com/Packages/com.unity.adaptiveperformance@latest (Accessed: 14 November 2026).
