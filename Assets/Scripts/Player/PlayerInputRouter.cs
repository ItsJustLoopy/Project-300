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

        // TODO: move to inputAction system
        if (Input.GetKeyDown(KeyCode.U))
        {
            LevelManager.Instance.UndoLastMove();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            inventory?.TryPickup();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            inventory?.TryPlace();
        }
    }
}