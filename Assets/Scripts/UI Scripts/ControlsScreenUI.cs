using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsScreenUI : MonoBehaviour
{
    [SerializeField] private TMP_Text controlsText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (controlsText == null)
            return;

        var sb = new StringBuilder();

        sb.AppendLine("Move: D-Pad | WASD");
        sb.AppendLine($"Elevator: A | Spacebar");
        sb.AppendLine($"Pick up block: {GetBinding("Pickup")} (face a block)");
        sb.AppendLine($"Place block: {GetBinding("Place")} (face an empty tile)");
        sb.AppendLine($"Undo: {GetBinding("Undo")}");
        sb.AppendLine($"Reset: {GetBinding("Reset")}");
        sb.AppendLine("Pause: Esc");

        controlsText.text = sb.ToString();
    }

    private string GetBinding(string actionName)
    {
        if (LevelManager.Instance == null || LevelManager.Instance._playerInstance == null)
            return actionName;

        var input = LevelManager.Instance._playerInstance.GetComponent<PlayerInput>();
        if (input == null)
            return actionName;

        var action = input.actions[actionName];
        if (action == null)
            return actionName;

        return action.GetBindingDisplayString();
    }

    public void OnBack()
    {
        ScreenManager.Instance.ShowScreen("Pause Screen");
    }
}