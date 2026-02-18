using UnityEngine;

public class Player : MonoBehaviour
{
    public InventoryManager inventory;
    public Vector2Int facingDirection;

    public GridMover _gridMover;

    public bool isMoving => _gridMover.isMoving;

    public Vector2Int gridPosition
    {
        get => _gridMover.gridPosition;
        set => _gridMover.gridPosition = value;
    }

    private void Awake()
    {
        _gridMover = new GridMover(this, transform);

        if (inventory == null)
        {
            inventory = GetComponent<InventoryManager>();
        }

        InventoryUI.Instance.AssignPlayer(this);
    }

    private void Start()
    {
        if (_gridMover.gridPosition == Vector2Int.zero)
        {
            _gridMover.gridPosition = LevelManager.Instance._currentLevelData.playerSpawn;
        }
    }
}
