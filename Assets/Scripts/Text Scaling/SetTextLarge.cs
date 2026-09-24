using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class SetTextLarge : MonoBehaviour
{
    public void OnClick()
    {
        TextScale.Factor = 1f;
        Debug.Log("TextScale.Factor set to 1f");
    }
}
