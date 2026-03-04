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

    public bool HasMovingBlockOnCurrentLevel()
    {
        if (!_loadedLevels.TryGetValue(_levelManager.currentLevelIndex, out var levelObjects) || levelObjects.blocks == null)
        {
            return false;
        }

        foreach (GameObject blockObj in levelObjects.blocks)
        {
            if (blockObj == null)
            {
                continue;
            }

            Block block = blockObj.GetComponent<Block>();
            if (block != null && block.levelIndex == _levelManager.currentLevelIndex && block.isMoving)
            {
                return true;
            }
        }

        return false;
    }

    private readonly LevelManager _levelManager;
    private GroundTile[,] _groundTiles;
    private readonly Dictionary<int, LevelObjects> _loadedLevels = new Dictionary<int, LevelObjects>();
    private int? _auxiliaryLoadedLevel;

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
        float initialOpacity = levelIndex == _levelManager.currentLevelIndex ? 1f : _levelManager.inactiveLevelOpacity;
        levelObjects.currentOpacity = initialOpacity;
        levelObjects.targetOpacity = initialOpacity;

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

        if (_levelManager.holeIndicatorPrefab != null)
        {
            Vector3 holeIndicatorPosition = new Vector3(
                levelData.holePosition.x,
                yOffset + _levelManager.arrowYOffset,
                levelData.holePosition.y
            );

            GameObject holeIndicatorObj = Object.Instantiate(_levelManager.holeIndicatorPrefab, holeIndicatorPosition, Quaternion.identity);
            holeIndicatorObj.transform.SetParent(tilesParent.transform);
            levelObjects.tiles.Add(holeIndicatorObj);
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

                    if (blockData.isImmovable)
                    {
                        GroundTile tile = FindTileInLevel(levelObjects, new Vector2Int(
                            Mathf.RoundToInt(blockData.BlockPosition.x),
                            Mathf.RoundToInt(blockData.BlockPosition.z)));

                        if (tile != null)
                        {
                            tile.SetOccupant(blockComponent);
                        }
                    }
                }

                levelObjects.blocks.Add(blockObj);
            }

            RemoveTileChildrenForWalls(levelObjects);
        }

        _loadedLevels[levelIndex] = levelObjects;
        _levelManager.visuals.SetLevelOpacity(levelIndex, initialOpacity);
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

    private void UnloadLevel(int levelIndex, bool destroyAllBlocks = false, bool preserveInHoleBlocks = false)
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
                bool shouldPreserveInHole = preserveInHoleBlocks && blockComponent != null && blockComponent._isInHole;
                if (!shouldPreserveInHole && (destroyAllBlocks || blockComponent == null || !blockComponent._isInHole))
                {
                    Object.Destroy(block);
                }
            }
        }

        levelObjects.blocks.Clear();

        _loadedLevels.Remove(levelIndex);
    }

    public void ResetLevelToInitialState(int levelIndex, bool preserveInHoleBlocks = false)
    {
        if (levelIndex < 0 || levelIndex >= _levelManager.levelDatas.Length)
        {
            return;
        }

        List<GameObject> preservedBlocks = new List<GameObject>();

        if (_loadedLevels.ContainsKey(levelIndex))
        {
            if (preserveInHoleBlocks && _loadedLevels.TryGetValue(levelIndex, out var levelObjects))
            {
                foreach (var blockObj in levelObjects.blocks)
                {
                    if (blockObj == null) continue;

                    Block blockComponent = blockObj.GetComponent<Block>();
                    if (blockComponent != null && blockComponent._isInHole)
                    {
                        preservedBlocks.Add(blockObj);
                    }
                }
            }

            UnloadLevel(levelIndex, destroyAllBlocks: true, preserveInHoleBlocks: preserveInHoleBlocks);
        }

        GenerateLevel(levelIndex);

        foreach (var preservedBlock in preservedBlocks)
        {
            if (preservedBlock == null) continue;

            Block blockComponent = preservedBlock.GetComponent<Block>();
            RegisterBlockInstance(levelIndex, preservedBlock);

            if (blockComponent != null && blockComponent._isInHole)
            {
                SetTileVisible(levelIndex, blockComponent.gridPosition, false);
            }
        }
    }

    public void ManageLoadedLevels(bool loadMissingLevels = true)
    {
        List<int> levelsToKeep = new List<int>();
        levelsToKeep.Add(_levelManager.currentLevelIndex);

        if (_auxiliaryLoadedLevel.HasValue)
        {
            int auxiliaryLevel = _auxiliaryLoadedLevel.Value;
            if (auxiliaryLevel >= 0 &&
                auxiliaryLevel < _levelManager.levelDatas.Length &&
                auxiliaryLevel != _levelManager.currentLevelIndex)
            {
                levelsToKeep.Add(auxiliaryLevel);
            }
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

    public void SetAuxiliaryLoadedLevel(int? levelIndex)
    {
        if (!levelIndex.HasValue)
        {
            _auxiliaryLoadedLevel = null;
            return;
        }

        int value = levelIndex.Value;
        if (value < 0 || value >= _levelManager.levelDatas.Length)
        {
            _auxiliaryLoadedLevel = null;
            return;
        }

        _auxiliaryLoadedLevel = value;
    }

    public void UpdateGroundTilesForCurrentLevel()
    {
        LevelData levelData = _levelManager.levelDatas[_levelManager.currentLevelIndex];
        _groundTiles = new GroundTile[levelData.levelHeight, levelData.levelHeight];

        if (!_loadedLevels.TryGetValue(_levelManager.currentLevelIndex, out var currentLevelObjects))
        {
            return;
        }

        foreach (GameObject tileObj in currentLevelObjects.tiles)
        {
            if (tileObj == null)
            {
                continue;
            }

            GroundTile tile = tileObj.GetComponent<GroundTile>();
            if (tile == null)
            {
                continue;
            }

            Vector2Int gridPos = tile.gridPosition;
            if (gridPos.x >= 0 && gridPos.x < levelData.levelHeight &&
                gridPos.y >= 0 && gridPos.y < levelData.levelHeight)
            {
                _groundTiles[gridPos.x, gridPos.y] = tile;
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

        foreach (GameObject blockObj in currentLevelObjects.blocks)
        {
            if (blockObj == null)
            {
                continue;
            }

            Block block = blockObj.GetComponent<Block>();
            if (block != null && block.levelIndex == _levelManager.currentLevelIndex && !block._isInHole)
            {
                GroundTile tile = GetTileAt(block.gridPosition);
                if (tile != null)
                {
                    tile.SetOccupant(block);
                }
            }
        }

        RemoveTileChildrenForWalls(currentLevelObjects);
    }

    private static GroundTile FindTileInLevel(LevelObjects levelObjects, Vector2Int gridPosition)
    {
        if (levelObjects == null || levelObjects.tiles == null)
        {
            return null;
        }

        foreach (GameObject tileObj in levelObjects.tiles)
        {
            if (tileObj == null)
            {
                continue;
            }

            GroundTile tile = tileObj.GetComponent<GroundTile>();
            if (tile != null && tile.gridPosition == gridPosition)
            {
                return tile;
            }
        }

        return null;
    }

    private static void RemoveTileChildrenForWalls(LevelObjects levelObjects)
    {
        if (levelObjects == null || levelObjects.tiles == null)
        {
            return;
        }

        foreach (GameObject tileObj in levelObjects.tiles)
        {
            if (tileObj == null)
            {
                continue;
            }

            GroundTile tile = tileObj.GetComponent<GroundTile>();
            if (tile == null || tile.occupant == null)
            {
                continue;
            }

            BlockData source = tile.occupant.runtimeData != null ? tile.occupant.runtimeData : tile.occupant.data;
            bool isWall = source != null && source.isImmovable;
            if (!isWall)
            {
                continue;
            }

            for (int i = tileObj.transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(tileObj.transform.GetChild(i).gameObject);
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

    public Block GetBlockingBlockAt(Vector2Int position)
    {
        if (CheckOutOfBounds(position))
        {
            return null;
        }

        GroundTile tile = _groundTiles[position.x, position.y];
        if (tile != null && tile.isOccupied && tile.occupant != null)
        {
            return tile.occupant;
        }

        if (!_loadedLevels.TryGetValue(_levelManager.currentLevelIndex, out var levelObjects))
        {
            return null;
        }

        foreach (GameObject blockObj in levelObjects.blocks)
        {
            if (blockObj == null)
            {
                continue;
            }

            Block block = blockObj.GetComponent<Block>();
            if (block == null)
            {
                continue;
            }

            if (block.levelIndex == _levelManager.currentLevelIndex && !block._isInHole && block.gridPosition == position)
            {
                return block;
            }
        }

        return null;
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

    public void SetTileVisible(int levelIndex, Vector2Int position, bool isVisible)
    {
        if (!_loadedLevels.TryGetValue(levelIndex, out var levelObjects))
        {
            return;
        }

        if (isVisible)
        {
            if (levelObjects.hiddenTiles.TryGetValue(position, out var hiddenTile) && hiddenTile != null)
            {
                SetObjectRenderersEnabled(hiddenTile, true);
            }

            levelObjects.hiddenTiles.Remove(position);
            return;
        }

        if (levelObjects.hiddenTiles.ContainsKey(position))
        {
            return;
        }

        foreach (var tileObj in levelObjects.tiles)
        {
            if (tileObj == null) continue;

            GroundTile tile = tileObj.GetComponent<GroundTile>();
            if (tile == null || tile.gridPosition != position) continue;

            SetObjectRenderersEnabled(tileObj, false);
            levelObjects.hiddenTiles[position] = tileObj;
            return;
        }
    }

    private static void SetObjectRenderersEnabled(GameObject obj, bool isEnabled)
    {
        if (obj == null)
        {
            return;
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isEnabled;
        }
    }

    public void ClearLoadedLevels()
    {
        _loadedLevels.Clear();
        _auxiliaryLoadedLevel = null;
    }
}
