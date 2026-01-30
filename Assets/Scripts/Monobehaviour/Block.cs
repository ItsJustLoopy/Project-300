using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int blockId = 0;
    public bool canBePlacedInHole = true;
    public BlockData data; // don't mutate this please

    // runtime clone used for all mutations
    [NonSerialized]
    public BlockData runtimeData;

    public int levelIndex = 0;
    public int originLevelIndex = 0;
    public bool isAtOriginLevel = true;

    private GridMover _gridMover;
    public bool _isInHole = false;
    private List<BlockData.BlockColor> _containedPrimaryColors = new List<BlockData.BlockColor>();
    [NonSerialized]
    public bool skipStartInit = false;

    public Vector2Int gridPosition
    {
        get => _gridMover.gridPosition;
        set => _gridMover.gridPosition = value;
    }
    public bool isMoving => _gridMover.isMoving;
    public List<BlockData.BlockColor> containedPrimaryColors => _containedPrimaryColors;

    private bool IsImmovable
    {
        get
        {
            var source = runtimeData != null ? runtimeData : data;
            return source != null && source.isImmovable;
        }
    }

    private void Awake()
    {
        _gridMover = new GridMover(this, transform);
    }

    private void EnsureRuntimeDataExists()
    {
        if (runtimeData == null && data != null)
        {
            runtimeData = Instantiate(data);
        }
    }

    public void Start()
    {
        if (skipStartInit)
        {
            return;
        }
        EnsureRuntimeDataExists();

        if (runtimeData != null)
        {
            _gridMover.gridPosition = new Vector2Int((int)runtimeData.BlockPosition.x, (int)runtimeData.BlockPosition.z);
        }
        else if (data != null)
        {
            _gridMover.gridPosition = new Vector2Int((int)data.BlockPosition.x, (int)data.BlockPosition.z);
        }

        if (levelIndex == LevelManager.Instance.currentLevelIndex)
        {
            var tile = LevelManager.Instance.GetTileAt(gridPosition);
            if (tile != null)
            {
                tile.occupant = this;
                tile.isOccupied = true;
            }
        }

        originLevelIndex = levelIndex;

        // initialize contained colors from runtimeData
        var sourceColors = (runtimeData != null && runtimeData.containedColors != null && runtimeData.containedColors.Count > 0)
            ? runtimeData.containedColors
            : data?.containedColors;

        if (sourceColors != null && sourceColors.Count > 0)
        {
            _containedPrimaryColors = new List<BlockData.BlockColor>(sourceColors);
        }
        else
        {
            var color = runtimeData != null ? runtimeData.blockColor : data.blockColor;
            if (IsPrimaryColor(color))
            {
                _containedPrimaryColors.Add(color);
            }
        }

        if (IsImmovable)
        {
            _containedPrimaryColors.Clear();
            if (runtimeData != null)
            {
                runtimeData.blockColor = BlockData.BlockColor.White;
                runtimeData.containedColors = new List<BlockData.BlockColor>();
            }
        }
        else
        {
            // sync runtimeData with internal state
            if (runtimeData != null)
            {
                runtimeData.containedColors = new List<BlockData.BlockColor>(_containedPrimaryColors);
                runtimeData.blockColor = DetermineColorFromPrimaries(_containedPrimaryColors);
            }
        }

        ApplyRuntimeData();
    }

    public bool IsPrimaryColor(BlockData.BlockColor color)
    {
        return color == BlockData.BlockColor.Red ||
               color == BlockData.BlockColor.Yellow ||
               color == BlockData.BlockColor.Blue;
    }

    public void PushTo(Vector2Int targetPos)
    {
        if (IsImmovable)
        {
            return;
        }

        float levelY = levelIndex * LevelManager.Instance.verticalSpacing + 1f;
        _gridMover.MoveToGrid(targetPos, levelY);
    }

    public void PushToThenPlaceInHole(Vector2Int targetPos)
    {
        if (IsImmovable)
        {
            return;
        }
        StartCoroutine(MoveToHole(targetPos));
    }

    private IEnumerator MoveToHole(Vector2Int targetPos)
    {
        Vector3 startPosition = transform.position;
        float myLevelY = levelIndex * LevelManager.Instance.verticalSpacing;
        Vector3 holePosition = new Vector3(targetPos.x, myLevelY + 1f, targetPos.y);

        yield return StartCoroutine(_gridMover.MoveWithCustomAnimation(startPosition, holePosition, 0.1f, recordSnapshot: false));

        _gridMover.gridPosition = targetPos;
        yield return StartCoroutine(PlaceDownInHole());

        _isInHole = true;
        ApplyRuntimeData();
    }

    private IEnumerator PlaceDownInHole()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y - 1, startPosition.z);

        yield return StartCoroutine(_gridMover.MoveWithCustomAnimation(startPosition, targetPosition, 0.1f, recordSnapshot: false));
    }

    public void SetInHole(bool inHole)
    {
        _isInHole = inHole;
        ApplyRuntimeData();
    }

    public int GetTargetLevel()
    {
        int target = isAtOriginLevel ? levelIndex + 1 : levelIndex - 1;
        return target;
    }

    public bool CanCombineWith(Block otherBlock)
    {
        if (IsImmovable)
        {
            return false;
        }
        if (otherBlock != null && otherBlock.IsImmovable)
        {
            return false;
        }
        return !_isInHole && !otherBlock._isInHole;
    }

    public void CombineWith(Block otherBlock)
    {
        if (IsImmovable)
        {
            return;
        }
        if (otherBlock != null && otherBlock.IsImmovable)
        {
            return;
        }

        // ensure runtime clones exist
        EnsureRuntimeDataExists();
        otherBlock.EnsureRuntimeDataExists();

        foreach (var color in otherBlock._containedPrimaryColors)
        {
            if (!_containedPrimaryColors.Contains(color))
            {
                _containedPrimaryColors.Add(color);
            }
        }

        // update runtime data
        if (runtimeData != null)
        {
            runtimeData.blockColor = DetermineColorFromPrimaries(_containedPrimaryColors);
            runtimeData.containedColors = new List<BlockData.BlockColor>(_containedPrimaryColors);
        }

        UpdateBlockAppearance();
        UpdateElevatorStatus();
		Destroy(otherBlock.gameObject);

    }

    private BlockData.BlockColor DetermineColorFromPrimaries(List<BlockData.BlockColor> primaries)
    {
        bool hasRed = primaries.Contains(BlockData.BlockColor.Red);
        bool hasYellow = primaries.Contains(BlockData.BlockColor.Yellow);
        bool hasBlue = primaries.Contains(BlockData.BlockColor.Blue);

        if (hasRed && hasYellow && hasBlue)
            return BlockData.BlockColor.Black;
        if (hasRed && hasBlue)
            return BlockData.BlockColor.Purple;
        if (hasRed && hasYellow)
            return BlockData.BlockColor.Orange;
        if (hasYellow && hasBlue)
            return BlockData.BlockColor.Green;

        if (hasRed) return BlockData.BlockColor.Red;
        if (hasYellow) return BlockData.BlockColor.Yellow;
        if (hasBlue) return BlockData.BlockColor.Blue;

        return BlockData.BlockColor.White;
    }

    private void UpdateBlockAppearance()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (IsImmovable)
            {
                renderer.material.color = Color.white;
                return;
            }

            var colorSource = runtimeData != null ? runtimeData.blockColor : data.blockColor;
            Color visualColor = _isInHole ? Color.lightSlateGray : GetColorFromBlockColor(colorSource);
            renderer.material.color = visualColor;
        }
    }

    private Color GetColorFromBlockColor(BlockData.BlockColor blockColor)
    {
        switch (blockColor)
        {
            case BlockData.BlockColor.White: return Color.white;
            case BlockData.BlockColor.Red: return Color.red;
            case BlockData.BlockColor.Yellow: return Color.yellow;
            case BlockData.BlockColor.Blue: return Color.blue;
            case BlockData.BlockColor.Purple: return Color.blueViolet;
            case BlockData.BlockColor.Orange: return Color.orange;
            case BlockData.BlockColor.Green: return Color.green;
            case BlockData.BlockColor.Black: return Color.black;
            default: return Color.white;
        }
    }

    private void UpdateElevatorStatus()
    {
        if (IsImmovable)
        {
            canBePlacedInHole = false;
            return;
        }

        var colorSource = runtimeData != null ? runtimeData.blockColor : data.blockColor;
        canBePlacedInHole = (colorSource == BlockData.BlockColor.Black);
    }

    // helper to apply runtime data after load/restore
    public void ApplyRuntimeData()
    {
        EnsureRuntimeDataExists();

        if (IsImmovable)
        {
            _containedPrimaryColors.Clear();
            if (runtimeData != null)
            {
                runtimeData.blockColor = BlockData.BlockColor.White;
                runtimeData.containedColors = new List<BlockData.BlockColor>();
            }
            UpdateBlockAppearance();
            UpdateElevatorStatus();
            return;
        }

        // ensure internal lists reflect runtimeData
        if (runtimeData != null)
        {
            if (runtimeData.containedColors != null)
                _containedPrimaryColors = new List<BlockData.BlockColor>(runtimeData.containedColors);
        }
        SyncRuntimeFromPrimaries();
        UpdateBlockAppearance();
        UpdateElevatorStatus();
    }

    private void SyncRuntimeFromPrimaries()
    {
        if (runtimeData == null)
        {
            return;
        }
        runtimeData.containedColors = new List<BlockData.BlockColor>(_containedPrimaryColors);
        runtimeData.blockColor = DetermineColorFromPrimaries(_containedPrimaryColors);
    }

    public void AddPrimaryColor(BlockData.BlockColor color)
    {
        if (IsImmovable)
        {
            return;
        }

        if (!IsPrimaryColor(color))
        {
            return;
        }
        if (!_containedPrimaryColors.Contains(color))
        {
            _containedPrimaryColors.Add(color);
        }
        SyncRuntimeFromPrimaries();
        ApplyRuntimeData();
    }

    public void RemovePrimaryColor(BlockData.BlockColor color)
    {
        if (IsImmovable)
        {
            return;
        }

        if (!IsPrimaryColor(color))
        {
            return;
        }
        _containedPrimaryColors.Remove(color);
        SyncRuntimeFromPrimaries();
        ApplyRuntimeData();
    }
}
