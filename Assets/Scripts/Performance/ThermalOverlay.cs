using UnityEngine;
using UnityEngine.AdaptivePerformance;
using TMPro;

 

public class ThermalOverlay : MonoBehaviour

{

    public TMP_Text label;

    float ms;

    void Update()

    {

        float now = Time.unscaledDeltaTime * 1000f;

        ms = ms > 0f ? Mathf.Lerp(ms, now, 0.1f) : now;

        var ap = Holder.Instance;

        if (ap == null || !ap.Active)

        { label.text = $"{ms:0.0} ms  (AP inactive)"; return; }

        ThermalMetrics t = ap.ThermalStatus.ThermalMetrics;

        label.text = $"{ms:0.0} ms  {t.WarningLevel}\n"

            + $"temp {t.TemperatureLevel:0.00}  "

            + $"trend {t.TemperatureTrend:0.00}";

    }

}