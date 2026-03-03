using System;
using UnityEngine;

public class GridMover
{
    private readonly Transform _transform;

    private bool _hasActiveMove;
    private Vector3 _moveStartPosition;
    private Vector3 _moveTargetPosition;
    private float _moveDuration;
    private float _moveElapsed;
    private bool _applyGridPositionOnComplete;
    private Vector2Int _moveTargetGridPosition;
    private Action _customOnComplete;
    
    public bool isMoving { get; set; }
    public Vector2Int gridPosition { get; set; }
    
    public float moveDuration = 0.1f;
    
    public event Action<Vector2Int> OnMoveComplete;
    
    public GridMover(Transform transform)
    {
        _transform = transform;
    }
    
    public void MoveToGrid(Vector2Int targetGridPos, float yLevel)
    {
        if (!isMoving)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RecordSnapshot();
            }

            Vector3 targetPosition = new Vector3(targetGridPos.x, yLevel, targetGridPos.y);
            BeginMove(
                _transform.position,
                targetPosition,
                moveDuration,
                applyGridPositionOnComplete: true,
                targetGridPos,
                onComplete: null);
        }
    }

    public void Tick(float deltaTime)
    {
        if (!_hasActiveMove)
        {
            return;
        }

        _moveElapsed += deltaTime;

        float t = _moveDuration <= 0f ? 1f : Mathf.Clamp01(_moveElapsed / _moveDuration);
        _transform.position = Vector3.Lerp(_moveStartPosition, _moveTargetPosition, t);

        if (t < 1f)
        {
            return;
        }

        _transform.position = _moveTargetPosition;

        if (_applyGridPositionOnComplete)
        {
            gridPosition = _moveTargetGridPosition;
        }

        Action onComplete = _customOnComplete; 
        bool invokeGridComplete = _applyGridPositionOnComplete;
        Vector2Int completeGridPosition = _moveTargetGridPosition;

        _hasActiveMove = false;
        isMoving = false;

        _customOnComplete = null;
        _applyGridPositionOnComplete = false;

        onComplete?.Invoke();

        if (invokeGridComplete)
        {
            OnMoveComplete?.Invoke(completeGridPosition);
        }
    }
    
    public bool MoveWithCustomAnimation(Vector3 startPos, Vector3 endPos, float duration, Action onComplete = null, bool recordSnapshot = true)
    {
        if (isMoving)
        {
            return false;
        }

        if (recordSnapshot && LevelManager.Instance != null)
        {
            LevelManager.Instance.RecordSnapshot();
        }

        BeginMove(
            startPos,
            endPos,
            duration,
            applyGridPositionOnComplete: false,
            targetGridPos: gridPosition,
            onComplete);

        return true;
    }

    private void BeginMove(
        Vector3 startPos,
        Vector3 endPos,
        float duration,
        bool applyGridPositionOnComplete,
        Vector2Int targetGridPos,
        Action onComplete)
    {
        _moveStartPosition = startPos;
        _moveTargetPosition = endPos;
        _moveDuration = Mathf.Max(0f, duration);
        _moveElapsed = 0f;
        _applyGridPositionOnComplete = applyGridPositionOnComplete;
        _moveTargetGridPosition = targetGridPos;
        _customOnComplete = onComplete;

        _transform.position = startPos;

        _hasActiveMove = true;
        isMoving = true;
    }
}
