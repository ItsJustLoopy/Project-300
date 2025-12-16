using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public void OnEnable()
    {
        GameManager.Instance.Pause();  
    }

    public void OnPause()
    {
        ScreenManager.Instance.ShowScreen("HUD");    
    }

    public void OnDisable()
    {
        GameManager.Instance.UnPause();
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
