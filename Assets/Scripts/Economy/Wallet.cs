using UnityEngine;

// Soft-currency wallet. Lives on the persistent App object (DontDestroyOnLoad) so it
// survives the Retry scene reload. Runs early so WalletPersistence/HUD can find Instance.
[DefaultExecutionOrder(-100)]
public class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    public int Coins { get; private set; }
    public int Level { get; private set; }
    public event System.Action Changed;
    public int NextCost => (int)(100 * Mathf.Pow(1.5f, Level));

    void Awake()
    {
        // SampleScene is reloaded on Retry, which brings a second App object with it.
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    public void Earn(int n) { Coins += n; Changed?.Invoke(); }
    public void Restore(int c, int l)
    { Coins = c; Level = l; Changed?.Invoke(); }

    public bool TryUpgrade()
    {
        if (Coins < NextCost) return false;
        Coins -= NextCost; Level++; Changed?.Invoke(); return true;
    }

    
    
}
