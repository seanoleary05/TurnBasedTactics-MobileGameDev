
using UnityEngine;

public class GridInital : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform gridParent;

[SerializeField] private Transform _cam;
    void Awake(){
    for (int row = 0; row < 3; row++)
    {
        for (int col = 0; col < 4; col++)
        {
            GameObject tile = Instantiate(tilePrefab, new UnityEngine.Vector3(gridParent.position.x + row, gridParent.position.y + col, 0), Quaternion.identity);
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            sr.name = "Tile_" + row + "_" + col;
            sr.sortingLayerName = "UI Layer1";
            sr.sortingOrder = 1;
        }
    }
    _cam.transform.position = new UnityEngine.Vector3(gridParent.position.x + 1.5f, gridParent.position.y + 2f, -10f);
    }
    
}
