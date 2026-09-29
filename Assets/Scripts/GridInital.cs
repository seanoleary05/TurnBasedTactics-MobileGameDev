using UnityEngine;

public class GridInital : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform gridParent;

    
    void Awake(){
    for (int row = 0; row < 3; row++)
    {
        for (int col = 0; col < 4; col++)
        {
            GameObject tile = Instantiate(tilePrefab, new Vector3(gridParent.position.x + row, gridParent.position.y + col, 0), Quaternion.identity);
            SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
            sr.name = "Tile_" + row + "_" + col;
            sr.sortingLayerName = "UI Layer1";
            sr.sortingOrder = 1;
        }
    }
    }
    
}
