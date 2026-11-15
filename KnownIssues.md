# Known Issues (v0.2-CA2)

## Gameplay and UX
- **One encounter only.** The slice is a single 6×8 battle. Retry reloads the same battle.
- **Upgrade curve is uneven.** The first upgrade (ATK 2 → 3) turns a 3-hit kill into a 2-hit kill. The second (ATK 4) changes nothing visible against 5 HP enemies.
- **No tutorial or onboarding.** The controls are only explained in the README.
- **No audio.**
- **No Quit or Main Menu option in game.** The pause panel only has Resume. To quit, use the Android recent-apps screen.
- **Back on the main menu does nothing.** It should quit or ask to quit.
- **A button in the battle scene still shows the default "Button" label.**
- **The stub shop's "buy pack" grants 500 coins for free.** `FakePurchaseProvider` always succeeds; there is no real billing.

## Performance
- **Median frame time is about 22 ms (about 45 fps) against a 60 fps target.** It is very steady (p99 about 22.4 ms), so it is probably frame pacing on the 90 Hz screen rather than load. Not yet confirmed or fixed.
- **A 64 ms worst-frame spike remains.** The font fix cut the worst frame from 938 ms to 64 ms; the remaining spike has not been investigated.
- **Adaptive Performance and device-tier presets are not integrated.**
- **`telemetry.log` can lose its last lines after a hard crash**, because lines are buffered until the app pauses, quits, or 64 lines build up.

## Build and repo
- `versionCode` must be raised for each new release build so it installs over the old one.
- IL2CPP backup folders (`*_BackUpThisFolder_ButDontShipItWithYourGame`) were committed in the past and bloat the repo.
- `Assets/Scripts/SceneLoader.cs` is an unused empty template.
