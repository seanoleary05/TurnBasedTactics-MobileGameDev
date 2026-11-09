using System.Collections.Generic;
using UnityEngine;

public class DebugSpawn : MonoBehaviour
{
    public Unit unitPrefab;
    readonly List<Vector2Int> _reach = new List<Vector2Int>();
    readonly List<Unit> _targets = new List<Unit>();

    void Start()
    {
        var grid = GridManager.Instance;
        var hero = Instantiate(unitPrefab); hero.Init(Team.Player, new Vector2Int(2, 2));
        Instantiate(unitPrefab).Init(Team.Enemy,  new Vector2Int(2, 3)); // adjacent, blocks north
        Instantiate(unitPrefab).Init(Team.Player, new Vector2Int(3, 2)); // ally, blocks east

        grid.GetReachable(hero, _reach);
        foreach (var c in _reach) grid.GetTile(c).SetHighlight(Highlight.Move);

        grid.GetAttackTargets(hero, _targets);
        foreach (var u in _targets) grid.GetTile(u.Coord).SetHighlight(Highlight.Attack);

        grid.GetTile(hero.Coord).SetHighlight(Highlight.Selected);
        Debug.Log($"reach={_reach.Count} targets={_targets.Count}");
    }
}
