# Performance Baseline Sheet (CA2)

Build: v0.3.0 (IL2CPP, ARM64), Unity 6000.6.0f1.
Captures (Unity Profiler on the device): **before** on 9 Nov 2026 (`docs/CA2/baseline/thermal-soak/before.data`); **after** on 15 Nov 2026, development build with the font fix, typical battle frame (`ProfilerCaptures/after.data`).
Cells marked **NOT MEASURED** were not captured for CA2.

## Device

| Field | Value |
|---|---|
| Device model | Samsung Galaxy A15 (SM-A155F) |
| SoC / GPU | MediaTek Helio G99 / Mali-G57 MC2 |
| RAM | ___ GB (Settings → About phone) |
| OS version | Android 14 (build A155FXXS6BYE2), Vulkan |
| Battery health | ___ (Settings → Battery → Battery information) |
| Display | 1080 × 2340, 450 dpi, 90 Hz |
| Target fps / render scale | 60 fps (`targetFrameRate = 60`) / URP render scale 1.0, MSAA off |

## Frame time and rendering

| Metric | Value |
|---|---|
| Frame time median | Before: **22.18 ms**. After: **22.1 ms** (about 45 fps) |
| Frame time max (worst frame) | Before: **938.3 ms** (frame 762; min 13.19 ms). After: **64 ms** |
| Frame time average / p99 | **22.19 ms / 22.36–22.45 ms** (`FrameTimeSampler`, 600-frame windows in logcat, 15 Nov) |
| CPU vs GPU time | CPU **21.97 ms** / GPU **not reported** (the Profiler shows `--` on this Mali/Vulkan device) |
| GC allocs per frame | **99 B** (typical battle frame, 15 Nov). CA1 capture (28 Sep): no GC.Collect. |
| Peak memory | **216 MB** Total Reserved |
| Rendering | **3** batches, **7** SetPass calls, **0** draw calls reported (SRP Batcher on), **222** triangles, **450** vertices |
| Reading | CPU time is almost the whole frame, and p99 is within 0.3 ms of the average. The frame time is steady rather than spiky. 22.2 ms is half of the 90 Hz panel's refresh rate (45 fps), so the likely cause is frame pacing to every second refresh, not CPU or GPU load. **Not yet confirmed** (test: set the display to 60 Hz and re-check the average). |
| CA1 comparison (28 Sep) | Worst main-thread frame 20 ms; tallest marker Rendering |

## Thermals

| Metric | Value |
|---|---|
| Starting temperature | 23.0 °C (`dumpsys battery`: `temperature: 230`) |
| Change after 10 minutes | 23.0 C |
| Throttling observed | None |

## Start-up and size

| Metric | Value |
|---|---|
| Cold start (process start → first frame) | About 2.2 s (CA1 logcat, 15 Sep: 20:42:48.684 → 20:42:50.848). Not re-measured for v0.3.0. |
| APK size | **33.1 MB** (v0.3.0). The first release build (v0.1.0, 28 Sep) was 58.8 MB. |

## Bottleneck and fix

**Bottleneck:** the worst frame (938 ms) was a one-off hitch caused by TextMesh Pro loading font data on the main thread during play: `FontEngine.LoadFontFace_From_File` 61.9 ms, `File.Read` 58.8 ms, `FontEngine.Get_GPOS_Lookups` 53.1 ms, plus `EngineJob` 450.5 ms.

**Fix (14 Nov, `f1b07d9`):** I turned off `GetFontFeaturesAtRuntime` in TMP Settings, removed the dynamic fallback from `LiberationSans SDF` (the static atlas already covers all the ASCII text in the game), and buffered telemetry file writes so `File.AppendAllText` no longer runs on gameplay frames.

**Before/after (Profile Analyzer compare, same device):**

| | Before (9 Nov) | After (15 Nov) | Change |
|---|---|---|---|
| Worst frame | 938.3 ms | **64 ms** | **−93%** |
| Median frame | 22.18 ms | 22.1 ms | unchanged |

The one-second hitch is gone. The median did not move, which fits the fix: it removed a one-off load, not per-frame work. The remaining 64 ms spike is the next thing to investigate. **Side effects to check:** missing glyphs (□) at every text size, APK size, and that `telemetry.log` still has every event after pressing Home.
