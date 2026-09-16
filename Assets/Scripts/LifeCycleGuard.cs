using UnityEngine;

public class LifecycleGuard : MonoBehaviour
{
    public static bool IsPaused { get; private set; }
    public static event System.Action<bool> PausedChanged;

    // Home, app switch, incoming call, screen off: save HERE.
    void OnApplicationPause(bool paused)
    {
        if (!paused) return;           // resuming is a player choice
        PlayerPrefs.Save();            // swap for your save system
        SetPaused(true);
    }

    // Notification shade, permission dialog, on-screen keyboard.
    void OnApplicationFocus(bool f) { if (!f) SetPaused(true); }

    public void SetPaused(bool value)  // Resume button passes false
    {
        IsPaused = value;
        Time.timeScale = value ? 0f : 1f;
        AudioListener.pause = value;
        PausedChanged?.Invoke(value);
    }
}