using System.Collections.Generic;
using UnityEngine;

public class SaveStateRestore
{
    public void RestoreGameState(LevelManager levelManager, GameSaveData saveData)
    {
        ClearCurrentGameState(levelManager);

        levelManager.currentLevelIndex = saveData.currentLevelIndex;
        levelManager._currentLevelData = levelManager.levelDatas[saveData.currentLevelIndex];

        levelManager.GenerateLevel(saveData.currentLevelIndex, skipBlocks: true);

        if (saveData.currentLevelIndex > 0 && saveData.loadedLevelIndices.Contains(saveData.currentLevelIndex - 1))
            levelManager.GenerateLevel(saveData.currentLevelIndex - 1, skipBlocks: true);

        if (saveData.currentLevelIndex + 1 < levelManager.levelDatas.Length && saveData.loadedLevelIndices.Contains(saveData.currentLevelIndex + 1))
            levelManager.GenerateLevel(saveData.currentLevelIndex + 1, skipBlocks: true);

        RestoreBlocks(levelManager, saveData.allBlocks);
        RestorePlayer(levelManager, saveData.playerPosition);
        RestoreElevators(levelManager, saveData.elevators);
        RestoreInventory(levelManager, saveData.inventory);

        levelManager.visuals.PositionCameraForLevel(saveData.currentLevelIndex);
        levelManager.UpdateLevelOpacities();
        levelManager.UpdateGroundTilesForCurrentLevel();
    }

    private static void RestoreInventory(LevelManager levelManager, InventorySaveData invData)
    {
        if (invData == null || !invData.hasItem)
            return;

        if (levelManager == null || levelManager._playerInstance == null)
        {
            Debug.LogWarning("[SaveManager] Cannot restore inventory: player instance not spawned yet.");
            return;
        }

        InventoryManager inventory = levelManager._playerInstance.GetComponent<InventoryManager>();
        if (inventory == null)
        {
            Debug.LogWarning("[SaveManager] Cannot restore inventory: InventoryManager component missing on player prefab.");
            return;
        }

        string normalized = BlockDataUtils.NormalizeBlockDataName(invData.heldBlockDataName);
        BlockData asset = BlockDataUtils.FindBlockDataByName(levelManager, normalized);
        if (asset == null)
        {
            Debug.LogWarning($"[SaveManager] Could not restore inventory: BlockData not found: '{normalized}' (saved as '{invData.heldBlockDataName}')");
            return;
        }

        BlockData runtime = Object.Instantiate(asset);
        runtime.blockColor = invData.blockColor;
        runtime.containedColors = invData.containedColors != null
            ? new List<BlockData.BlockColor>(invData.containedColors)
            : new List<BlockData.BlockColor>();

        inventory.SetHeldFromSave(asset, runtime);

        Player player = levelManager._playerInstance.GetComponent<Player>();
        if (player != null)
            player.inventory = inventory;
    }

    private static void ClearCurrentGameState(LevelManager levelManager)
    {
        Block[] existingBlocks = Object.FindObjectsByType<Block>(FindObjectsSortMode.None);
        foreach (Block block in existingBlocks)
            Object.Destroy(block.gameObject);

        if (levelManager._playerInstance != null)
            Object.Destroy(levelManager._playerInstance);

        foreach (var kvp in levelManager.loader.LoadedLevels)
        {
            foreach (GameObject tile in kvp.Value.tiles)
            {
                if (tile != null)
                    Object.Destroy(tile);
            }

            foreach (GameObject block in kvp.Value.blocks)
            {
                if (block != null)
                    Object.Destroy(block);
            }
        }

        levelManager.loader.ClearLoadedLevels();
        levelManager.elevators.ClearElevators();
    }

    private static void RestoreBlocks(LevelManager levelManager, List<BlockSaveData> blockSaveDataList)
    {
        foreach (BlockSaveData savedBlock in blockSaveDataList)
        {
            BlockData originalBlockData = BlockDataUtils.FindBlockDataByName(levelManager, savedBlock.blockDataName);
            if (originalBlockData == null)
            {
                Debug.LogWarning($"Could not find BlockData with name: {savedBlock.blockDataName}");
                continue;
            }

            float yOffset = savedBlock.levelIndex * levelManager.verticalSpacing;
            float yPos = savedBlock.isInHole ? yOffset : yOffset + 1f;
            Vector3 blockPosition = new Vector3(savedBlock.gridPosition.x, yPos, savedBlock.gridPosition.y);

            GameObject blockObj = Object.Instantiate(levelManager.blockPrefab, blockPosition, Quaternion.identity);
            Block block = blockObj.GetComponent<Block>();
            if (block == null)
            {
                Object.Destroy(blockObj);
                continue;
            }

            block.data = originalBlockData;
            block.runtimeData = Object.Instantiate(originalBlockData);
            block.runtimeData.BlockPosition = new Vector3(savedBlock.gridPosition.x, yPos, savedBlock.gridPosition.y);
            block.runtimeData.blockColor = savedBlock.blockColor;
            block.runtimeData.containedColors = new List<BlockData.BlockColor>(savedBlock.containedColors ?? new List<BlockData.BlockColor>());

            block.levelIndex = savedBlock.levelIndex;
            block.originLevelIndex = savedBlock.originLevelIndex;
            block.isAtOriginLevel = savedBlock.isAtOriginLevel;

            levelManager.loader.RegisterBlockInstance(savedBlock.levelIndex, blockObj);
            block.SetInHole(savedBlock.isInHole);

            if (savedBlock.isInHole)
            {
                Vector2Int pos = new Vector2Int(savedBlock.gridPosition.x, savedBlock.gridPosition.y);
                levelManager.elevators.SetElevatorAt(pos, block);
            }

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

    private static void RestorePlayer(LevelManager levelManager, Vector2IntSerializable playerPosition)
    {
        float yOffset = levelManager.currentLevelIndex * levelManager.verticalSpacing;
        Vector3 spawnPosition = new Vector3(playerPosition.x, yOffset + 1f, playerPosition.y);

        levelManager._playerInstance = Object.Instantiate(levelManager.playerPrefab, spawnPosition, Quaternion.identity);
        Player player = levelManager._playerInstance.GetComponent<Player>();
        if (player != null)
        {
            player.gridPosition = playerPosition;
            levelManager._playerScript = player;
        }
    }

    private static void RestoreElevators(LevelManager levelManager, List<ElevatorSaveData> elevatorDataList)
    {
        foreach (ElevatorSaveData elevatorData in elevatorDataList)
        {
            Vector2Int position = elevatorData.position.ToVector2Int();

            if (!levelManager.loader.LoadedLevels.ContainsKey(elevatorData.originLevelIndex))
                levelManager.GenerateLevel(elevatorData.originLevelIndex, skipBlocks: true);

            if (!levelManager.loader.LoadedLevels.ContainsKey(elevatorData.currentLevelIndex))
                levelManager.GenerateLevel(elevatorData.currentLevelIndex, skipBlocks: true);

            Block[] allBlocks = Object.FindObjectsByType<Block>(FindObjectsSortMode.None);
            foreach (Block block in allBlocks)
            {
                if (block.gridPosition == position && block._isInHole)
                {
                    block.levelIndex = elevatorData.currentLevelIndex;
                    block.originLevelIndex = elevatorData.originLevelIndex;
                    block.isAtOriginLevel = elevatorData.isAtOriginLevel;
                    levelManager.elevators.SetElevatorAt(position, block);
                    break;
                }
            }
        }
    }

}
