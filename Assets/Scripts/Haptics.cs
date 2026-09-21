using UnityEngine;

public static class Haptics
{
    const string Key = "haptics";

    public static bool Enabled
    {
        get => PlayerPrefs.GetInt(Key, 1) == 1;
        set { PlayerPrefs.SetInt(Key, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static void Pulse()
    {
        if (!Enabled) return;
        Handheld.Vibrate();   // one fixed pulse; no duration or intensity control
    }
}