using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private const string SAVE_FILENAME = "gamesave.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILENAME);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveGame()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("Cannot save: LevelManager not found");
            return;
        }

        GameSaveData saveData = CaptureGameState();
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);

        try
        {
            File.WriteAllText(SavePath, json);
            Debug.Log($"Game saved successfully to: {SavePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    private GameSaveData CaptureGameState()
    {
        GameSaveData data = new GameSaveData();
        LevelManager levelManager = LevelManager.Instance;

        // Level info
        data.currentLevelIndex = levelManager.currentLevelIndex;
        data.playerPosition = levelManager._playerInstance.GetComponent<Player>().gridPosition;

        // All blocks in the scene
        Block[] allBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
        foreach (Block block in allBlocks)
        {
            var sourceData = block.runtimeData != null ? block.runtimeData : block.data;

            BlockSaveData blockData = new BlockSaveData
            {
                gridPosition = block.gridPosition,
                blockColor = sourceData.blockColor,
                containedColors = new List<BlockData.BlockColor>(sourceData.containedColors ?? new List<BlockData.BlockColor>()),
                levelIndex = block.levelIndex,
                originLevelIndex = block.originLevelIndex,
                isAtOriginLevel = block.isAtOriginLevel,
                isInHole = block._isInHole,
                // store original asset name so we can find the asset to instantiate on load
                blockDataName = block.data != null ? block.data.name : (block.runtimeData != null ? block.runtimeData.name : "")
            };
            data.allBlocks.Add(blockData);
        }

        // Elevator states
        foreach (var kvp in levelManager.elevators.ElevatorBlocks)
        {
            Vector2IntSerializable pos = kvp.Key;
            Block elevatorBlock = kvp.Value;

            ElevatorSaveData elevatorData = new ElevatorSaveData
            {
                position = pos,
                originLevelIndex = elevatorBlock.originLevelIndex,
                isAtOriginLevel = elevatorBlock.isAtOriginLevel,
                currentLevelIndex = elevatorBlock.levelIndex
            };

            data.elevators.Add(elevatorData);
        }

        // Which levels are currently loaded
        foreach (int levelIndex in levelManager.loader.GetLoadedLevelIndices())
        {
            data.loadedLevelIndices.Add(levelIndex);
        }

        return data;
    }

    public bool SaveFileExists()
    {
        return File.Exists(SavePath);
    }

    public bool LoadGame()
    {
        if (!SaveFileExists())
        {
            Debug.Log("No save file found - starting new game");
            return false;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            GameSaveData saveData = JsonConvert.DeserializeObject<GameSaveData>(json);

            RestoreGameState(saveData);
            Debug.Log($"Game loaded successfully from: {SavePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return false;
        }
    }

    private void RestoreGameState(GameSaveData saveData)
    {
        LevelManager levelManager = LevelManager.Instance;
        
        ClearCurrentGameState();
        
        levelManager.currentLevelIndex = saveData.currentLevelIndex;
        levelManager._currentLevelData = levelManager.levelDatas[saveData.currentLevelIndex];

        levelManager.GenerateLevel(saveData.currentLevelIndex, skipBlocks: true);
        
        if (saveData.currentLevelIndex > 0 && saveData.loadedLevelIndices.Contains(saveData.currentLevelIndex - 1))
        {
            levelManager.GenerateLevel(saveData.currentLevelIndex - 1, skipBlocks: true);
        }
        if (saveData.currentLevelIndex + 1 < levelManager.levelDatas.Length && 
            saveData.loadedLevelIndices.Contains(saveData.currentLevelIndex + 1))
        {
            levelManager.GenerateLevel(saveData.currentLevelIndex + 1, skipBlocks: true);
        }
        
        RestoreBlocks(saveData.allBlocks);
        RestorePlayer(saveData.playerPosition);
        RestoreElevators(saveData.elevators);
        levelManager.visuals.PositionCameraForLevel(saveData.currentLevelIndex);
        
        levelManager.UpdateLevelOpacities();
        levelManager.UpdateGroundTilesForCurrentLevel();
    }

    private static void ClearCurrentGameState()
    {
        LevelManager levelManager = LevelManager.Instance;

        Block[] existingBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
        foreach (Block block in existingBlocks)
        {
            Destroy(block.gameObject);
        }

        if (levelManager._playerInstance != null)
        {
            Destroy(levelManager._playerInstance);
        }

        foreach (var kvp in levelManager.loader.LoadedLevels)
        {
            foreach (GameObject tile in kvp.Value.tiles)
            {
                if (tile != null) Destroy(tile);
            }
            foreach (GameObject block in kvp.Value.blocks)
            {
                if (block != null) Destroy(block);
            }
        }
        
        levelManager.loader.ClearLoadedLevels();
        levelManager.elevators.ClearElevators();
    }


    public void RestoreBlocks(List<BlockSaveData> blockSaveDataList)
    {
        LevelManager levelManager = LevelManager.Instance;

        foreach (BlockSaveData savedBlock in blockSaveDataList)
        {
            // Find the original BlockData asset by name
            BlockData originalBlockData = FindBlockDataByName(savedBlock.blockDataName);
            if (originalBlockData == null)
            {
                Debug.LogWarning($"Could not find BlockData with name: {savedBlock.blockDataName}");
                continue;
            }

            // Calculate position
            float yOffset = savedBlock.levelIndex * levelManager.verticalSpacing;
            float yPos = savedBlock.isInHole ? yOffset : yOffset + 1f;
            Vector3 blockPosition = new Vector3(
                savedBlock.gridPosition.x,
                yPos,
                savedBlock.gridPosition.y
            );

            // Instantiate block prefab
            GameObject blockObj = Instantiate(levelManager.blockPrefab, blockPosition, Quaternion.identity);
            Block block = blockObj.GetComponent<Block>();

            // Assign asset ref and create runtime clone data
            block.data = originalBlockData;
            block.runtimeData = Instantiate(originalBlockData);

            // Apply saved runtime state
            block.runtimeData.BlockPosition = new Vector3(savedBlock.gridPosition.x, yPos, savedBlock.gridPosition.y);
            block.runtimeData.blockColor = savedBlock.blockColor;
            block.runtimeData.containedColors =
                new List<BlockData.BlockColor>(savedBlock.containedColors ?? new List<BlockData.BlockColor>());

            block.levelIndex = savedBlock.levelIndex;
            block.originLevelIndex = savedBlock.originLevelIndex;
            block.isAtOriginLevel = savedBlock.isAtOriginLevel;

            // Add the instantiated block to the LevelObjects list for that level 
            levelManager.loader.RegisterBlockInstance(savedBlock.levelIndex, blockObj);

            block.SetInHole(savedBlock.isInHole);

            // register elevator 
            if (savedBlock.isInHole)
            {
                Vector2Int pos = new Vector2Int(savedBlock.gridPosition.x, savedBlock.gridPosition.y);
                levelManager.elevators.SetElevatorAt(pos, block);
            }

            // Apply visuals and elevator status from runtimeData
            block.ApplyRuntimeData();

            if (savedBlock.levelIndex == levelManager.currentLevelIndex && !savedBlock.isInHole)
            {
                GroundTile tile = levelManager.GetTileAt(savedBlock.gridPosition);
                if (tile != null)
                {
                    tile.occupant = block;
                    tile.isOccupied = true;
                }
            }
        }
    }


    public void RestorePlayer(Vector2IntSerializable playerPosition)
    {
        LevelManager levelManager = LevelManager.Instance;
        
        float yOffset = levelManager.currentLevelIndex * levelManager.verticalSpacing;
        Vector3 spawnPosition = new Vector3(
            playerPosition.x,
            yOffset + 1f,
            playerPosition.y
        );

        levelManager._playerInstance = Instantiate(levelManager.playerPrefab, spawnPosition, Quaternion.identity);
        Player player = levelManager._playerInstance.GetComponent<Player>();
        player.gridPosition = playerPosition;
        
        levelManager._playerScript = player;
    }

    private void RestoreElevators(List<ElevatorSaveData> elevatorDataList)
    {
        LevelManager levelManager = LevelManager.Instance;

        foreach (ElevatorSaveData elevatorData in elevatorDataList)
        {
            Vector2Int position = elevatorData.position.ToVector2Int();

            if (!levelManager.loader.LoadedLevels.ContainsKey(elevatorData.originLevelIndex))
            {
                levelManager.GenerateLevel(elevatorData.originLevelIndex, skipBlocks: true);
            }
            if (!levelManager.loader.LoadedLevels.ContainsKey(elevatorData.currentLevelIndex))
            {
                levelManager.GenerateLevel(elevatorData.currentLevelIndex, skipBlocks: true);
            }

            Block[] allBlocks = FindObjectsByType<Block>(FindObjectsSortMode.None);
            foreach (Block block in allBlocks)
            {
                if (block.gridPosition == position && block._isInHole)
                {
                    block.levelIndex = elevatorData.currentLevelIndex;
                    block.originLevelIndex = elevatorData.originLevelIndex;
                    block.isAtOriginLevel = elevatorData.isAtOriginLevel;
                    levelManager.elevators.SetElevatorAt(position, block);
                    Debug.Log($"Restored elevator at {position}, origin level: {elevatorData.originLevelIndex}," +
                              $" isAtOrigin: {elevatorData.isAtOriginLevel}, current level: {elevatorData.currentLevelIndex}");
                    break;
                }
            }
        }
    }

    private BlockData FindBlockDataByName(string blockDataName)
    {
        foreach (LevelData levelData in LevelManager.Instance.levelDatas)
        {
            if (levelData.blocks != null)
            {
                foreach (BlockData blockData in levelData.blocks)
                {
                    if (blockData.name == blockDataName)
                    {
                        return blockData;
                    }
                }
            }
        }
        return null;
    }


    public void DeleteSave()
    {
        if (SaveFileExists())
        {
            try
            {
                File.Delete(SavePath);
                Debug.Log("Save file deleted");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to delete save file: {e.Message}");
            }
        }
    }

    public void PrintSavePath()
    {
        Debug.Log($"Save file location: {SavePath}");
    }
    
}
