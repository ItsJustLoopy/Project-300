using UnityEngine;

public class Title : ScreenBase
{
    [SerializeField]
    private float TimeOnScreen = 5;

    [SerializeField]
    private string NextScreen = "Menu";

    private void OnEnable()
    {
        Invoke(nameof(ShowMenu), TimeOnScreen);
    }

    private void OnDisable()
    {
        if (IsInvoking(nameof(ShowMenu)))
        {
            CancelInvoke(nameof(ShowMenu));
        }
    }

    private void ShowMenu()
    {
        ScreenManager.ShowScreen(NextScreen);
    }
}
