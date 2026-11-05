
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private int width = 6;
    [SerializeField] private int height = 8;
    [SerializeField] private Color lightColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color darkColor = new Color(0.2f, 0.2f, 0.2f);

    public GameObject tilePrefab;
    public Transform gridParent;
    [SerializeField] private Transform _cam;

    private readonly Dictionary<Vector2Int, Tile> _tiles = new Dictionary<Vector2Int, Tile>();
    private readonly Dictionary<Vector2Int, Unit> _occupants = new Dictionary<Vector2Int, Unit>();

    // Reused by the search below so it creates no garbage after the first call.
    private readonly Queue<Vector2Int> _frontier = new Queue<Vector2Int>();
    private readonly Dictionary<Vector2Int, int> _steps = new Dictionary<Vector2Int, int>();

    private static readonly Vector2Int[] Dirs =
        { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

    public int Width => width;
    public int Height => height;

    void Awake()
    {
        Instance = this;
        GenerateGrid();
        CenterCamera();
    }

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

        // Step 1: fit the whole grid on a portrait screen, with room for UI above and below.
        var cam = _cam.GetComponent<Camera>();
        cam.orthographicSize = Mathf.Max(height * 0.5f + 1f, (width * 0.5f + 0.5f) / cam.aspect);
    }

    // ---------- Tiles ----------

    public Tile GetTile(Vector2Int coord) =>
        _tiles.TryGetValue(coord, out var tile) ? tile : null;

    public void ClearHighlights()
    {
        foreach (var tile in _tiles.Values)
            tile.SetHighlight(Highlight.None);
    }

    public bool InBounds(Vector2Int c) =>
        c.x >= 0 && c.y >= 0 && c.x < width && c.y < height;

    // ---------- Coordinates ----------

    public Vector3 GridToWorld(Vector2Int c) =>
        gridParent.TransformPoint(new Vector3(c.x, c.y, 0f));

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

    // Grid distance: steps up/down/left/right, no diagonals. 1 means adjacent.
    public static int Distance(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    // ---------- Occupancy ----------

    public Unit GetOccupant(Vector2Int c) =>
        _occupants.TryGetValue(c, out var u) ? u : null;

    public void SetOccupant(Vector2Int c, Unit u) => _occupants[c] = u;

    public void ClearOccupant(Vector2Int c, Unit u)
    {
        if (_occupants.TryGetValue(c, out var cur) && cur == u)
            _occupants.Remove(c);
    }

    // ---------- Queries for the turn system and AI ----------

    // Fills `results` with every empty tile the unit can walk to within its Move range.
    // Other units (friend or foe) block the path. The unit's own tile is not included.
    public void GetReachable(Unit unit, List<Vector2Int> results)
    {
        results.Clear();
        _frontier.Clear();
        _steps.Clear();

        _frontier.Enqueue(unit.Coord);
        _steps[unit.Coord] = 0;

        while (_frontier.Count > 0)
        {
            Vector2Int cur = _frontier.Dequeue();
            int s = _steps[cur];
            if (s == unit.Move) continue;            // out of movement

            foreach (var d in Dirs)
            {
                Vector2Int next = cur + d;
                if (!InBounds(next) || _steps.ContainsKey(next)) continue;
                if (GetOccupant(next) != null) continue; // blocked

                _steps[next] = s + 1;
                _frontier.Enqueue(next);
                results.Add(next);
            }
        }
    }

    // Fills `results` with living enemy units directly next to this unit.
    public void GetAttackTargets(Unit unit, List<Unit> results)
    {
        results.Clear();
        foreach (var d in Dirs)
        {
            Unit other = GetOccupant(unit.Coord + d);
            if (other != null && other.IsAlive && other.Team != unit.Team)
                results.Add(other);
        }
    }
}
