# CA3 Checklist: Performance & Quality Dossier + Final Build

**Due: Sun 6 Dec 2026, 23:59**

This list comes from the W03, W07, W08, W09 and W10 lab sheets. There's no official CA3 brief in the folder yet. When it's released, check the submission format (ZIP layout, video length, rubric criteria) against it.

Status on 16 Nov 2026: `docs/CA3/` and `docs/store/` don't exist yet. Every item below is still open unless it's marked ✅.

---

## Key dates

| Date | What |
|---|---|
| Tue 17 Nov, 10:00 | Async Pack 2 (Store Presence Draft) due on Moodle |
| Sun 29 Nov | Async Pack 3 (Peer APK survey) closes |
| **Sun 6 Dec, 23:59** | **CA3 due** |

---

## 1. Final build

- [ ] Release-signed APK in `releases/` with a new version number. The project is at `bundleVersion 0.3.0`, `versionCode 2`, and `0.4.0.apk` is already in `releases/`, so go higher than both.
- [ ] Increase `versionCode` for every build you share, or testers can't install over the old one.
- [ ] `Assets/Scripts/UI/BuildStamp.cs` shows version, build ID and device on the About/Info screen (W08-LabA). There's no `BuildStamp.cs` yet. `Info.cs` may already cover part of it.
- [ ] Telemetry `session_start` includes `version`, `build` and `device` (W08-LabA). `MobileBootstrap.cs` logs `session_start`, but check which fields it sends.
- [ ] `ThermalOverlay` stays in the build until CA3, because markers need it at the showcase (W07-LabB). There's no `ThermalOverlay.cs` in `Assets/Scripts/` at the moment. Check whether it lives somewhere else.
- [ ] Development-only screenshot tools (`StoreShot.cs`, `DebugShotButton.cs`) are removed from the final build (W10-LabA says they come out in Week 12).
- [ ] Git tag on the final commit, named in `LINKS.txt`. CA2 used `v0.2-CA2`; the CA3 tag name isn't stated yet.
- [ ] `README.md`, `KnownIssues.md` and `LINKS.txt` updated for the CA3 build. They still say `v0.2-CA2` / 0.3.0.

## 2. Performance dossier: targets and before/after evidence

- [ ] `docs/CA3/targets.md` has three final-sprint targets with one number each. These become the dossier headings (W09-LabB, Part D):
  - [ ] **Thermals**: Balanced mode holds ≤ __ ms at p99 at minute 10 of a soak on the Galaxy A15.
  - [ ] **Resilience**: all 6 test-plan scenarios pass on your phone and on a peer's phone. Crash-free sessions __ of __ (from `session_start`/`session_end`) by 29 Nov.
  - [ ] **Store assets**: icon, feature graphic, screenshots, descriptions and privacy statement.
- [ ] A **before-and-after table** for every fix you claim. Each "before" must point to a bottleneck note or capture (W03-LabA/B). Existing "before" evidence:
  - ✅ `docs/CA2/baseline/thermal-soak/before.data` and `after.data`
  - ✅ `docs/CA2/Performance_Baseline.md`, `docs/CA2/baseline/baseline-sheet.md`
  - [ ] There's no `bottleneck-01.md`. W03-LabA asks for one; write it now if you'll claim a fix from it.
- [ ] Follow up the "What next for CA3" items in `docs/CA2/Reflective_Journal.md` §2.3, with numbers:
  - [ ] Median of 22.18 ms against the 16.7 ms budget: test at 60 Hz, then use `RenderScaleProbe` at 0.5× to find out whether you're CPU-bound or GPU-bound.
  - [ ] Thermal tiers: a timed 10-minute soak with the battery temperature recorded at the start and at the end. Use Adaptive Performance if it throttles.
  - [ ] Economy tuning from telemetry: add `coins` to `level_complete`, log `upgrade_purchased`, and check the 2:00 first-upgrade target using 5+ peer runs.
  - [ ] Pooling, only if the profiler shows a cost.
  - [ ] Tune enemy HP so each upgrade changes hits-to-kill (§2.1 Action).
- [ ] Look into the remaining 64 ms worst-frame spike (`KnownIssues.md`).

## 3. QA evidence pack (`docs/CA3/qa/`)

