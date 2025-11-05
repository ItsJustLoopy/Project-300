using System.Collections;
using System.Collections.Generic;
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
    
    [Header("Visibility Settings")]
    public float inactiveLevelOpacity = 0.2f;
    public float fadeTransitionSpeed = 0.5f;
    
    
    public LevelData _currentLevelData;
    private GroundTile[,] _groundTiles;
    private GameObject _playerInstance;
    private Player _playerScript;
    
    
    private Dictionary<Vector2Int, Block> _elevatorBlocks = new Dictionary<Vector2Int, Block>();
    
    
    private Dictionary<int, LevelObjects> _loadedLevels = new Dictionary<int, LevelObjects>();
    private class LevelObjects
    {
        public List<GameObject> tiles = new List<GameObject>();
        public List<GameObject> blocks = new List<GameObject>();
        public float targetOpacity = 1f;
        public float currentOpacity = 1f;
    }

    void Awake()  
    {
        Instance = this;
        
        if (levelDatas == null || levelDatas.Length == 0 || groundTilePrefab == null)
        {
            Debug.LogError("Missing LevelData or GroundTile prefab and cannot generate level");
            return;
        }
        
        
        GenerateLevel(currentLevelIndex);
        _currentLevelData = levelDatas[currentLevelIndex];
        
        SpawnPlayer();
        
        UpdateLevelOpacities();
    }

    void Update()
    {
        
        foreach (var lvl in _loadedLevels)
        {
            LevelObjects levelObjs = lvl.Value;
            
            
            if (Mathf.Abs(levelObjs.currentOpacity - levelObjs.targetOpacity) > 0.01f)
            {
                levelObjs.currentOpacity = Mathf.Lerp(levelObjs.currentOpacity, levelObjs.targetOpacity, 
                    Time.deltaTime * fadeTransitionSpeed);
                SetLevelOpacity(lvl.Key, levelObjs.currentOpacity);
            }
        }
    }

    private void GenerateLevel(int levelIndex)
    {
        if (_loadedLevels.ContainsKey(levelIndex))
        {
            Debug.Log($"Level {levelIndex} already loaded");
            return;
        }

        Debug.Log($"Generating level {levelIndex}");
        
        LevelData levelData = levelDatas[levelIndex];
        float yOffset = levelIndex * verticalSpacing;
        
        LevelObjects levelObjects = new LevelObjects();
        
        
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
                
                levelObjects.tiles.Add(tileObj);
                
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
            Vector3 blockPosition = new Vector3(
                blockData.BlockPosition.x,
                yOffset + 1f,
                blockData.BlockPosition.z
            );
            
            GameObject blockObj = Instantiate(blockPrefab, blockPosition, Quaternion.identity);
            Block blockComponent = blockObj.GetComponent<Block>();
            if (blockComponent != null)
            {
                blockComponent.data = blockData;
                blockComponent.levelIndex = levelIndex;
            }
            
            levelObjects.blocks.Add(blockObj);
        }
        
        _loadedLevels[levelIndex] = levelObjects;
    }

    private void UnloadLevel(int levelIndex)
    {
        if (!_loadedLevels.ContainsKey(levelIndex))
        {
            return;
        }

        Debug.Log($"Unloading level {levelIndex}");
        
        LevelObjects levelObjects = _loadedLevels[levelIndex];
        
        
        foreach (GameObject tile in levelObjects.tiles)
        {
            if (tile != null)
            {
                Destroy(tile);
            }
        }
        
        foreach (GameObject block in levelObjects.blocks)
        {
            if (block != null)
            {
                Block blockComponent = block.GetComponent<Block>();
                
                if (blockComponent == null || !blockComponent.IsInHole())
                {
                    Destroy(block);
                }
            }
        }
        
        _loadedLevels.Remove(levelIndex);
    }

    private void ManageLoadedLevels()
    {
        
        List<int> levelsToKeep = new List<int>();
        
        if (currentLevelIndex > 0)
        {
            levelsToKeep.Add(currentLevelIndex - 1); 
        }
        
        levelsToKeep.Add(currentLevelIndex); 
        
        if (currentLevelIndex + 1 < levelDatas.Length)
        {
            levelsToKeep.Add(currentLevelIndex + 1); 
        }
        
        
        List<int> levelsToUnload = new List<int>();
        foreach (int loadedLevel in _loadedLevels.Keys)
        {
            if (!levelsToKeep.Contains(loadedLevel))
            {
                levelsToUnload.Add(loadedLevel);
            }
        }
        
        foreach (int level in levelsToUnload)
        {
            UnloadLevel(level);
        }
        
        
        foreach (int level in levelsToKeep)
        {
            if (!_loadedLevels.ContainsKey(level))
            {
                GenerateLevel(level);
            }
        }
        
        UpdateLevelOpacities();
    }

    private void UpdateLevelOpacities()
    {
        foreach (var kvp in _loadedLevels)
        {
            int levelIndex = kvp.Key;
            LevelObjects levelObjs = kvp.Value;
            
            
            if (levelIndex == currentLevelIndex)
            {
                levelObjs.targetOpacity = 1f;
            }
            else
            {
                levelObjs.targetOpacity = inactiveLevelOpacity;
            }
        }
    }

    private void SetLevelOpacity(int levelIndex, float opacity)
    {
        if (!_loadedLevels.ContainsKey(levelIndex))
        {
            return;
        }

        LevelObjects levelObjs = _loadedLevels[levelIndex];
        
        
        foreach (GameObject tileObj in levelObjs.tiles)
        {
            if (tileObj != null)
            {
                SetObjectOpacity(tileObj, opacity);
            }
        }
        
        
        foreach (GameObject blockObj in levelObjs.blocks)
        {
            if (blockObj != null)
            {
                SetObjectOpacity(blockObj, opacity);
            }
        }
    }

    private void SetObjectOpacity(GameObject obj, float opacity)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            foreach (Material mat in renderer.materials)
            {
                
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = opacity;
                    mat.color = color;
                    
                    
                    if (opacity < 1f)
                    {
                        mat.SetFloat("_Surface", 1); 
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }
                    else
                    {
                        mat.SetFloat("_Surface", 0); 
                        mat.renderQueue = 2000;
                    }
                }
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
        _playerScript = _playerInstance.GetComponent<Player>();
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

    public void RegisterElevator(Vector2Int position, Block block)
    {
        if (!_elevatorBlocks.ContainsKey(position))
        {
            _elevatorBlocks[position] = block;
            block.originLevelIndex = currentLevelIndex;
            block.isAtOriginLevel = true;
            Debug.Log($"Registered elevator at {position} on level {currentLevelIndex}");
            
            // Check if we need to load the next level
            int nextLevel = currentLevelIndex + 1;
            if (nextLevel < levelDatas.Length && !_loadedLevels.ContainsKey(nextLevel))
            {
                GenerateLevel(nextLevel);
                UpdateLevelOpacities();
            }
        }
    }

    public bool IsElevatorAt(Vector2Int position)
    {
        return _elevatorBlocks.ContainsKey(position);
    }

    public Block GetElevatorAt(Vector2Int position)
    {
        if (_elevatorBlocks.ContainsKey(position))
        {
            return _elevatorBlocks[position];
        }
        return null;
    }

    public IEnumerator UseElevator(Vector2Int elevatorPosition)
    {
        Block elevator = GetElevatorAt(elevatorPosition);
        if (elevator == null)
        {
            Debug.LogError("No elevator found at position");
            yield break;
        }

        int targetLevel = elevator.GetTargetLevel();
        
        if (targetLevel < 0 || targetLevel >= levelDatas.Length)
        {
            Debug.Log("Cannot travel to that level");
            yield break;
        }

        bool goingUp = targetLevel > currentLevelIndex;
        
        _playerScript.isMoving = true;

        float targetLevelY = targetLevel * verticalSpacing;
        
        Vector3 playerStart = _playerInstance.transform.position;
        Vector3 playerTarget = new Vector3(elevatorPosition.x, targetLevelY + 1f, elevatorPosition.y);
        
        Vector3 elevatorStart = elevator.transform.position;
        Vector3 elevatorTarget = new Vector3(elevatorPosition.x, targetLevelY, elevatorPosition.y);

        float duration = 1.5f;
        float elapsed = 0f;

        Debug.Log($"Elevator traveling from level {currentLevelIndex} to level {targetLevel}");

        
        int fromLevel = currentLevelIndex;
        int toLevel = targetLevel;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);

            _playerInstance.transform.position = Vector3.Lerp(playerStart, playerTarget, smoothT);
            elevator.transform.position = Vector3.Lerp(elevatorStart, elevatorTarget, smoothT);
            
            
            if (_loadedLevels.ContainsKey(fromLevel))
            {
                float fromOpacity = Mathf.Lerp(1f, inactiveLevelOpacity, t);
                SetLevelOpacity(fromLevel, fromOpacity);
            }
            
            if (_loadedLevels.ContainsKey(toLevel))
            {
                float toOpacity = Mathf.Lerp(inactiveLevelOpacity, 1f, t);
                SetLevelOpacity(toLevel, toOpacity);
            }
            
            yield return null;
        }

        _playerInstance.transform.position = playerTarget;
        elevator.transform.position = elevatorTarget;

        
        currentLevelIndex = targetLevel;
        _currentLevelData = ScriptableObject.Instantiate(levelDatas[currentLevelIndex]);
        _playerScript.gridPosition = elevatorPosition;
        
        elevator.isAtOriginLevel = !elevator.isAtOriginLevel;
        
        UpdateGroundTilesForCurrentLevel();
        ManageLoadedLevels(); 
        
        _playerScript.isMoving = false;
        
        string direction = goingUp ? "up" : "down";
        Debug.Log($"Arrived at level {currentLevelIndex} (went {direction})");
    }

    private void UpdateGroundTilesForCurrentLevel()
    {
        LevelData levelData = levelDatas[currentLevelIndex];
        _groundTiles = new GroundTile[levelData.levelHeight, levelData.levelHeight];

        GroundTile[] allTiles = FindObjectsOfType<GroundTile>();
        float currentLevelY = currentLevelIndex * verticalSpacing;

        foreach (GroundTile tile in allTiles)
        {
            if (Mathf.Approximately(tile.transform.position.y, currentLevelY))
            {
                Vector2Int gridPos = tile.gridPosition;
                if (gridPos.x >= 0 && gridPos.x < levelData.levelHeight &&
                    gridPos.y >= 0 && gridPos.y < levelData.levelHeight)
                {
                    _groundTiles[gridPos.x, gridPos.y] = tile;
                }
            }
        }

        
        Block[] allBlocks = FindObjectsOfType<Block>();
        foreach (Block block in allBlocks)
        {
            if (block.levelIndex == currentLevelIndex && !block.IsInHole())
            {
                GroundTile tile = GetTileAt(block.gridPosition);
                if (tile != null)
                {
                    tile.occupant = block;
                    tile.isOccupied = true;
                }
            }
        }
    }
}
