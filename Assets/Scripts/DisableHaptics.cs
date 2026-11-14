using UnityEngine;
using TMPro;


[RequireComponent(typeof(TMP_Text))]

public class DisableHaptics : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
public void OnClick()
    {
        Haptics.Enabled = !Haptics.Enabled;
        Debug.Log("Haptics.Enabled set to " + Haptics.Enabled);
    }
}
