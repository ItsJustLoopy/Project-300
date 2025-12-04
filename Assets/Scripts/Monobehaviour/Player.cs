
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

        if (_gridMover.gridPosition == Vector2Int.zero)
        {
            _gridMover.gridPosition = LevelManager.Instance._currentLevelData.playerSpawn;
        }
    }

    public void Update()
    {
        if (!isMoving && _elevatorAction != null && _elevatorAction.triggered)
        {
            TryUseElevator();
            return;
            Debug.Log("Used Elevator");
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
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayElevatorSound();
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
            Debug.Log("Black block placed in hole - Elevator created!");
            PushBlockIntoHole(block, blockTargetPosition, newPosition);
            Move(newPosition);
        }
        else if (!targetOutOfBounds && !targetIsHole)
        {
            GroundTile targetTile = LevelManager.Instance.GetTileAt(blockTargetPosition);
            
            if (targetTile != null && targetTile.isOccupied && targetTile.occupant != null)
            {
                Block targetBlock = targetTile.occupant;
                
                if (block.CanCombineWith(targetBlock))
                {
                    LevelManager.Instance.GetTileAt(newPosition).occupant = null;
                    LevelManager.Instance.GetTileAt(newPosition).isOccupied = false;
                    
                    block.levelIndex = LevelManager.Instance.currentLevelIndex;
                    block.PushTo(blockTargetPosition);
                    
                    StartCoroutine(CombineBlocksAfterMove(block, targetBlock, blockTargetPosition));
                    Move(newPosition);
                }
            }
            else if (targetTile != null && !targetTile.isOccupied)
            {
                PushBlock(block, blockTargetPosition, newPosition);
                Move(newPosition);
            }
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
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerMoveSound();
        }
    }

    public void PushBlock(Block block, Vector2Int targetPos, Vector2Int currentPos)
    {
        LevelManager.Instance.GetTileAt(currentPos).occupant = null;
        LevelManager.Instance.GetTileAt(currentPos).isOccupied = false;

        LevelManager.Instance.GetTileAt(targetPos).occupant = block;
        LevelManager.Instance.GetTileAt(targetPos).isOccupied = true;
    
        block.levelIndex = LevelManager.Instance.currentLevelIndex;
    
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBlockPushSound();
            
        }
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
    
    private IEnumerator CombineBlocksAfterMove(Block movingBlock, Block targetBlock, Vector2Int position)
    {
        while (movingBlock.isMoving)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(0.1f);
        
        movingBlock.CombineWith(targetBlock);
        
        GroundTile tile = LevelManager.Instance.GetTileAt(position);
        if (tile != null)
        {
            tile.occupant = movingBlock;
            tile.isOccupied = true;
        }
        
        Debug.Log($"Combined into: {movingBlock.data.blockColor}");
    }
}
