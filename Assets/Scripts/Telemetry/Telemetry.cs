using System.IO;
using System.Text;
using UnityEngine;

// Event log: logcat immediately, telemetry.log on disk in batches.
// Lines are buffered in memory and written on pause/quit (or when the buffer
// gets large), so gameplay frames never block on file I/O.
public static class Telemetry
{
    const int FlushThreshold = 64;   // lines; a normal session never reaches this

    static readonly string session =
        System.Guid.NewGuid().ToString("N").Substring(0, 8);
    static readonly string path =
        Path.Combine(Application.persistentDataPath, "telemetry.log");

    static readonly StringBuilder buffer = new StringBuilder(4096);
    static readonly StringBuilder line = new StringBuilder(128);
    static int pending;

    public static void Log(string ev, params (string k, object v)[] p)
    {
        line.Clear();
        line.Append(Time.realtimeSinceStartup.ToString("F2"))
            .Append(' ').Append(session).Append(' ').Append(ev);
        foreach (var (k, v) in p)
            line.Append(' ').Append(k).Append('=').Append(v);

        Debug.Log("[telemetry] " + line);       // logcat, today
        buffer.Append(line).Append('\n');       // disk, on next flush
        if (++pending >= FlushThreshold) Flush();
    }

    // Writes buffered lines to telemetry.log. Called on pause/quit.
    public static void Flush()
    {
        if (pending == 0) return;
        try
        {
            File.AppendAllText(path, buffer.ToString());  // survives relaunch
            buffer.Clear();
            pending = 0;
        }
        catch (IOException e)
        {
            Debug.LogWarning("[telemetry] flush failed: " + e.Message);  // keep buffer, retry next flush
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        var go = new GameObject("[Telemetry]") { hideFlags = HideFlags.HideAndDontSave };
        Object.DontDestroyOnLoad(go);
        go.AddComponent<TelemetryFlusher>();
    }

    // Android usually kills a backgrounded app without OnApplicationQuit,
    // so pause is the reliable save point.
    sealed class TelemetryFlusher : MonoBehaviour
    {
        void OnApplicationPause(bool paused) { if (paused) Flush(); }
        void OnApplicationQuit() => Flush();
    }
}