From W08-LabA, distribution:
- [ ] `docs/CA3/qa/qr-<version>.png`: a QR code for the APK share link. The lab sheet uses `qr-0.8.0.png`; use your real version.
- [ ] `docs/CA3/qa/builds.md` lists version, build ID, SHA-256 and share link for **every** build you shared.
- [ ] `docs/CA3/qa/device-matrix.md` has **at least 3 rows** (phone, SoC, RAM, Android, panel Hz, tier, owner, builds tested), ideally including a low-tier phone.

From W08-LabB, test plan:
- [ ] `docs/CA3/qa/test-plan.md` with all **6 scenarios** filled in (pass/fail plus one piece of evidence each):
  - [ ] 1. Resume/background: 10 Home cycles with no soft lock, plus a force-stop and relaunch that keeps the save
  - [ ] 2. No network: airplane mode before launch and during play
  - [ ] 3. Rotation: the portrait lock holds and safe areas are respected
  - [ ] 4. Battery saver: 2 minutes of play
  - [ ] 5. Low RAM: `send-trim-memory`, background the app, return
  - [ ] 6. Cold and warm start: `TotalTime` values
- [ ] `docs/CA3/qa/logcat-w08-testplan.txt`: the logcat for the full run
- [ ] `docs/CA3/qa/screens/`: screenshots for the scenarios that need one
- [ ] The worst failure is fixed, or filed in `KnownIssues.md` with a repro, a logcat line, the build ID and the device
- [ ] `docs/CA2/baseline/Performance_Baseline.md` has PSS, cold-start and warm-start rows

## 4. Async Pack 3: Peer survey (closes Sun 29 Nov)

- [ ] `docs/CA3/qa/survey/form-link.md` has the form link and the list of fields
- [ ] At least **10 testers**, not only family
- [ ] Re-share a new QR code every time you ship a build, and keep the build ID column in the responses
- [ ] Responses exported to **CSV** in `docs/CA3/qa/survey/`
- [ ] `docs/CA3/qa/survey/insights.md` has **5–8 insights**, each with an immediate fix

## 5. Store presence (awareness only: nothing is uploaded)

Async Pack 2 (W09-LabB), due **Tue 17 Nov, 10:00**:
- [ ] `docs/CA3/store/pack2-store-presence.pdf`: a 5-minute mini-deck containing:
  - [ ] Icon moodboard (6–9 references)
  - [ ] Screenshot plan (4–6 shots, each annotated)
  - [ ] Short description (80 characters) and long description (up to 4,000 characters)
  - [ ] Competitor scan (2–3 titles)
- [ ] `docs/CA3/store/README.md` with a checklist of those four parts

Final assets (W10-LabA, in `docs/store/`):
- [ ] `icon-512.png`: 512×512, 32-bit PNG, under 1024 KB, readable at 48 px, no text, no rounded corners
- [ ] `feature-1024x500.png` (or `.jpg`): no alpha, focal point in the centre, no device frame
- [ ] `screenshots/`: 4–6 PNGs at 9:16 (1080×1920). The A15's 1080×2340 is too tall, so crop it. No HUD and no "Development Build" watermark. At least 3 in portrait.
- [ ] `asset-checklist.md`: file / spec / actual / tick for each asset, plus the line "Specs checked against the Play *Add preview assets* help page on 16 Nov 2026"
- [ ] `descriptions.md`: short description A, short description B (each ≤ 80 characters) and the long description. Only list features that exist in the build.
- [ ] A privacy statement that matches what data the game actually uses (no network, local `telemetry.log` and PlayerPrefs). `docs/CA1/privacy-statement.md` is a starting point.

## 6. Presentation video and portfolio (W10-LabA)

- [ ] Presentation video slides
- [ ] Portfolio
- [ ] The lab sheets don't give the details (length, format). Check the CA3 brief or the Week 11/12 sheets.

## 7. Journal and reflection

- [ ] An entry headed *Showcase feedback, Wed 11 Nov* with the three most repeated fixes, how many people raised each, and a decision for each: *before Sunday*, *for CA3* or *decline with reason* (W09-LabB). This isn't in the repo yet.
- [ ] `docs/CA2/showcase-feedback.csv`, once the lecturer shares it
- [ ] An AI-assistance statement, as in CA2

## 8. Repo housekeeping (from `KnownIssues.md`)

- [ ] Remove the `*_BackUpThisFolder_ButDontShipItWithYourGame` folders from Git
- [ ] Delete the unused `Assets/Scripts/SceneLoader.cs`
- [ ] Keep the `mono_crash.*.blob` files (10 MB each) out of the repo
- [ ] Push the local commit (`main` is 1 ahead of `origin`)
