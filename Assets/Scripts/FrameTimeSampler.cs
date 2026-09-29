using System;
using UnityEngine;

// Logs average and 99th percentile frame time every 600 frames.
public class FrameTimeSampler : MonoBehaviour
{
    const int N = 600;                 // 10 s at 60 fps
    readonly float[] _ms = new float[N];
    readonly float[] _sorted = new float[N];
    int _i;

    void Update()
    {
        _ms[_i++] = Time.unscaledDeltaTime * 1000f;
        if (_i < N) return;
        _i = 0;
        Array.Copy(_ms, _sorted, N);
        Array.Sort(_sorted);           // reused buffer, no garbage
        float sum = 0f;
        for (int k = 0; k < N; k++) sum += _sorted[k];
        float p99 = _sorted[Mathf.FloorToInt(0.99f * (N - 1))];
        Debug.Log($"[Baseline] avg {sum / N:F2} ms  p99 {p99:F2} ms");
    }
}