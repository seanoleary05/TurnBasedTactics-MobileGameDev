using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
    [SerializeField] private GameObject resultPanel;   // shown on victory/defeat
    [SerializeField] private TMP_Text resultText;

    [Header("Enemy pacing (seconds, scaled so pause freezes it)")]
    [SerializeField] private float enemyThinkDelay = 0.35f;
    [SerializeField] private float enemyActionDelay = 0.4f;

    [Header("Economy")]
    [Tooltip("Coins earned per enemy defeated (the core verb). Tune this, not the cost curve.")]
    [SerializeField] private int coinsPerKill = 10;

    private GridManager grid;
    private readonly List<Unit> players = new List<Unit>();
    private readonly List<Unit> enemies = new List<Unit>();

    private bool playerTurn;   // false during the enemy turn and after win/lose
    private bool gameOver;
    private Unit selected;     // the player unit you tapped, or null
    private int turn;

    // Reused lists so tapping doesn't create garbage.
    private readonly List<Vector2Int> reach = new List<Vector2Int>();
    private readonly List<Unit> targets = new List<Unit>();

    private float battleStartTime;

    void OnEnable()
    {
        TapSwipeInput.Tapped += OnTapped;
        Unit.Died += OnUnitDied;
    }

    void OnDisable()
    {
        TapSwipeInput.Tapped -= OnTapped;
        Unit.Died -= OnUnitDied;
    }

    void Start()   // Start, not Awake: GridManager builds the board in its Awake
    {
        grid = GridManager.Instance;

        foreach (var c in playerSpawns) players.Add(Spawn(Team.Player, c));
        foreach (var c in enemySpawns)  enemies.Add(Spawn(Team.Enemy, c));

        if (resultPanel != null) resultPanel.SetActive(false);
        battleStartTime = Time.time;

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
        foreach (var u in enemies) if (u.IsAlive) u.ResetTurn();   // un-grey enemies
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
            unitThere.TakeDamage(selected.Atk + UpgradeAtkBonus);
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

    // Each wallet upgrade level adds +1 damage to every player attack.
    static int UpgradeAtkBonus => Wallet.Instance != null ? Wallet.Instance.Level : 0;

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
        StartCoroutine(EnemyTurn());
    }

    // ---------- Enemy turn ----------
    // Each enemy: if no one is adjacent, walk to the reachable tile closest to a player,
    // then hit the adjacent player with the lowest HP (finish off the weak first).

    IEnumerator EnemyTurn()
    {
        foreach (var e in enemies)
        {
            if (gameOver) yield break;
            if (!e.IsAlive) continue;

            grid.ClearHighlights();
            grid.GetTile(e.Coord).SetHighlight(Highlight.Selected);
            yield return new WaitForSeconds(enemyThinkDelay);

            grid.GetAttackTargets(e, targets);
            if (targets.Count == 0)
            {
                Vector2Int dest = BestApproachTile(e);
                if (dest != e.Coord)
                {
                    e.Place(dest);
                    grid.ClearHighlights();
                    grid.GetTile(dest).SetHighlight(Highlight.Selected);
                    yield return new WaitForSeconds(enemyActionDelay);
                }
                grid.GetAttackTargets(e, targets);
            }

            Unit victim = Weakest(targets);
            if (victim != null)
            {
                grid.GetTile(victim.Coord).SetHighlight(Highlight.Attack);
                yield return new WaitForSeconds(enemyActionDelay);
                victim.TakeDamage(e.Atk);
                Haptics.Pulse();
            }

            e.MarkActed();
        }

        grid.ClearHighlights();
        if (!gameOver) StartPlayerTurn();
    }

    // The reachable tile (or the current one) with the smallest distance to any living player.
    Vector2Int BestApproachTile(Unit e)
    {
        Vector2Int best = e.Coord;
        int bestDist = DistanceToNearestPlayer(best);

        grid.GetReachable(e, reach);
        foreach (var c in reach)
        {
            int d = DistanceToNearestPlayer(c);
            if (d < bestDist) { bestDist = d; best = c; }
        }
        return best;
    }

    int DistanceToNearestPlayer(Vector2Int c)
    {
        int best = int.MaxValue;
        foreach (var p in players)
            if (p.IsAlive) best = Mathf.Min(best, GridManager.Distance(c, p.Coord));
        return best;
    }

    static Unit Weakest(List<Unit> list)
    {
        Unit best = null;
        foreach (var u in list)
            if (best == null || u.Hp < best.Hp) best = u;
        return best;
    }

    // ---------- Win / lose ----------

    void OnUnitDied(Unit u)
    {
        if (gameOver) return;
        // Only the player can defeat enemies, so an enemy death is the core-verb payout.
        if (u.Team == Team.Enemy && Wallet.Instance != null) Wallet.Instance.Earn(coinsPerKill);
        if (!AnyAlive(enemies)) EndBattle(true);
        else if (!AnyAlive(players)) EndBattle(false);
    }

    static bool AnyAlive(List<Unit> list)
    {
        foreach (var u in list) if (u.IsAlive) return true;
        return false;
    }

    void EndBattle(bool won)
    {
        gameOver = true;
        playerTurn = false;
        selected = null;
        StopAllCoroutines();
        grid.ClearHighlights();
        endTurnButton.interactable = false;

        int survivors = 0;
        foreach (var u in players) if (u.IsAlive) survivors++;
        float seconds = Time.time - battleStartTime;

        banner.text = won ? "Victory!" : "Defeat";
        if (resultText != null)
            resultText.text = won
                ? $"Victory!\nCleared in {turn} turns\n{survivors}/{players.Count} units survived"
                : $"Defeat\nYour squad fell on turn {turn}";
        if (resultPanel != null) resultPanel.SetActive(true);

        Telemetry.Log(won ? "level_complete" : "level_fail",
            ("level", "battle_01"), ("turns", turn),
            ("survivors", survivors), ("seconds", seconds.ToString("F1")));
    }

    // Wired to the result panel's Retry button.
    public void Restart()
    {
        Telemetry.Log("level_retry", ("level", "battle_01"));
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
