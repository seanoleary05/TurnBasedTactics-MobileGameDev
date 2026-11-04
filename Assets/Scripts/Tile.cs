using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f);

    private SpriteRenderer _sr;
    private Color _baseColor;

    public Vector2Int Coord { get; private set; }
    public bool IsSelected { get; private set; }

    void Awake() => _sr = GetComponent<SpriteRenderer>();

    public void Init(Vector2Int coord, Color baseColor)
    {
        Coord = coord;
        _baseColor = baseColor;
        _sr.color = baseColor;
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        _sr.color = selected ? selectedColor : _baseColor;
    }
}
