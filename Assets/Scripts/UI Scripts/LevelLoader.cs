using System.Collections.Generic;
using UnityEngine;

public class LevelLoader
{
    public class LevelObjects
    {
        public List<GameObject> tiles = new List<GameObject>();
        public List<GameObject> blocks = new List<GameObject>();
        public Dictionary<Vector2Int, GameObject> hiddenTiles = new Dictionary<Vector2Int, GameObject>();
        public float targetOpacity = 1f;
        public float currentOpacity = 1f;
    }

    private readonly LevelManager _levelManager;
    private GroundTile[,] _groundTiles;
    private readonly Dictionary<int, LevelObjects> _loadedLevels = new Dictionary<int, LevelObjects>();

    public IReadOnlyDictionary<int, LevelObjects> LoadedLevels => _loadedLevels;

    public LevelLoader(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }

    public void GenerateLevel(int levelIndex, bool skipBlocks = false)
    {
        if (_loadedLevels.ContainsKey(levelIndex))
        {
            Debug.Log($"Level {levelIndex} already loaded");
            return;
        }

        Debug.Log($"Generating level {levelIndex}");

        var levelData = _levelManager.levelDatas[levelIndex];
        float yOffset = levelIndex * _levelManager.verticalSpacing;
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
                GameObject tileObj = Object.Instantiate(_levelManager.groundTilePrefab, position, Quaternion.identity);
                tileObj.transform.SetParent(tilesParent.transform);
                GroundTile tile = tileObj.GetComponent<GroundTile>();
                tile.Initialize(tile.data, gridPos);

                levelObjects.tiles.Add(tileObj);

                if (TryGetArrowAt(levelData, gridPos, out var arrowData))
                {
                    ArrowTile arrow = tileObj.GetComponent<ArrowTile>();
                    if (arrow == null)
                    {
                        arrow = tileObj.AddComponent<ArrowTile>();
                    }
                    arrow.Initialize(arrowData.direction, arrowData.color, _levelManager.arrowPrefab, _levelManager.arrowYOffset);
                }

                if (levelIndex == _levelManager.currentLevelIndex)
                {
                    if (_groundTiles == null)
                    {
                        _groundTiles = new GroundTile[levelData.levelWidth, levelData.levelHeight];
                    }

                    _groundTiles[x, y] = tile;
                }
            }
        }

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

                GameObject blockObj = Object.Instantiate(_levelManager.blockPrefab, blockPosition, Quaternion.identity);
                blockObj.transform.SetParent(blocksParent.transform);
                Block blockComponent = blockObj.GetComponent<Block>();
                if (blockComponent != null)
                {
                    blockComponent.data = blockData;
                    blockComponent.runtimeData = Object.Instantiate(blockData);
                    blockComponent.runtimeData.BlockPosition = new Vector3(blockData.BlockPosition.x, blockPosition.y, blockData.BlockPosition.z);
                    blockComponent.levelIndex = levelIndex;
                    _levelManager.undo.EnsureBlockId(blockComponent);
                }

                levelObjects.blocks.Add(blockObj);
            }
        }

        _loadedLevels[levelIndex] = levelObjects;
    }

    private bool TryGetArrowAt(LevelData levelData, Vector2Int position, out LevelData.ArrowData arrowData)
    {
        arrowData = null;
        if (levelData.arrows == null)
        {
            return false;
        }
        foreach (var arrow in levelData.arrows)
        {
            if (arrow.position == position)
            {
                arrowData = arrow;
                return true;
            }
        }
        return false;
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
                Object.Destroy(tile);
            }
        }

        foreach (GameObject block in levelObjects.blocks)
        {
            if (block != null)
            {
                Block blockComponent = block.GetComponent<Block>();
                if (blockComponent == null || !blockComponent._isInHole)
                {
                    Object.Destroy(block);
                }
            }
        }

        levelObjects.blocks.Clear();

        _loadedLevels.Remove(levelIndex);
    }

    public void ManageLoadedLevels(bool loadMissingLevels = true)
    {
        List<int> levelsToKeep = new List<int>();

        if (_levelManager.currentLevelIndex > 0)
        {
            levelsToKeep.Add(_levelManager.currentLevelIndex - 1);
        }

        levelsToKeep.Add(_levelManager.currentLevelIndex);

        if (_levelManager.currentLevelIndex + 1 < _levelManager.levelDatas.Length)
        {
            levelsToKeep.Add(_levelManager.currentLevelIndex + 1);
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

        _levelManager.visuals.UpdateLevelOpacities();
    }

    public void UpdateGroundTilesForCurrentLevel()
    {
        LevelData levelData = _levelManager.levelDatas[_levelManager.currentLevelIndex];
        _groundTiles = new GroundTile[levelData.levelHeight, levelData.levelHeight];

        GroundTile[] allTiles = Object.FindObjectsByType<GroundTile>(FindObjectsSortMode.None);
        float currentLevelY = _levelManager.currentLevelIndex * _levelManager.verticalSpacing;

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
                    tile.ClearOccupant();
                }
            }
        }

        Block[] allBlocks = Object.FindObjectsByType<Block>(FindObjectsSortMode.None);
        foreach (Block block in allBlocks)
        {
            if (block.levelIndex == _levelManager.currentLevelIndex && !block._isInHole)
            {
                GroundTile tile = GetTileAt(block.gridPosition);
                if (tile != null)
                {
                    tile.SetOccupant(block);
                }
            }
        }
    }

    public bool CanPlaceBlockAt(Vector2Int position)
    {
        if (CheckOutOfBounds(position))
            return false;

        GroundTile tile = GetTileAt(position);
        if (tile == null)
            return false;

        return !tile.isOccupied;
    }


    public void PlaceExistingBlock(Vector2Int position, Block block)
    {
        if (block == null) return;

        if (CheckOutOfBounds(position))
            return;

        GroundTile tile = GetTileAt(position);
        if (tile == null || tile.isOccupied)
            return;

        float yOffset = _levelManager.currentLevelIndex * _levelManager.verticalSpacing;
        block.transform.position = new Vector3(position.x, yOffset + 1f, position.y);

        block.levelIndex = _levelManager.currentLevelIndex;
        block.gridPosition = position;

        tile.SetOccupant(block);
    }

    public void SpawnPlayer()
    {
        if (_levelManager.playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned");
            return;
        }

        float yOffset = _levelManager.currentLevelIndex * _levelManager.verticalSpacing;
        Vector3 spawnPosition = new Vector3(
            _levelManager._currentLevelData.playerSpawn.x,
            yOffset + 1f,
            _levelManager._currentLevelData.playerSpawn.y
        );

        _levelManager._playerInstance = Object.Instantiate(_levelManager.playerPrefab, spawnPosition, Quaternion.identity);
        _levelManager._playerScript = _levelManager._playerInstance.GetComponent<Player>();

        //InventoryUI.Instance.Assignment(_levelManager._playerScript);

        Debug.Log("meow 3");
    }

    public bool CheckOutOfBounds(Vector2Int position)
    {
        if (position.x < 0 || position.x >= _levelManager._currentLevelData.levelHeight ||
            position.y < 0 || position.y >= _levelManager._currentLevelData.levelHeight)
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
        return _levelManager._currentLevelData;
    }

    public bool TryGetLevelObjects(int levelIndex, out LevelObjects levelObjects)
    {
        return _loadedLevels.TryGetValue(levelIndex, out levelObjects);
    }

    public IEnumerable<int> GetLoadedLevelIndices()
    {
        return _loadedLevels.Keys;
    }

    public void RegisterBlockInstance(int levelIndex, GameObject blockObj)
    {
        if (_loadedLevels.TryGetValue(levelIndex, out var levelObjects))
        {
            levelObjects.blocks.Add(blockObj);
        }
    }

    public void UnregisterBlockInstance(int levelIndex, GameObject blockObj)
    {
        if (blockObj == null) return;

        if (_loadedLevels.TryGetValue(levelIndex, out var levelObjects) && levelObjects.blocks != null)
        {
            levelObjects.blocks.Remove(blockObj);
        }
    }

    public void RemoveBlock(Block block)
    {
        if (block == null) return;

        GroundTile tile = GetTileAt(block.gridPosition);
        if (tile != null)
        {
            tile.ClearOccupant();
        }

        UnregisterBlockInstance(block.levelIndex, block.gameObject);
        Object.Destroy(block.gameObject);
    }

    public void ClearLoadedLevels()
    {
        _loadedLevels.Clear();
    }
}
