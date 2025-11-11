using System;
using System.Collections;
using UnityEngine;

public class GridMover
{
    private MonoBehaviour _coroutineRunner;
    private Transform _transform;
    
    public bool isMoving { get; set; }
    public Vector2Int gridPosition { get; set; }
    
    public float moveDuration = 0.1f;
    
    public event Action<Vector2Int> OnMoveComplete;
    
    public GridMover(MonoBehaviour coroutineRunner, Transform transform)
    {
        _coroutineRunner = coroutineRunner;
        _transform = transform;
    }
    
    public void MoveToGrid(Vector2Int targetGridPos, float yLevel)
    {
        if (!isMoving)
        {
            _coroutineRunner.StartCoroutine(MoveCoroutine(targetGridPos, yLevel));
        }
    }
    
    private IEnumerator MoveCoroutine(Vector2Int targetGridPos, float yLevel)
    {
        isMoving = true;
        
        Vector3 startPosition = _transform.position;
        Vector3 targetPosition = new Vector3(targetGridPos.x, yLevel, targetGridPos.y);
        float elapsed = 0f;
        
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            _transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        _transform.position = targetPosition;
        gridPosition = targetGridPos;
        isMoving = false;
        
        OnMoveComplete?.Invoke(targetGridPos);
    }
    
    public IEnumerator MoveWithCustomAnimation(Vector3 startPos, Vector3 endPos, float duration, Action onComplete = null)
    {
        isMoving = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        _transform.position = endPos;
        isMoving = false;
        onComplete?.Invoke();
    }
}