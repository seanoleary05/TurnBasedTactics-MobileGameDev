using UnityEngine;
using TMPro;


[RequireComponent(typeof(TMP_Text))]

public class SetTextMed : MonoBehaviour
{
    public void OnClick()
    {
        TextScale.Factor = 0.75f;
        Debug.Log("TextScale.Factor set to 0.75f");
    }
}
