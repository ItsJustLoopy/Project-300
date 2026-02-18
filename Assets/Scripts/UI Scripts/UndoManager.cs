using System.Collections.Generic;
using UnityEngine;

public class UndoManager
{
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

    private readonly LevelManager _levelManager;
    private readonly List<MoveRecord> _undoStack = new List<MoveRecord>();
    private int _maxUndo = 10;
    private int _nextBlockId = 1;

    public UndoManager(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }

    public void EnsureBlockId(Block block)
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

    public void RecordSnapshot()
    {
        var record = new MoveRecord();

        if (_levelManager._playerInstance != null && _levelManager._playerScript != null)
        {
            record.playerWorldPosition = _levelManager._playerInstance.transform.position;
            record.playerGridPosition = _levelManager._playerScript.gridPosition;
            record.playerLevelIndex = _levelManager.currentLevelIndex;
        }

        Block[] allBlocks = Object.FindObjectsByType<Block>(FindObjectsSortMode.None);
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
                data = block.data != null ? block.data : block.runtimeData,
                blockColor = sourceData != null ? sourceData.blockColor : BlockData.BlockColor.Red,
                containedColors = containedColors,
                worldPosition = block.transform.position,
                gridPosition = block.gridPosition,
                levelIndex = block.levelIndex,
                isInHole = block._isInHole,
                isAtOriginLevel = block.isAtOriginLevel,
                originLevelIndex = block.originLevelIndex,
                wasRegisteredAsElevator = (_levelManager.elevators.ElevatorBlocks.TryGetValue(block.gridPosition, out var b) && b == block),
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
            if (_undoStack[i].playerLevelIndex == _levelManager.currentLevelIndex)
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

        _levelManager.elevators.ClearElevators();
        Block[] existingBlocks = Object.FindObjectsByType<Block>(FindObjectsSortMode.None);
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
                GameObject blockObj = Object.Instantiate(_levelManager.blockPrefab, snap.worldPosition, Quaternion.identity);
                block = blockObj.GetComponent<Block>();
                if (block == null)
                {
                    continue;
                }

                _levelManager.loader.RegisterBlockInstance(snap.levelIndex, blockObj);

                block.skipStartInit = true;
                block.blockId = snap.blockId;
                block.data = snap.data;
                if (snap.data != null)
                {
                    block.runtimeData = Object.Instantiate(snap.data);
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
                block.runtimeData = Object.Instantiate(snap.data);
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
                _levelManager.elevators.SetElevatorAt(snap.gridPosition, block);
            }
        }

        foreach (var block in existingBlocks)
        {
            if (block == null) continue;
            if (!snapshotIds.Contains(block.blockId))
            {
                _levelManager.loader.UnregisterBlockInstance(block.levelIndex, block.gameObject);
                Object.Destroy(block.gameObject);
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

        _levelManager.currentLevelIndex = record.playerLevelIndex;
        _levelManager._currentLevelData = Object.Instantiate(_levelManager.levelDatas[_levelManager.currentLevelIndex]);
        if (_levelManager._playerInstance != null)
        {
            _levelManager._playerInstance.transform.position = record.playerWorldPosition;
        }
        if (_levelManager._playerScript != null)
        {
            _levelManager._playerScript.gridPosition = record.playerGridPosition;
        }

        _levelManager.loader.UpdateGroundTilesForCurrentLevel();
        _levelManager.loader.ManageLoadedLevels(loadMissingLevels: false);
        _levelManager.visuals.UpdateLevelOpacities();

        Debug.Log("Undo performed");
    }
}
