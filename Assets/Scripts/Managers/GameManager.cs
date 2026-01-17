using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    #region Pause Functionality
    public bool GameIsPaused = false;

    public void UnPause()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
    #endregion
}