using UnityEngine;

public class MobileBootstrap : MonoBehaviour
{
    void Awake()
    {
        // Android defaults to 30 fps when targetFrameRate is -1.
        Application.targetFrameRate = 60;
        // vSyncCount is ignored on Android; keep it at 0.
        QualitySettings.vSyncCount = 0;
        // Keep the screen on while the game is in the foreground.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Debug.Log($"[Boot] {SystemInfo.deviceModel} | " +
                  $"{SystemInfo.operatingSystem} | " +
                  $"{SystemInfo.graphicsDeviceType} | " +
                  $"{Screen.width}x{Screen.height} @ {Screen.dpi} dpi");
    }
}