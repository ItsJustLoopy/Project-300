using System.Net;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public static LevelManager Instance;
    
    [Header("Level Data")]
    public LevelData[] levelDatas;
    
    
    [Header("Level Settings")]
    public int currentLevelIndex = 0;
    public float verticalSpacing = 5f;
    
    [Header("Prefabs")]
    public GameObject groundTilePrefab;
    public GameObject playerPrefab;
    public GameObject blockPrefab;
    
    
    public LevelData _currentLevelData;
    private GroundTile[,] _groundTiles;
    private GameObject _playerInstance;

    void Awake()  
    {
        Instance = this;
        
        if (levelDatas == null || levelDatas.Length == 0 || groundTilePrefab == null)
        {
            Debug.LogError("Missing LevelData or GroundTile prefab and cannot generate level");
            return;
        }
        
        GenerateAllLevels();
        SpawnPlayer();
    }

    public void GenerateAllLevels()
    {
        for (int i = 0; i < levelDatas.Length; i++)
        {
            GenerateLevel(levelDatas[i], i);
        }
        
        _currentLevelData = levelDatas[currentLevelIndex];
    }

    public void GenerateLevel(LevelData levelData, int levelIndex)
    {
        float yOffset = levelIndex * verticalSpacing;
        
        // TODO: Store tiles in the level data so we can make irregular formed levels
        for (int x = 0; x < levelData.levelHeight; x++)
        {
            for (int y = 0; y < levelData.levelHeight; y++)
            {

                if (x == levelData.holePosition.x && y == levelData.holePosition.y)
                {
                    continue;
                }
                
                Vector3 position = new Vector3(x, yOffset, y);
                GameObject tileObj = Instantiate(groundTilePrefab, position, Quaternion.identity);
                GroundTile tile = tileObj.GetComponent<GroundTile>();
                tile.Initialize(tile.data, new Vector2Int(x, y));
                
                
                if (levelIndex == currentLevelIndex)
                {
                    if (_groundTiles == null)
                    {
                        _groundTiles = new GroundTile[levelData.levelHeight, levelData.levelHeight];
                    }
                    _groundTiles[x, y] = tile;
                }
            }
        }
        
        foreach (var blockData in levelData.blocks)
        {
            GameObject blockObj = Instantiate(blockPrefab, blockData.BlockPosition, Quaternion.identity);
            Block blockComponent = blockObj.GetComponent<Block>();
            if (blockComponent != null)
            {
                blockComponent.data = blockData;
            }
        }

    }
    
    public void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned");
            return;
        }
        
        float yOffset = currentLevelIndex * verticalSpacing;
        Vector3 spawnPosition = new Vector3(
            _currentLevelData.playerSpawn.x, 
            yOffset + 1f, 
            _currentLevelData.playerSpawn.y
        );
        
        
        _playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
    
    public bool CheckOutOfBounds(Vector2Int position)
    {
        if (position.x < 0 || position.x >= _currentLevelData.levelHeight ||
            position.y < 0 || position.y >= _currentLevelData.levelHeight)
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
    
    public LevelData GetCurrentLevelData()
    {
        return _currentLevelData;
    }
}
