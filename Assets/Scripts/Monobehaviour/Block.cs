
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public bool canBePlacedInHole = true;
    public BlockData data;
    public int levelIndex = 0; 
    public int originLevelIndex = 0; 
    public bool isAtOriginLevel = true; 

    private GridMover _gridMover;
    private bool _isInHole = false;
    private List<BlockData.BlockColor> _containedPrimaryColors = new List<BlockData.BlockColor>();

    public Vector2Int gridPosition => _gridMover.gridPosition;
    public bool isMoving => _gridMover.isMoving;
    public List<BlockData.BlockColor> containedPrimaryColors => _containedPrimaryColors; 

    private void Awake()
    {
        _gridMover = new GridMover(this, transform);
    }

    public void Start()
    {
        _gridMover.gridPosition = new Vector2Int((int)data.BlockPosition.x, (int)data.BlockPosition.z);
    
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
        
        
        if (data.containedColors != null && data.containedColors.Count > 0)
        {
            _containedPrimaryColors = new List<BlockData.BlockColor>(data.containedColors);
        }
        else if (IsPrimaryColor(data.blockColor))
        {
            _containedPrimaryColors.Add(data.blockColor);
        }
        
        UpdateBlockAppearance();
        UpdateElevatorStatus();
    }

    private bool IsPrimaryColor(BlockData.BlockColor color)
    {
        return color == BlockData.BlockColor.Red || 
               color == BlockData.BlockColor.Yellow || 
               color == BlockData.BlockColor.Blue;
    }

    public void PushTo(Vector2Int targetPos)
    {
        float levelY = levelIndex * LevelManager.Instance.verticalSpacing + 1f;
        _gridMover.MoveToGrid(targetPos, levelY);
    }

    public void PushToThenPlaceInHole(Vector2Int targetPos)
    {
        StartCoroutine(MoveToHole(targetPos));
    }

    private IEnumerator MoveToHole(Vector2Int targetPos)
    {
        Vector3 startPosition = transform.position;
        float myLevelY = levelIndex * LevelManager.Instance.verticalSpacing;
        Vector3 holePosition = new Vector3(targetPos.x, myLevelY + 1f, targetPos.y);
        
        yield return StartCoroutine(_gridMover.MoveWithCustomAnimation(startPosition, holePosition, 0.1f));
        
        _gridMover.gridPosition = targetPos;
        yield return StartCoroutine(PlaceDownInHole());
    
        _isInHole = true;
        UpdateBlockAppearance();
    }
    
    private IEnumerator PlaceDownInHole()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, startPosition.y - 1, startPosition.z);
        
        yield return StartCoroutine(_gridMover.MoveWithCustomAnimation(startPosition, targetPosition, 0.1f));
    }

    public bool IsInHole()
    {
        return _isInHole;
    }
    
    public int GetTargetLevel()
    {
        if (isAtOriginLevel)
        {
            return originLevelIndex + 1; 
        }
        return originLevelIndex; 
    }
    
    public bool CanCombineWith(Block otherBlock)
    {
        return !_isInHole && !otherBlock.IsInHole();
    }
    
    public void CombineWith(Block otherBlock)
    {
        
        foreach (var color in otherBlock._containedPrimaryColors)
        {
            if (!_containedPrimaryColors.Contains(color))
            {
                _containedPrimaryColors.Add(color);
            }
        }
        
        
        data.blockColor = DetermineColorFromPrimaries(_containedPrimaryColors);
        data.containedColors = new List<BlockData.BlockColor>(_containedPrimaryColors);
        
        UpdateBlockAppearance();
        UpdateElevatorStatus();
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBlockCombineSound();
        }
        
        var tile = LevelManager.Instance.GetTileAt(otherBlock.gridPosition);
        if (tile != null)
        {
            tile.occupant = null;
            tile.isOccupied = false;
        }
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
        
        return BlockData.BlockColor.Red;
    }
    
    private void UpdateBlockAppearance()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color visualColor = _isInHole ? Color.lightSlateGray : GetColorFromBlockColor(data.blockColor);
            renderer.material.color = visualColor;
        }
    }
    
    private Color GetColorFromBlockColor(BlockData.BlockColor blockColor)
    {
        switch (blockColor)
        {
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
        canBePlacedInHole = (data.blockColor == BlockData.BlockColor.Black);
    }
}