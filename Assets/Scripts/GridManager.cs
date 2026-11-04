using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private int width = 3;
    [SerializeField] private int height = 4;
    [SerializeField] private Color lightColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color darkColor = new Color(0.2f, 0.2f, 0.2f);

    public GameObject tilePrefab;
    public Transform gridParent;
    [SerializeField] private Transform _cam;

    private readonly Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();
    private Tile _selected;

    public int Width => width;
    public int Height => height;
    public Tile Selected => _selected;

    void Awake()
    {
        Instance = this;
        GenerateGrid();
        CenterCamera();
    }

    void OnEnable()  => TapSwipeInput.Tapped += OnTapped;
    void OnDisable() => TapSwipeInput.Tapped -= OnTapped;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject go = Instantiate(tilePrefab, gridParent);
                go.transform.localPosition = new Vector3(x, y, 0f);
                go.name = $"Tile_{x}_{y}";

                var sr = go.GetComponent<SpriteRenderer>();
                sr.sortingLayerName = "UI Layer1";
                sr.sortingOrder = 1;

                var tile = go.GetComponent<Tile>();
                var coord = new Vector2Int(x, y);
                tile.Init(coord, (x + y) % 2 == 0 ? lightColor : darkColor);
                _tiles[coord] = tile;
            }
        }
    }

    void CenterCamera()
    {
        Vector3 center = gridParent.TransformPoint(new Vector3((width - 1) * 0.5f, (height - 1) * 0.5f, 0f));
        _cam.position = new Vector3(center.x, center.y, _cam.position.z);
    }

    public Tile GetTile(Vector2Int coord) =>
        _tiles.TryGetValue(coord, out var tile) ? tile : null;

    public Vector2Int WorldToGrid(Vector3 world)
    {
        Vector3 local = gridParent.InverseTransformPoint(world);
        return new Vector2Int(Mathf.RoundToInt(local.x), Mathf.RoundToInt(local.y));
    }

    public Tile TileAtScreen(Vector2 screenPos)
    {
        var cam = Camera.main;
        if (cam == null) return null;
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
        return GetTile(WorldToGrid(world));
    }

    void OnTapped(Vector2 screenPos)
    {
        if (LifecycleGuard.IsPaused) return;

        Tile tapped = TileAtScreen(screenPos);
        if (tapped == _selected) tapped = null;   // tapping the selected tile deselects it

        if (_selected != null) _selected.SetSelected(false);
        _selected = tapped;
        if (_selected != null) _selected.SetSelected(true);
    }
}
