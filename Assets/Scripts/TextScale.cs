using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TextScale : MonoBehaviour
{
    const string Key = "textScale";
    public static event System.Action Changed;

    public static float Factor
    {
        get => PlayerPrefs.GetFloat(Key, 1f);
        set { PlayerPrefs.SetFloat(Key, value); PlayerPrefs.Save(); Changed?.Invoke(); }
    }

    TMP_Text _text;
    float _baseSize;

    // public void DropdownSample(int index)
    // {
    //     switch (index)
    //     {
    //         case 0: Factor = 0.5f; break;
    //         case 1: Factor = 0.75f; break;
    //         case 2: Factor = 1f; break;
    //     }
    // }

    void Awake() { _text = GetComponent<TMP_Text>(); _baseSize = _text.fontSize; }
    void OnEnable() { Changed += Apply; Apply(); }
    void OnDisable() { Changed -= Apply; }
    void Apply() { _text.fontSize = _baseSize * Factor; }
}