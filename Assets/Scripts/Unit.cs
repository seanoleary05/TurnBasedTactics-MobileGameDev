using UnityEngine;
using System;
using TMPro;


public enum Team { Player, Enemy }


[RequireComponent(typeof(SpriteRenderer))]
public class Unit : MonoBehaviour
{

    public static event Action<Unit> Died;

    [SerializeField] private Team team = Team.Player;
    [SerializeField] private int maxHp = 5;
    [SerializeField] private int atk = 2;
    [SerializeField] private int move = 3;

    [SerializeField] private TMP_Text hpLabel;

    [SerializeField] private Color playerColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color enemyColor  = new Color(1f, 0.4f, 0.2f);
    [SerializeField] private Color spentColor  = new Color(0.5f, 0.5f, 0.5f);


    private SpriteRenderer _sr;
    private TextMesh _hpText;

    public Team Team => team;
    public int MaxHp => maxHp;
    public int Hp { get; private set; }
    public int Atk => atk;
    public int Move => move;
    public Vector2Int Coord { get; private set; }
    public bool IsAlive => Hp > 0;
    public bool HasMoved { get; set; }
    public bool HasActed { get; private set; }


    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _hpText = GetComponent<TextMesh>();
        _sr.sortingLayerName = "UI Layer2";   // tiles are on UI Layer1, so units draw on top
        _sr.sortingOrder = 0;
    }


    public void Init(Team t, Vector2Int start, int hpBonus = 0, int atkBonus = 0)
    {
        team = t;
        maxHp += hpBonus;
        atk += atkBonus;
        Hp = maxHp;
    
        name = $"{t}_{start.x}_{start.y}";
        if (t == Team.Enemy) transform.rotation = Quaternion.Euler(0, 0, 180f); // enemies face down
        if (hpLabel != null) hpLabel.transform.rotation = Quaternion.identity;
        RefreshHpLabel();
        Place(start);
        ResetTurn();
    }

        public void Place(Vector2Int c)
    {
        var grid = GridManager.Instance;
        grid.ClearOccupant(Coord, this);
        Coord = c;
        grid.SetOccupant(c, this);
        transform.position = grid.GridToWorld(c);
    }


    public void MarkActed()
    {
        HasActed = true;
        HasMoved = true;
        _sr.color = spentColor;
    }

    public void ResetTurn()
    {
        HasMoved = false;
        HasActed = false;
        _sr.color = team == Team.Player ? playerColor : enemyColor;
    }

    public void TakeDamage(int amount)
    {
       
        if (!IsAlive) return;
        Hp = Mathf.Max(0, Hp - amount);
        RefreshHpLabel();
        if (Hp > 0) return;

        GridManager.Instance.ClearOccupant(Coord, this);
        gameObject.SetActive(false);   // deactivate rather than Destroy, so it can be pooled later
        Died?.Invoke(this);
        
    }
                                                                                                                                                                    
 void RefreshHpLabel()                                                                                                                                                                 
 {                                                                                                                                                                                     
     if (hpLabel == null) return;                                                                                                            
     hpLabel.text = Hp.ToString();                                                                                                                         
                                                                                      
}
}
