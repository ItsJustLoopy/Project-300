using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public class PlayerInputRouter : MonoBehaviour
{
    [SerializeField] private PlayerMovementController movement;
    [SerializeField] private PlayerElevatorController elevator;
    [SerializeField] private PlayerInventoryActions inventory;

    private Player _player;

    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _elevatorAction;
    private InputAction _pickupAction;
    private InputAction _placeAction;
    private InputAction _undoAction;
    private InputAction _resetAction;

    private void Awake()
    {
        _player = GetComponent<Player>();

        if (movement == null) movement = GetComponent<PlayerMovementController>();
        if (elevator == null) elevator = GetComponent<PlayerElevatorController>();
        if (inventory == null) inventory = GetComponent<PlayerInventoryActions>();

        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput != null)
        {
            _moveAction = _playerInput.actions["Move"];
            _elevatorAction = _playerInput.actions["Elevator"];
            _pickupAction = _playerInput.actions["Pickup"];
            _placeAction = _playerInput.actions["Place"];
            _undoAction = _playerInput.actions["Undo"];
            _resetAction = _playerInput.actions["Reset"];
        }
    }

    private void Update()
    {
        if (_player == null || movement == null)
            return;

        if (!_player.isMoving && _elevatorAction != null && _elevatorAction.triggered)
        {
            elevator?.TryUseElevator();
            return;
        }

        if (!_player.isMoving && _moveAction != null && _moveAction.triggered)
        {
            Vector2 input = _moveAction.ReadValue<Vector2>();
            Vector2Int direction = movement.ConvertInputToDirection(input);

            if (direction != Vector2Int.zero)
            {
                movement.TryMove(direction);
            }
        }

        if (_undoAction != null && _undoAction.triggered)
        {
            LevelManager.Instance.UndoLastMove();
        }

        if (!_player.isMoving && _resetAction != null && _resetAction.triggered)
        {
            LevelManager.Instance.ResetCurrentLevel();
            return;
        }

        if (_pickupAction != null && _pickupAction.triggered && LevelManager.Instance != null && LevelManager.Instance.IsInventoryUnlocked())
        {
            inventory?.TryPickup();
        }

        if (_placeAction != null && _placeAction.triggered && LevelManager.Instance != null && LevelManager.Instance.IsInventoryUnlocked())
        {
            inventory?.TryPlace();
        }
    }
}