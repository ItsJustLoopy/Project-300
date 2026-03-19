using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public void OnEnable()
    {
        //GameManager.Instance.Pause();  
    }

    public void OnPause()
    {
        ScreenManager.Instance.ShowScreen("HUD");    
    }

    public void OnDisable()
    {
        //GameManager.Instance.UnPause();
        
        //game manager literally does nothing since timescale isnt used for anything
        // - sami
    }

    public void LoadMenu()
    {
        ScreenManager.Instance.ShowScreen("Main Menu Screen");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
