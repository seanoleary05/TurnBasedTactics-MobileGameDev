using UnityEngine;

public class GridInital : MonoBehaviour
{
    public GameObject tilePrefab;

    
    void Awake(){
    for (int row = 0; row < 4; row++)
    {
        for (int col = 0; col < 4; col++)
        {
            Instantiate(tilePrefab, new Vector3(col, row, 0), Quaternion.identity);
        }
    }
    }
    
}
