using System.Net;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public static LevelManager Instance;
    
    [Header("Level Data")]
    public LevelData levelData;
    
    [Header("Tiles")]
    public GameObject groundTilePrefab;
    
    private GroundTile[,] _groundTiles;

    void Awake()  
    {
        Instance = this;
        
        if (levelData == null || groundTilePrefab == null)
        {
            Debug.LogError("Missing LevelData or GroundTile prefab and cannot generate level");
            return;
        }
        
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        _groundTiles = new GroundTile[levelData.levelHeight, levelData.levelHeight];

        for (int x = 0; x < levelData.levelHeight; x++)
        {
            for (int y = 0; y < levelData.levelHeight; y++)
            {

                if (x == levelData.holePosition.x && y == levelData.holePosition.y)
                {
                    continue;
                }
                
                _groundTiles[x, y] = Instantiate(groundTilePrefab, new Vector3(x, 0, y), Quaternion.identity).GetComponent<GroundTile>();
                _groundTiles[x, y].Initialize(_groundTiles[x,y].data, new Vector2Int(x, y));
            }
        }
        

    }
    
    public bool CheckOutOfBounds(Vector2Int position)
    {
        
        if (position.x < 0 || position.x >= levelData.levelHeight ||
            position.y < 0 || position.y >= levelData.levelHeight)
        {
            return true;
        }
        
        
        if (_groundTiles[position.x, position.y] is null)
        {
            return true;
        }

        return false;
    }
    
    public GroundTile GetTileAt(Vector2Int position)
    {
        
        if (CheckOutOfBounds(position))
        {
            return null;
        }
        
        return _groundTiles[position.x, position.y];
    }
}
