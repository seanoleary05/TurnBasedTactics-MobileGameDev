# Privacy Statement

## Data Collected

This game does **not collect, transmit, sell, share, or process any personal data** from players.

No user accounts are required, no analytics services are used, and no personally identifiable information (PII) is collected. The game is designed to function entirely offline.

---

## Data Stored on the Device

The game stores gameplay data locally on the player's device to support game functionality.

### PlayerPrefs
The following PlayerPrefs keys may be used:

- `MasterVolume`
- `MusicVolume`
- `SFXVolume`
- `TutorialCompleted`
- `wallet.coins` (int): soft-currency balance earned in play
- `wallet.level` (int): number of upgrades bought

These values store player settings, preferences and in-game progress only. The coin
balance is a soft currency earned by playing; no real-money purchases are processed
(the in-game "Buy 500 coins" button is a local stub and does not contact any store).

### Save Files

Game progress is stored locally using files saved within:

```text
Application.persistentDataPath
