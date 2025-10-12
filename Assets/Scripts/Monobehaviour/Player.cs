using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool isMoving;
    
    private PlayerInput playerInput;
    private InputAction moveAction;
    
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
    }
    
    public void Initialize(Vector2Int gridPos)
    {
        gridPosition = gridPos;
    }

    public void Update()
    {
        if (!isMoving && moveAction.triggered)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            
            // This converts the input vector to a direction vector and moves the player one square at a time
            Vector2Int direction = Vector2Int.zero;
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                direction = new Vector2Int((int)Mathf.Sign(input.x), 0);
            }
            else if (input.y != 0)
            {
                direction = new Vector2Int(0, (int)Mathf.Sign(input.y));
            }
            
            if (direction != Vector2Int.zero )
            {
                Vector2Int newPosition = gridPosition + direction;
                
                // Check if new position is out of bounds
                if (LevelManager.Instance.CheckOutOfBounds(newPosition))
                {
                    return; 
                }
                
                // Check if new position is occupied
                if (LevelManager.Instance.GetTileAt(newPosition).isOccupied)
                {
                    
                    Vector2Int blockTargetPosition = newPosition + direction;
                    
                    // Check if block can be pushed 
                    if (!LevelManager.Instance.CheckOutOfBounds(blockTargetPosition)
                        && !LevelManager.Instance.GetTileAt(blockTargetPosition).isOccupied)
                    {
                        var block = LevelManager.Instance.GetTileAt(newPosition).occupant;  
                        PushBlock(block, blockTargetPosition, newPosition);
                        Move(newPosition);
                    }
                    
                }
                else
                {
                    
                    Move(newPosition);
                }
            }
        }
    }

    

    public void Move(Vector2Int gridPos)
    {
        
        if (!isMoving)
        {
            StartCoroutine(MoveTo(gridPos));
        }
    }

    private IEnumerator MoveTo(Vector2Int gridPos)
    {
        isMoving = true;
        
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(gridPos.x, 1, gridPos.y);
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        transform.position = targetPosition;
        gridPosition = gridPos;
        isMoving = false;
    }

    public void PushBlock(Block block, Vector2Int targetPos, Vector2Int currentPos)
    {
        // Update old tile
        LevelManager.Instance.GetTileAt(currentPos).occupant = null;
        LevelManager.Instance.GetTileAt(currentPos).isOccupied = false;
        
        // Update new tile
        LevelManager.Instance.GetTileAt(targetPos).occupant = block;  
        LevelManager.Instance.GetTileAt(targetPos).isOccupied = true;
        
        block.PushTo(targetPos);
    }
    
}
