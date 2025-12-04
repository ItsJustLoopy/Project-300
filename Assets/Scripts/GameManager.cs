using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public bool GameIsPaused = false;

    public void UnPause()
    {
        Debug.Log("UnPaused");
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        Debug.Log("Paused");
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
}
