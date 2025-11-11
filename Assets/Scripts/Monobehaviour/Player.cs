
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public GridMover _gridMover;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _elevatorAction;

    public bool isMoving => _gridMover.isMoving;

    public Vector2Int gridPosition
    {
        get => _gridMover.gridPosition;
        set => _gridMover.gridPosition = value;
    }

    private void Awake()
    {
        _gridMover = new GridMover(this, transform);
    }

    private void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _moveAction = _playerInput.actions["Move"];
        _elevatorAction = _playerInput.actions["Elevator"];

        _gridMover.gridPosition = LevelManager.Instance._currentLevelData.playerSpawn;
    }

    public void Update()
    {
        if (!isMoving && _elevatorAction != null && _elevatorAction.triggered)
        {
            TryUseElevator();
            return;
        }

        if (!isMoving && _moveAction.triggered)
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();
            Vector2Int direction = ConvertInputToDirection(input);

            if (direction != Vector2Int.zero)
            {
                TryMovePlayer(direction);
            }
        }
    }

    private void TryUseElevator()
    {
        if (LevelManager.Instance.IsElevatorAt(gridPosition))
        {
            Block elevator = LevelManager.Instance.GetElevatorAt(gridPosition);
            int targetLevel = elevator.GetTargetLevel();

            if (elevator.isAtOriginLevel)
            {
                Debug.Log($"Taking elevator UP to level {targetLevel}");
            }
            else
            {
                Debug.Log($"Taking elevator DOWN to level {targetLevel}");
            }

            StartCoroutine(LevelManager.Instance.UseElevator(gridPosition));
        }
    }

    private Vector2Int ConvertInputToDirection(Vector2 input)
    {
        // here we convert the input to -1,0 or 1 for each axis so we can do tile-based movement
        // we prioritize the x axis over the y axis 
        
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            return new Vector2Int((int)Mathf.Sign(input.x), 0); 
            // the .Sign() is to make sure we only get 1 or -1
        }

        if (input.y != 0)
        {
            return new Vector2Int(0, (int)Mathf.Sign(input.y));
        }

        return Vector2Int.zero; // if we don't have any input, return zero
    }

    private void TryMovePlayer(Vector2Int direction)
    {
        Vector2Int newPosition = gridPosition + direction;

        bool isBlockInHole = IsBlockInHoleAtPosition(newPosition);

        if (!isBlockInHole && LevelManager.Instance.CheckOutOfBounds(newPosition))
        {
            return;
        }

        if (isBlockInHole)
        {
            Move(newPosition);
            return;
        }

        GroundTile targetTile = LevelManager.Instance.GetTileAt(newPosition);

        if (targetTile != null && targetTile.isOccupied)
        {
            HandleBlockInteraction(targetTile.occupant, direction, newPosition);
        }
        else
        {
            Move(newPosition);
        }
    }

    private bool IsBlockInHoleAtPosition(Vector2Int position)
    {
        if (position != LevelManager.Instance.GetCurrentLevelData().holePosition)
        {
            return false;
        }

        return LevelManager.Instance.IsElevatorAt(position);
    }

    private void HandleBlockInteraction(Block block, Vector2Int direction, Vector2Int newPosition)
    {
        if (block.IsInHole())
        {
            Move(newPosition);
            return;
        }

        Vector2Int blockTargetPosition = newPosition + direction;

        bool targetIsHole = blockTargetPosition == LevelManager.Instance.GetCurrentLevelData().holePosition;
        bool targetOutOfBounds = LevelManager.Instance.CheckOutOfBounds(blockTargetPosition);

        if (targetIsHole && block.canBePlacedInHole)
        {
            Debug.Log("Elevator placed in hole!");
            PushBlockIntoHole(block, blockTargetPosition, newPosition);
            Move(newPosition);
        }
        else if (!targetOutOfBounds && !targetIsHole && CanPushBlockToTarget(blockTargetPosition))
        {
            PushBlock(block, blockTargetPosition, newPosition);
            Move(newPosition);
        }
    }

    private bool CanPushBlockToTarget(Vector2Int targetPosition)
    {
        GroundTile targetTile = LevelManager.Instance.GetTileAt(targetPosition);
        return targetTile != null && !targetTile.isOccupied;
    }

    public void Move(Vector2Int gridPos)
    {
        float levelY = LevelManager.Instance.currentLevelIndex * LevelManager.Instance.verticalSpacing + 1f;
        _gridMover.MoveToGrid(gridPos, levelY);
    }

    public void PushBlock(Block block, Vector2Int targetPos, Vector2Int currentPos)
    {
        LevelManager.Instance.GetTileAt(currentPos).occupant = null;
        LevelManager.Instance.GetTileAt(currentPos).isOccupied = false;

        LevelManager.Instance.GetTileAt(targetPos).occupant = block;
        LevelManager.Instance.GetTileAt(targetPos).isOccupied = true;
    
        block.levelIndex = LevelManager.Instance.currentLevelIndex;
    
        block.PushTo(targetPos);
    }

    public void PushBlockIntoHole(Block block, Vector2Int holePos, Vector2Int currentPos)
    {
        GroundTile oldTile = LevelManager.Instance.GetTileAt(currentPos);
        if (oldTile != null)
        {
            oldTile.occupant = null;
            oldTile.isOccupied = false;
        }

        
        block.levelIndex = LevelManager.Instance.currentLevelIndex;
    
        block.PushToThenPlaceInHole(holePos);
        LevelManager.Instance.RegisterElevator(holePos, block);

        Debug.Log("Block placed in hole, it now acts as an elavator. Press Spacebar to use.");
    }
}
