using UnityEngine;

public enum Highlight { None, Selected, Move, Attack }

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f);   // yellow
    [SerializeField] private Color moveColor     = new Color(0.3f, 0.6f, 1f);    // blue
    [SerializeField] private Color attackColor   = new Color(1f, 0.3f, 0.3f);    // red

    private SpriteRenderer _sr;
    private Color _baseColor;

    public Vector2Int Coord { get; private set; }
    public Highlight Current { get; private set; }

    void Awake() => _sr = GetComponent<SpriteRenderer>();

    public void Init(Vector2Int coord, Color baseColor)
    {
        Coord = coord;
        _baseColor = baseColor;
        SetHighlight(Highlight.None);
    }

    public void SetHighlight(Highlight h)
    {
        Current = h;
        _sr.color = h switch
        {
            Highlight.Selected => selectedColor,
            Highlight.Move     => moveColor,
            Highlight.Attack   => attackColor,
            _                  => _baseColor,
        };
    }
}
