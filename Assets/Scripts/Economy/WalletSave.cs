using UnityEngine;

public static class WalletSave
{
    const string CoinsKey = "wallet.coins", LevelKey = "wallet.level";

    public static void Save(Wallet w)
    {
        PlayerPrefs.SetInt(CoinsKey, w.Coins);
        PlayerPrefs.SetInt(LevelKey, w.Level);
        PlayerPrefs.Save();                     // flush to disk now
    }

    public static void Load(Wallet w) =>
        w.Restore(PlayerPrefs.GetInt(CoinsKey, 0), PlayerPrefs.GetInt(LevelKey, 0));
}
