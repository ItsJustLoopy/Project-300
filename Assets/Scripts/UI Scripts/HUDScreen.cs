using UnityEngine;

public class HUDScreen : MonoBehaviour
{
    public void OnPause()
    {
        ScreenManager.Instance.ShowScreen("Pause Screen");
    }
}
