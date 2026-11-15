using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Main menu Info button: opens a panel listing this device's hardware and the build.
// The text is built when the panel opens (not every frame), so it costs nothing in play.
public class Info : MonoBehaviour
{
    [SerializeField] GameObject panel;   // full-screen overlay, hidden until Info is tapped
    [SerializeField] TMP_Text body;

    readonly StringBuilder sb = new StringBuilder(512);

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
    }

    // Wired to the Info button.
    public void Show()
    {
        body.text = BuildReport();
        panel.SetActive(true);
        Telemetry.Log("info_opened", ("device_model", SystemInfo.deviceModel));
    }

    // Wired to the Close button.
    public void Hide() => panel.SetActive(false);

    void Update()
    {
        // Android back arrives as Escape in the Input System: close the panel first.
        if (panel == null || !panel.activeSelf) return;
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame) Hide();
    }

    string BuildReport()
    {
        sb.Clear();

        sb.AppendLine("<b>Device</b>");
        sb.AppendLine($"Model: {SystemInfo.deviceModel}");
        sb.AppendLine($"OS: {SystemInfo.operatingSystem}");
        sb.AppendLine($"RAM: {SystemInfo.systemMemorySize} MB");
        sb.AppendLine($"Battery: {Battery()}");
        sb.AppendLine();

        sb.AppendLine("<b>CPU / GPU</b>");
        sb.AppendLine($"CPU: {SystemInfo.processorType}");
        sb.AppendLine($"Cores: {SystemInfo.processorCount} @ {SystemInfo.processorFrequency} MHz");
        sb.AppendLine($"GPU: {SystemInfo.graphicsDeviceName}");
        sb.AppendLine($"API: {SystemInfo.graphicsDeviceType}, VRAM {SystemInfo.graphicsMemorySize} MB");
        sb.AppendLine();

        sb.AppendLine("<b>Display</b>");
        sb.AppendLine($"Resolution: {Screen.width} x {Screen.height} @ {Screen.dpi:F0} dpi");
        sb.AppendLine($"Refresh: {Screen.currentResolution.refreshRateRatio.value:F0} Hz, " +
                      $"target {Application.targetFrameRate} fps");
        Rect s = Screen.safeArea;
        sb.AppendLine($"Safe area: {s.width:F0} x {s.height:F0} at ({s.x:F0}, {s.y:F0})");
        sb.AppendLine();

        sb.AppendLine("<b>Build</b>");
        sb.AppendLine($"{Application.productName} {Application.version}");
        sb.Append($"Unity {Application.unityVersion}");

        return sb.ToString();
    }

    static string Battery()
    {
        float level = SystemInfo.batteryLevel;   // -1 when the platform can't tell
        return level < 0f
            ? "unknown"
            : $"{level * 100f:F0}% ({SystemInfo.batteryStatus})";
    }
}
