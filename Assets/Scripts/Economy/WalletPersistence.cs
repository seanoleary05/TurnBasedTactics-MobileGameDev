using UnityEngine;

// Sits on the App object next to Wallet. Loads before the HUD subscribes, saves on every
// change and on pause (OnApplicationQuit is not reliable on Android).
[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(Wallet))]
public class WalletPersistence : MonoBehaviour
{
    Wallet wallet;

    void Awake()
    {
        wallet = GetComponent<Wallet>();
        if (Wallet.Instance != wallet) return;   // duplicate from a scene reload; being destroyed

        WalletSave.Load(wallet);
        wallet.Changed += Save;
    }

    void OnDestroy() { if (wallet != null) wallet.Changed -= Save; }

    void OnApplicationPause(bool paused)
    {
        if (paused && Wallet.Instance == wallet) WalletSave.Save(wallet);
    }

    void Save() => WalletSave.Save(wallet);

    // Part E reset. Hook to a debug button, or use Edit > Clear All PlayerPrefs in the editor.
    public void ResetWallet()
    {
        PlayerPrefs.DeleteAll();
        WalletSave.Load(wallet);
    }
}
