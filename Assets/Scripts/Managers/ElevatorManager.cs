using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorManager
{
    private readonly LevelManager _levelManager;
    private readonly Dictionary<Vector2Int, Block> _elevatorBlocks = new Dictionary<Vector2Int, Block>();
    private int _highestVisitedLevel = -1;

    public IReadOnlyDictionary<Vector2Int, Block> ElevatorBlocks => _elevatorBlocks;
    public int HighestVisitedLevel => _highestVisitedLevel < 0 ? _levelManager.currentLevelIndex : _highestVisitedLevel;

    public ElevatorManager(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }

    public void ClearElevators()
    {
        _elevatorBlocks.Clear();
    }

    public void ClearElevatorsForLevel(int levelIndex)
    {
        var positionsToRemove = new List<Vector2Int>();

        foreach (var kvp in _elevatorBlocks)
        {
            Block block = kvp.Value;
            if (block == null || block.levelIndex == levelIndex || block.originLevelIndex == levelIndex)
            {
                positionsToRemove.Add(kvp.Key);
            }
        }

        foreach (var position in positionsToRemove)
        {
            _elevatorBlocks.Remove(position);
        }
    }

    public void SetElevatorAt(Vector2Int position, Block block)
    {
        if (block != null)
        {
            _elevatorBlocks[position] = block;
        }
    }

    public void RegisterElevator(Vector2Int position, Block block)
    {
        if (!_elevatorBlocks.ContainsKey(position))
        {
            _elevatorBlocks[position] = block;
            block.levelIndex = _levelManager.currentLevelIndex;
            block.originLevelIndex = _levelManager.currentLevelIndex;
            block.isAtOriginLevel = true;
            Debug.Log($"Registered elevator at {position} on level {_levelManager.currentLevelIndex}");
        }
    }

    public bool IsElevatorAt(Vector2Int position)
    {
        if (_elevatorBlocks.TryGetValue(position, out var block))
        {
            return block != null && block._isInHole && block.levelIndex == _levelManager.currentLevelIndex;
        }
        return false;
    }

    public Block GetElevatorAt(Vector2Int position)
    {
        if (_elevatorBlocks.TryGetValue(position, out var block))
        {
            if (block != null && block._isInHole && block.levelIndex == _levelManager.currentLevelIndex)
                return block;
        }
        return null;
    }

    public bool TryGetElevatorPositionOnLevel(int levelIndex, out Vector2Int elevatorPosition)
    {
        foreach (var kvp in _elevatorBlocks)
        {
            Block block = kvp.Value;
            if (block != null && block._isInHole && block.levelIndex == levelIndex)
            {
                elevatorPosition = kvp.Key;
                return true;
            }
        }

        elevatorPosition = Vector2Int.zero;
        return false;
    }

    public IEnumerator UseElevator(Vector2Int elevatorPosition)
    {
        Block elevator = GetElevatorAt(elevatorPosition);
        if (elevator == null)
        {
            Debug.LogError("No elevator found at position");
            yield break;
        }

        _levelManager.undo.RecordSnapshot();

        if (_highestVisitedLevel < 0)
        {
            _highestVisitedLevel = _levelManager.currentLevelIndex;
        }

        int upperLevel = elevator.originLevelIndex + 1;
        int targetLevel = elevator.isAtOriginLevel ? upperLevel : elevator.originLevelIndex;

        if (targetLevel < _levelManager.currentLevelIndex && _levelManager.currentLevelIndex < _highestVisitedLevel)
        {
            Debug.Log("Cannot travel further down");
            yield break;
        }

        if (targetLevel < 0 || targetLevel >= _levelManager.levelDatas.Length)
        {
            Debug.Log("Cannot travel to that level");
            yield break;
        }

        _levelManager._playerScript._gridMover.isMoving = true;

        float targetLevelY = targetLevel * _levelManager.verticalSpacing;
        float cameraY = targetLevelY + 19f;

        Vector3 playerStart = _levelManager._playerInstance.transform.position;
        Vector3 playerTarget = new Vector3(elevatorPosition.x, targetLevelY + 1f, elevatorPosition.y);

        Vector3 elevatorStart = elevator.transform.position;
        Vector3 elevatorTarget = new Vector3(elevatorPosition.x, targetLevelY, elevatorPosition.y);

        Vector3 cameraStart = _levelManager.mainCamera.transform.position;
        Vector3 cameraTarget = new Vector3(cameraStart.x, cameraY, cameraStart.z);

        float duration = 1.5f;
        float elapsed = 0f;

        int fromLevel = _levelManager.currentLevelIndex;
        int toLevel = targetLevel;

        if (!_levelManager.loader.LoadedLevels.ContainsKey(toLevel))
        {
            _levelManager.loader.GenerateLevel(toLevel);
        }

        SetLevelOpacityAndState(toLevel, _levelManager.inactiveLevelOpacity);
        SetLevelOpacityAndState(fromLevel, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);

            _levelManager._playerInstance.transform.position = Vector3.Lerp(playerStart, playerTarget, smoothT);
            elevator.transform.position = Vector3.Lerp(elevatorStart, elevatorTarget, smoothT);
            _levelManager.mainCamera.transform.position = Vector3.Lerp(cameraStart, cameraTarget, smoothT);

            if (_levelManager.loader.LoadedLevels.ContainsKey(fromLevel))
            {
                float fromOpacity = Mathf.Lerp(1f, _levelManager.inactiveLevelOpacity, t);
                SetLevelOpacityAndState(fromLevel, fromOpacity);
            }

            if (_levelManager.loader.LoadedLevels.ContainsKey(toLevel))
            {
                float toOpacity = Mathf.Lerp(_levelManager.inactiveLevelOpacity, 1f, t);
                SetLevelOpacityAndState(toLevel, toOpacity);
            }

            yield return null;
        }

        if (_levelManager.loader.LoadedLevels.ContainsKey(fromLevel))
        {
            SetLevelOpacityAndState(fromLevel, _levelManager.inactiveLevelOpacity);
        }

        if (_levelManager.loader.LoadedLevels.ContainsKey(toLevel))
        {
            SetLevelOpacityAndState(toLevel, 1f);
        }

        _levelManager._playerInstance.transform.position = playerTarget;
        elevator.transform.position = elevatorTarget;
        _levelManager.mainCamera.transform.position = cameraTarget;

        _levelManager.loader.UnregisterBlockInstance(fromLevel, elevator.gameObject);
        _levelManager.loader.RegisterBlockInstance(targetLevel, elevator.gameObject);
        SetLevelOpacityAndState(targetLevel, 1f);

        elevator.levelIndex = targetLevel;
        elevator.isAtOriginLevel = targetLevel == elevator.originLevelIndex;

        if (targetLevel > _highestVisitedLevel)
        {
            _highestVisitedLevel = targetLevel;
        }

        _levelManager.currentLevelIndex = targetLevel;
        _levelManager._currentLevelData = Object.Instantiate(_levelManager.levelDatas[_levelManager.currentLevelIndex]);
        _levelManager._playerScript.gridPosition = elevatorPosition;

        _levelManager.loader.SetTileVisible(targetLevel, elevatorPosition, false);
        _levelManager.loader.SetAuxiliaryLoadedLevel(fromLevel);

        _levelManager.loader.UpdateGroundTilesForCurrentLevel();
        _levelManager.loader.ManageLoadedLevels();

        _levelManager._playerScript._gridMover.isMoving = false;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RequestSave();
        }
    }

    private void SetLevelOpacityAndState(int levelIndex, float opacity)
    {
        _levelManager.visuals.SetLevelOpacity(levelIndex, opacity);

        if (_levelManager.loader.TryGetLevelObjects(levelIndex, out var levelObjects))
        {
            levelObjects.currentOpacity = opacity;
            levelObjects.targetOpacity = opacity;
        }
    }
}
