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
    
    [Header("Objects")]
    public GameObject groundTilePrefab;
    public GameObject playerPrefab;
    public GameObject blockPrefab;
    public GameObject mainCamera;
    
    [Header("Visuals")]
    public float inactiveLevelOpacity = 0.2f;
    public float fadeTransitionSpeed = 0.5f;
    
    public LevelData _currentLevelData;
    private GroundTile[,] _groundTiles;
    public GameObject _playerInstance; 
    public Player _playerScript;
    
    public Dictionary<Vector2Int, Block> _elevatorBlocks = new Dictionary<Vector2Int, Block>(); 
    public Dictionary<int, LevelObjects> _loadedLevels = new Dictionary<int, LevelObjects>(); 
    
    public class LevelObjects
    {
        public List<GameObject> tiles = new List<GameObject>();
        public List<GameObject> blocks = new List<GameObject>();
        public Dictionary<Vector2Int, GameObject> hiddenTiles = new Dictionary<Vector2Int, GameObject>();
        public float targetOpacity = 1f;
        public float currentOpacity = 1f;
    }
    
    private struct BlockSnapshot
    {
        public int blockId;
        public Block blockRef;
        public BlockData data;
        public BlockData.BlockColor blockColor;
        public List<BlockData.BlockColor> containedColors;
        public Vector3 worldPosition;
        public Vector2Int gridPosition;
        public int levelIndex;
        public bool isInHole;
        public bool isAtOriginLevel;
        public int originLevelIndex;
        public bool wasRegisteredAsElevator;
        public Vector3 runtimeDataPosition;
    }
    
    private class MoveRecord
    {
        public List<BlockSnapshot> blockSnapshots = new List<BlockSnapshot>();
        public Vector3 playerWorldPosition;
        public Vector2Int playerGridPosition;
        public int playerLevelIndex;
        public int levelIndex;
    }
    private List<MoveRecord> _undoStack = new List<MoveRecord>();
    private int _maxUndo = 10;
    private int _nextBlockId = 1;

    void Awake()  
    {
        Instance = this;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        
        BackgroundGenerator.CreateAnimatedBackground(mainCamera.GetComponent<Camera>());
        
        if (levelDatas == null || levelDatas.Length == 0 || groundTilePrefab == null)
        {
            Debug.LogError("Missing LevelData or GroundTile prefab and cannot generate level");
            return;
        }

        // Check if save file exists and load it
        if (SaveManager.Instance != null && SaveManager.Instance.SaveFileExists())
        {
            bool loadSuccess = SaveManager.Instance.LoadGame();
            if (loadSuccess)
            {
                Debug.Log("Game loaded from save file");
                return;
            }
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

    public void GenerateLevel(int levelIndex, bool skipBlocks = false)
    {
        if (_loadedLevels.ContainsKey(levelIndex))
        {
            Debug.Log($"Level {levelIndex} already loaded");
            return;
        }

        Debug.Log($"Generating level {levelIndex}");
        
        var levelData = levelDatas[levelIndex];
        float yOffset = levelIndex * verticalSpacing;
        var levelObjects = new LevelObjects();
        
        GameObject tilesParent = new GameObject($"Level_{levelIndex + 1}_Tiles");
        tilesParent.transform.position = new Vector3(0, yOffset, 0);
        
        for (int x = 0; x < levelData.levelHeight; x++)
        {
            for (int y = 0; y < levelData.levelHeight; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                
                if (gridPos == levelData.holePosition)
                {
                    continue;
                }
                
                Vector3 position = new Vector3(x, yOffset, y);
                GameObject tileObj = Instantiate(groundTilePrefab, position, Quaternion.identity);
                tileObj.transform.SetParent(tilesParent.transform);
                GroundTile tile = tileObj.GetComponent<GroundTile>();
                tile.Initialize(tile.data, gridPos);

                levelObjects.tiles.Add(tileObj); 
                
                if (levelIndex == currentLevelIndex)
                {
                    if (_groundTiles == null)
                    {
                        _groundTiles = new GroundTile[levelData.levelWidth, levelData.levelHeight];
                    }
                    
                    _groundTiles[x, y] = tile;
                }
            }
        }
        
        // Only spawn blocks if not loading from save
        if (!skipBlocks)
        {
            GameObject blocksParent = new GameObject($"Level_{levelIndex + 1}_Blocks");
            blocksParent.transform.position = new Vector3(0, yOffset, 0);

            foreach (var blockData in levelData.blocks)
            {
                Vector3 blockPosition = new Vector3(
                    blockData.BlockPosition.x,
                    yOffset + 1f,
                    blockData.BlockPosition.z
                );

                GameObject blockObj = Instantiate(blockPrefab, blockPosition, Quaternion.identity);
                blockObj.transform.SetParent(blocksParent.transform);
                Block blockComponent = blockObj.GetComponent<Block>();
                if (blockComponent != null)
                {
                    // reference to the original asset data
                    blockComponent.data = blockData;

                    // runtime data clone to mutate at runtime
                    blockComponent.runtimeData = Instantiate(blockData);
                    blockComponent.runtimeData.BlockPosition = new Vector3(blockData.BlockPosition.x, blockPosition.y, blockData.BlockPosition.z);

                    blockComponent.levelIndex = levelIndex;
                    EnsureBlockId(blockComponent);
                }

                levelObjects.blocks.Add(blockObj);
            }
        }
        _loadedLevels[levelIndex] = levelObjects;
    }

    private void EnsureBlockId(Block block)
    {
        if (block == null)
        {
            return;
        }
        if (block.blockId == 0)
        {
            block.blockId = _nextBlockId;
            _nextBlockId++;
        }
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
                
                if (blockComponent == null || !blockComponent._isInHole)
                {
                    Destroy(block);
                }
            }
        }
        
        _loadedLevels.Remove(levelIndex);
    }

    private void ManageLoadedLevels(bool loadMissingLevels = true)
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
                if (loadMissingLevels)
                {
                    GenerateLevel(level);
                }
            }
        }
        
        UpdateLevelOpacities();
    }

    public void UpdateLevelOpacities()
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
            block.levelIndex = currentLevelIndex;
            block.originLevelIndex = currentLevelIndex;
            block.isAtOriginLevel = true;
            Debug.Log($"Registered elevator at {position} on level {currentLevelIndex}");
            
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
        if (_elevatorBlocks.TryGetValue(position, out var block))
        {
            // only consider it an elevator for the current level when it's in the hole and the block is on this level
            return block != null && block._isInHole && block.levelIndex == currentLevelIndex;
        }
        return false;
    }

    public Block GetElevatorAt(Vector2Int position)
    {
        if (_elevatorBlocks.TryGetValue(position, out var block))
        {
            if (block != null && block._isInHole && block.levelIndex == currentLevelIndex)
                return block;
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
        
        RecordSnapshot();
        
        int targetLevel = elevator.GetTargetLevel();
        
        if (targetLevel < 0 || targetLevel >= levelDatas.Length)
        {
            Debug.Log("Cannot travel to that level");
            yield break;
        }

        bool goingUp = targetLevel > currentLevelIndex;
        
        _playerScript._gridMover.isMoving = true;

        float targetLevelY = targetLevel * verticalSpacing;
        float cameraY = targetLevelY + 19f;
        
        
        // y is treated as z here because we get elevator position from a top-down perspective
        Vector3 playerStart = _playerInstance.transform.position;
        Vector3 playerTarget = new Vector3(elevatorPosition.x, targetLevelY + 1f, elevatorPosition.y);
        
        Vector3 elevatorStart = elevator.transform.position;
        Vector3 elevatorTarget = new Vector3(elevatorPosition.x, targetLevelY, elevatorPosition.y);
        
        // the camera is an exception because it is from a side-view perspective
        Vector3 cameraStart = mainCamera.transform.position;
        Vector3 cameraTarget = new Vector3(cameraStart.x, cameraY, cameraStart.z);

        float duration = 1.5f;
        float elapsed = 0f;
        
        int fromLevel = currentLevelIndex;
        int toLevel = targetLevel;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);

            _playerInstance.transform.position = Vector3.Lerp(playerStart, playerTarget, smoothT);
            elevator.transform.position = Vector3.Lerp(elevatorStart, elevatorTarget, smoothT);
            mainCamera.transform.position = Vector3.Lerp(cameraStart, cameraTarget, smoothT);
            
            
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
        mainCamera.transform.position = cameraTarget;

        elevator.levelIndex = targetLevel;
        
        currentLevelIndex = targetLevel;
        _currentLevelData = Instantiate(levelDatas[currentLevelIndex]);
        _playerScript.gridPosition = elevatorPosition;
        
        elevator.isAtOriginLevel = !elevator.isAtOriginLevel;
        
        UpdateGroundTilesForCurrentLevel();
        ManageLoadedLevels(); 
        
        _playerScript._gridMover.isMoving = false;
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }
        
        //string direction = goingUp ? "up" : "down";
        //Debug.Log($"Arrived at level {currentLevelIndex} (went {direction})");
    }

    public void UpdateGroundTilesForCurrentLevel()
    {
        LevelData levelData = levelDatas[currentLevelIndex];
        _groundTiles = new GroundTile[levelData.levelHeight, levelData.levelHeight];

        GroundTile[] allTiles = FindObjectsByType<GroundTile>(FindObjectsSortMode.None);
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

        for (int x = 0; x < levelData.levelHeight; x++)
        {
            for (int y = 0; y < levelData.levelHeight; y++)
            {
                GroundTile tile = _groundTiles[x, y];
                if (tile != null)
                {
                    tile.occupant = null;
                    tile.isOccupied = false;
                }
            }
        }

        Block[] allBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
        foreach (Block block in allBlocks)
        {
            if (block.levelIndex == currentLevelIndex && !block._isInHole)
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
    public void RecordSnapshot()
    {
        var record = new MoveRecord();
        
        // player
        if (_playerInstance != null && _playerScript != null)
        {
            record.playerWorldPosition = _playerInstance.transform.position;
            record.playerGridPosition = _playerScript.gridPosition;
            record.playerLevelIndex = currentLevelIndex;
        }

        // blocks
        Block[] allBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
        foreach (var block in allBlocks)
        {
            EnsureBlockId(block);
            var sourceData = block.runtimeData != null ? block.runtimeData : block.data;
            var containedColors = sourceData != null && sourceData.containedColors != null && sourceData.containedColors.Count > 0
                ? new List<BlockData.BlockColor>(sourceData.containedColors)
                : new List<BlockData.BlockColor>(block.containedPrimaryColors);
            var snap = new BlockSnapshot
            {
                blockId = block.blockId,
                blockRef = block,
                data = block.data,
                blockColor = sourceData != null ? sourceData.blockColor : BlockData.BlockColor.Red,
                containedColors = containedColors,
                worldPosition = block.transform.position,
                gridPosition = block.gridPosition,
                levelIndex = block.levelIndex,
                isInHole = block._isInHole,
                isAtOriginLevel = block.isAtOriginLevel,
                originLevelIndex = block.originLevelIndex,
                wasRegisteredAsElevator = (_elevatorBlocks.TryGetValue(block.gridPosition, out var b) && b == block),
                runtimeDataPosition = block.runtimeData != null ? block.runtimeData.BlockPosition : block.transform.position
            };
            record.blockSnapshots.Add(snap);
        }

        _undoStack.Add(record);
        while (_undoStack.Count > _maxUndo)
        {
            _undoStack.RemoveAt(0);
        }
    }
    public void UndoLastMove()
    {
        if (_undoStack.Count == 0)
        {
            Debug.Log("Nothing to undo");
            return;
        }

        MoveRecord record = null;
        for (int i = _undoStack.Count - 1; i >= 0; i--)
        {
            if (_undoStack[i].playerLevelIndex == currentLevelIndex)
            {
                record = _undoStack[i];
                _undoStack.RemoveRange(i, _undoStack.Count - i);
                break;
            }
        }
        if (record == null)
        {
            Debug.Log("Nothing to undo on this level");
            return;
        }

        // restore blocks
        _elevatorBlocks.Clear();
        Block[] existingBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
        Dictionary<int, Block> existingById = new Dictionary<int, Block>();
        foreach (var block in existingBlocks)
        {
            if (block == null) continue;
            EnsureBlockId(block);
            if (!existingById.ContainsKey(block.blockId))
            {
                existingById[block.blockId] = block;
            }
        }

        HashSet<int> snapshotIds = new HashSet<int>();
        foreach (var snap in record.blockSnapshots)
        {
            snapshotIds.Add(snap.blockId);
            Block block;
            if (!existingById.TryGetValue(snap.blockId, out block) || block == null)
            {
                GameObject blockObj = Instantiate(blockPrefab, snap.worldPosition, Quaternion.identity);
                block = blockObj.GetComponent<Block>();
                if (block == null)
                {
                    continue;
                }
                block.skipStartInit = true;
                block.blockId = snap.blockId;
                block.data = snap.data;
                if (snap.data != null)
                {
                    block.runtimeData = Instantiate(snap.data);
                }
                if (block.runtimeData != null)
                {
                    block.runtimeData.blockColor = snap.blockColor;
                    block.runtimeData.containedColors = snap.containedColors != null
                        ? new List<BlockData.BlockColor>(snap.containedColors)
                        : new List<BlockData.BlockColor>();
                    block.runtimeData.BlockPosition = snap.runtimeDataPosition;
                }
                existingById[block.blockId] = block;
            }

            block.transform.position = snap.worldPosition;
            block.gridPosition = snap.gridPosition;
            block.levelIndex = snap.levelIndex;
            block._isInHole = snap.isInHole;
            block.isAtOriginLevel = snap.isAtOriginLevel;
            block.originLevelIndex = snap.originLevelIndex;

            if (block.runtimeData == null && snap.data != null)
            {
                block.runtimeData = Instantiate(snap.data);
            }
            if (block.runtimeData != null)
            {
                block.runtimeData.BlockPosition = snap.runtimeDataPosition;
                block.runtimeData.blockColor = snap.blockColor;
                block.runtimeData.containedColors = snap.containedColors != null
                    ? new List<BlockData.BlockColor>(snap.containedColors)
                    : new List<BlockData.BlockColor>();
            }
            block.ApplyRuntimeData();

            if (snap.wasRegisteredAsElevator)
            {
                _elevatorBlocks[snap.gridPosition] = block;
            }
        }

        foreach (var block in existingBlocks)
        {
            if (block == null) continue;
            if (!snapshotIds.Contains(block.blockId))
            {
                Destroy(block.gameObject);
            }
        }
        if (snapshotIds.Count > 0)
        {
            int maxId = 0;
            foreach (int id in snapshotIds)
            {
                if (id > maxId) maxId = id;
            }
            _nextBlockId = maxId + 1;
        }

        // restore player and level
        currentLevelIndex = record.playerLevelIndex;
        _currentLevelData = Instantiate(levelDatas[currentLevelIndex]);
        if (_playerInstance != null)
        {
            _playerInstance.transform.position = record.playerWorldPosition;
        }
        if (_playerScript != null)
        {
            _playerScript.gridPosition = record.playerGridPosition;
        }

        // refresh ground tiles, level loading and visuals
        UpdateGroundTilesForCurrentLevel();
        ManageLoadedLevels(loadMissingLevels: false);
        UpdateLevelOpacities();

        Debug.Log("Undo performed");
    }


    
    
}
