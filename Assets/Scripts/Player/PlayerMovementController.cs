using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private PlayerBlockInteraction blockInteraction;

    private Player _player;

    public Vector2Int FacingDirection { get; private set; }

    private void Awake()
    {
        _player = GetComponent<Player>();

        if (blockInteraction == null)
            blockInteraction = GetComponent<PlayerBlockInteraction>();
    }

    public Vector2Int ConvertInputToDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return new Vector2Int((int)Mathf.Sign(input.x), 0);
        }

        if (input.y != 0)
        {
            return new Vector2Int(0, (int)Mathf.Sign(input.y));
        }

        return Vector2Int.zero;
    }

    public void TryMove(Vector2Int direction)
    {
        FacingDirection = direction;
        _player.facingDirection = direction; 

        Vector2Int newPosition = _player.gridPosition + direction;

        bool isBlockInHole = IsBlockInHoleAtPosition(newPosition);

        if (!isBlockInHole && LevelManager.Instance.CheckOutOfBounds(newPosition))
            return;

        if (isBlockInHole)
        {
            Move(newPosition);
            return;
        }

        GroundTile targetTile = LevelManager.Instance.GetTileAt(newPosition);

        if (targetTile != null && targetTile.isOccupied)
        {
            bool shouldMove = blockInteraction != null &&
                              blockInteraction.TryHandleBlockInteraction(targetTile.occupant, direction, newPosition);

            if (shouldMove)
            {
                Move(newPosition);
            }

            return;
        }

        Move(newPosition);
    }

    public void Move(Vector2Int gridPos)
    {
        float levelY = LevelManager.Instance.currentLevelIndex * LevelManager.Instance.verticalSpacing + 1f;
        _player._gridMover.MoveToGrid(gridPos, levelY);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerMoveSound();
        }
    }

    private bool IsBlockInHoleAtPosition(Vector2Int position)
    {
        if (position != LevelManager.Instance.GetCurrentLevelData().holePosition)
            return false;

        return LevelManager.Instance.IsElevatorAt(position);
    }
}