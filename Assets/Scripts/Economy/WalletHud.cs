using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Coin label + Upgrade button. Refreshes only when the wallet changes, never in Update.
public class WalletHud : MonoBehaviour
{
    [SerializeField] TMP_Text coinsLabel;
    [SerializeField] TMP_Text upgradeLabel;
    [SerializeField] Button upgradeButton;

    Wallet wallet;

    void OnEnable()
    {
        // Always use the surviving singleton: after a scene reload the scene's own
        // App object is a duplicate that is about to be destroyed.
        wallet = Wallet.Instance;
        if (wallet == null) { Debug.LogWarning("[economy] No Wallet in scene"); return; }
        wallet.Changed += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        if (wallet != null) wallet.Changed -= Refresh;
    }

    void Refresh()
    {
        coinsLabel.SetText("{0}", wallet.Coins);            // no string concat
        upgradeLabel.SetText("Upgrade ({0})", wallet.NextCost);
        upgradeButton.interactable = wallet.Coins >= wallet.NextCost;
    }

    // Wire to the Upgrade button's OnClick.
    public void OnUpgradePressed()
    {
        if (wallet == null) return;
        int cost = wallet.NextCost;
        if (!wallet.TryUpgrade()) return;                   // Refresh should already disable it
        Telemetry.Log("upgrade_bought", ("level", wallet.Level), ("cost", cost),
            ("coins_left", wallet.Coins), ("t", Time.realtimeSinceStartup.ToString("F1")));
    }
}
