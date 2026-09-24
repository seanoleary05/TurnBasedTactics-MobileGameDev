using UnityEngine;
using TMPro;


[RequireComponent(typeof(TMP_Text))]

public class SetTextSmall : MonoBehaviour
{
    public void OnClick()
    {
        TextScale.Factor = 0.5f;
        Debug.Log("TextScale.Factor set to 0.5f");
    }

}
