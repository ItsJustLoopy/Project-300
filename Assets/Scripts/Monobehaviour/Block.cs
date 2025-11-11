using System;
using System.Collections;
using Unity.VisualScripting;
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

    public Vector2Int gridPosition => _gridMover.gridPosition;

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
        //Debug.Log($"my index : {levelIndex}, my level Y : {myLevelY}");
        
        Vector3 holePosition = new Vector3(targetPos.x, myLevelY + 1f, targetPos.y);
        
        yield return StartCoroutine(_gridMover.MoveWithCustomAnimation(startPosition, holePosition, 0.1f));
        
        _gridMover.gridPosition = targetPos;
        
        yield return StartCoroutine(PlaceDownInHole());
    
        _isInHole = true;
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }
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
}
