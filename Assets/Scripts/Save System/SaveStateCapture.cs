using System.Collections.Generic;
using UnityEngine;

public class SaveStateCapture
{
    public GameSaveData CaptureGameState(LevelManager levelManager)
    {
        GameSaveData data = new GameSaveData();

        data.currentLevelIndex = levelManager.currentLevelIndex;
        data.playerPosition = CapturePlayerPosition(levelManager);

        CaptureInventory(levelManager, data);
        CaptureBlocks(data);
        CaptureElevators(levelManager, data);
        CaptureLoadedLevels(levelManager, data);

        return data;
    }

    private static Vector2IntSerializable CapturePlayerPosition(LevelManager levelManager)
    {
        if (levelManager._playerInstance == null)
            return Vector2Int.zero;

        Player player = levelManager._playerInstance.GetComponent<Player>();
        return player != null ? player.gridPosition : Vector2Int.zero;
    }

    private static void CaptureInventory(LevelManager levelManager, GameSaveData data)
    {
        InventoryManager inv = null;
        if (levelManager._playerInstance != null)
            inv = levelManager._playerInstance.GetComponent<InventoryManager>();

        if (inv != null && inv.HasItem)
        {
            var src = inv.HeldRuntime != null ? inv.HeldRuntime : inv.HeldAsset;

            data.inventory.hasItem = true;

            string assetName = inv.HeldAsset != null ? inv.HeldAsset.name : "";
            string fallbackName = src != null ? src.name : "";
            data.inventory.heldBlockDataName = BlockDataUtils.NormalizeBlockDataName(!string.IsNullOrEmpty(assetName) ? assetName : fallbackName);

            data.inventory.blockColor = src != null ? src.blockColor : BlockData.BlockColor.White;
            data.inventory.containedColors = src != null && src.containedColors != null
                ? new List<BlockData.BlockColor>(src.containedColors)
                : new List<BlockData.BlockColor>();

            return;
        }

        if (inv == null)
            Debug.LogWarning("[SaveManager] Player InventoryManager not found; inventory will not be saved.");

        data.inventory.hasItem = false;
        data.inventory.heldBlockDataName = "";
        data.inventory.containedColors = new List<BlockData.BlockColor>();
        data.inventory.blockColor = BlockData.BlockColor.White;
    }

    private static void CaptureBlocks(GameSaveData data)
    {
        Block[] allBlocks = Object.FindObjectsByType<Block>(FindObjectsSortMode.None);
        foreach (Block block in allBlocks)
        {
            var sourceData = block.runtimeData != null ? block.runtimeData : block.data;
            if (sourceData == null)
                continue;

            BlockSaveData blockData = new BlockSaveData
            {
                gridPosition = block.gridPosition,
                blockColor = sourceData.blockColor,
                containedColors = new List<BlockData.BlockColor>(sourceData.containedColors ?? new List<BlockData.BlockColor>()),
                levelIndex = block.levelIndex,
                originLevelIndex = block.originLevelIndex,
                isAtOriginLevel = block.isAtOriginLevel,
                isInHole = block._isInHole,
                blockDataName = BlockDataUtils.NormalizeBlockDataName(block.data != null ? block.data.name : (block.runtimeData != null ? block.runtimeData.name : ""))
            };

            data.allBlocks.Add(blockData);
        }
    }

    private static void CaptureElevators(LevelManager levelManager, GameSaveData data)
    {
        foreach (Block elevatorBlock in levelManager.elevators.ElevatorBlocks)
        {
            if (elevatorBlock == null)
                continue;

            Vector2IntSerializable pos = elevatorBlock.gridPosition;

            ElevatorSaveData elevatorData = new ElevatorSaveData
            {
                position = pos,
                originLevelIndex = elevatorBlock.originLevelIndex,
                isAtOriginLevel = elevatorBlock.isAtOriginLevel,
                currentLevelIndex = elevatorBlock.levelIndex
            };

            data.elevators.Add(elevatorData);
        }
    }

    private static void CaptureLoadedLevels(LevelManager levelManager, GameSaveData data)
    {
        foreach (int levelIndex in levelManager.loader.GetLoadedLevelIndices())
            data.loadedLevelIndices.Add(levelIndex);
    }
}
