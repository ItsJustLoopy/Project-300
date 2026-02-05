using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public class PlayerElevatorController : MonoBehaviour
{
    private Player _player;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    public void TryUseElevator()
    {
        if (!LevelManager.Instance.IsElevatorAt(_player.gridPosition))
            return;

        Block elevator = LevelManager.Instance.GetElevatorAt(_player.gridPosition);
        if (elevator == null)
            return;

        int targetLevel = elevator.GetTargetLevel();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayElevatorSound();
        }

        _player.StartCoroutine(LevelManager.Instance.UseElevator(_player.gridPosition));
    }
}