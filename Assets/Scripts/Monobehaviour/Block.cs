using System;
using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    public bool canBePlacedInHole = true;
    public Vector2Int gridPosition;
    public BlockData data;
    public int levelIndex = 0; 
    public int originLevelIndex = 0; 
    public bool isAtOriginLevel = true; 

    private bool _isInHole = false;

    public void Start()
    {
        gridPosition = new Vector2Int((int)data.BlockPosition.x, (int)data.BlockPosition.z);
    
        
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
        StartCoroutine(Move(targetPos));
    }

    private IEnumerator Move(Vector2Int targetPos)
    {
        Vector3 startPosition = transform.position;
        float levelY = levelIndex * LevelManager.Instance.verticalSpacing + 1f;
        Vector3 targetPosition = new Vector3(targetPos.x, levelY, targetPos.y);
        float duration = 0.1f;
        float elapsed = 0f;
    
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
    
        transform.position = targetPosition;
        gridPosition = targetPos;
    }

    private IEnumerator MoveToHole(Vector2Int targetPos)
    {
        
        Vector3 startPosition = transform.position;
        Vector3 holePosition = new Vector3(targetPos.x, 1, targetPos.y);
        float duration = 0.1f;
        float elapsed = 0f;
    
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPosition, holePosition, t);
            yield return null;
        }
    
        transform.position = holePosition;
        gridPosition = targetPos;
    
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
    
        float duration = 0.1f;
        float elapsed = 0f;
    
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
    
        transform.position = targetPosition;
    }
    
    public void PushToThenPlaceInHole(Vector2Int targetPos)
    {
        StartCoroutine(MoveToHole(targetPos));
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
