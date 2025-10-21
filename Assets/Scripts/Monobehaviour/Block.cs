using System;
using System.Collections;
using UnityEngine;

public class Block : MonoBehaviour
{
    public bool canBePlacedInHole = false;
    public Vector2Int gridPosition;
    public BlockData data;


    public void Start()
    {
        gridPosition = new Vector2Int((int)data.BlockPosition.x, (int)data.BlockPosition.z);
        var tile = LevelManager.Instance.GetTileAt(gridPosition);
        if (tile != null)
        {
            tile.occupant = this;
            tile.isOccupied = true;
        }
        
        transform.position = data.BlockPosition;
    }

    public void PushTo(Vector2Int targetPos)
    {
        StartCoroutine(Move(targetPos));
    }

    private IEnumerator Move(Vector2Int targetPos)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(targetPos.x, 1, targetPos.y);
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

    
}
