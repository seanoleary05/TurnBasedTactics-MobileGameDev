using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Runs one battle: spawns both squads, turns taps into select/move/attack, and swaps turns.
public class TurnManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Vector2Int[] playerSpawns =
        { new Vector2Int(1, 0), new Vector2Int(2, 1), new Vector2Int(4, 0) };
    [SerializeField] private Vector2Int[] enemySpawns =
        { new Vector2Int(1, 7), new Vector2Int(3, 6), new Vector2Int(4, 7) };

    [Header("UI")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private TMP_Text banner;

    private GridManager grid;
    private readonly List<Unit> players = new List<Unit>();
    private readonly List<Unit> enemies = new List<Unit>();

    private bool playerTurn;   // false during the enemy turn (and later, after win/lose)
    private Unit selected;     // the player unit you tapped, or null
    private int turn;

    // Reused lists so tapping doesn't create garbage.
    private readonly List<Vector2Int> reach = new List<Vector2Int>();
    private readonly List<Unit> targets = new List<Unit>();

    void OnEnable()  => TapSwipeInput.Tapped += OnTapped;
    void OnDisable() => TapSwipeInput.Tapped -= OnTapped;

    void Start()   // Start, not Awake: GridManager builds the board in its Awake
    {
        grid = GridManager.Instance;

        foreach (var c in playerSpawns) players.Add(Spawn(Team.Player, c));
        foreach (var c in enemySpawns)  enemies.Add(Spawn(Team.Enemy, c));

        Telemetry.Log("level_start", ("level", "battle_01"),
            ("players", players.Count), ("enemies", enemies.Count));

        StartPlayerTurn();
    }

    Unit Spawn(Team team, Vector2Int c)
    {
        Unit u = Instantiate(unitPrefab);
        u.Init(team, c);
        return u;
    }

    // ---------- Player turn ----------

    void StartPlayerTurn()
    {
        turn++;
        foreach (var u in players) if (u.IsAlive) u.ResetTurn();
        playerTurn = true;
        endTurnButton.interactable = true;
        banner.text = $"Turn {turn}: your move";
    }

    void OnTapped(Vector2 screenPos)
    {
        if (!playerTurn || LifecycleGuard.IsPaused || IsOverUI(screenPos)) return;

        Tile tile = grid.TileAtScreen(screenPos);
        if (tile == null) { Deselect(); return; }
        Unit unitThere = grid.GetOccupant(tile.Coord);

        // Blue tile: move there.
        if (tile.Current == Highlight.Move)
        {
            selected.Place(tile.Coord);
            selected.HasMoved = true;
            ShowOptions(selected);
            if (targets.Count == 0) Deselect();   // nothing to hit, so this unit is done
            return;
        }

        // Red tile: attack that enemy.
        if (tile.Current == Highlight.Attack)
        {
            unitThere.TakeDamage(selected.Atk);
            Haptics.Pulse();
            selected.MarkActed();
            Deselect();
            return;
        }

        // One of your ready units: select it (unless the current one has already moved).
        bool canSwitch = selected == null || !selected.HasMoved;
        if (IsReady(unitThere) && canSwitch)
        {
            selected = unitThere;
            ShowOptions(selected);
            return;
        }

        // Anything else: deselect.
        Deselect();
    }

    bool IsReady(Unit u) =>
        u != null && u.Team == Team.Player && !u.HasActed;

    // Paint the board for the selected unit: yellow = it, blue = can move, red = can attack.
    void ShowOptions(Unit u)
    {
        grid.ClearHighlights();

        if (!u.HasMoved)
        {
            grid.GetReachable(u, reach);
            foreach (var c in reach) grid.GetTile(c).SetHighlight(Highlight.Move);
        }

        grid.GetAttackTargets(u, targets);
        foreach (var t in targets) grid.GetTile(t.Coord).SetHighlight(Highlight.Attack);

        grid.GetTile(u.Coord).SetHighlight(Highlight.Selected);
    }

    // If the unit already moved, deselecting it means "wait": it's done for this turn.
    void Deselect()
    {
        if (selected != null && selected.HasMoved) selected.MarkActed();
        selected = null;
        grid.ClearHighlights();

        if (AllPlayersActed()) EndPlayerTurn();
    }

    bool AllPlayersActed()
    {
        foreach (var u in players)
            if (u.IsAlive && !u.HasActed) return false;
        return true;
    }

    // Also wired to the End Turn button's OnClick.
    public void EndPlayerTurn()
    {
        if (!playerTurn) return;
        if (selected != null && selected.HasMoved) selected.MarkActed();
        selected = null;
        grid.ClearHighlights();

        playerTurn = false;
        endTurnButton.interactable = false;
        banner.text = "Enemy turn";
        EnemyTurn();
    }

    // ---------- Enemy turn (placeholder; step 6 replaces this) ----------

    void EnemyTurn()
    {
        Debug.Log("[Turn] enemy turn (stub)");
        StartPlayerTurn();
    }

    // ---------- Helpers ----------

    // Taps on buttons (End Turn, Resume) shouldn't also tap the tile underneath.
    private PointerEventData pointer;
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

    bool IsOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;
        pointer ??= new PointerEventData(EventSystem.current);
        pointer.position = screenPos;
        EventSystem.current.RaycastAll(pointer, uiHits);
        return uiHits.Count > 0;
    }
}
