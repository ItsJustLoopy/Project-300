using RotaryHeart.Lib.SerializableDictionary;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

//SerializableDictionaryBase requires the Serialized Dictionary Lite package to be installed.
[Serializable]
public class ScreenDictionary: SerializableDictionaryBase<string, GameObject> { }

public class ScreenManager : Singleton<ScreenManager>
{
    [Space(10)]
    [Header("Screens")]
    [SerializeField]
    private string StartingScreen;

    [SerializeField]
    private ScreenDictionary Screens;

    private string ActiveScreen;

    private void Start()
    {
        DisableAllScreens();
        ShowScreen(StartingScreen);
    }

    public void Play()
    {
        DisableAllScreens();
        SceneManager.LoadScene("Game");
    }

    public void ShowScreen(string screenName)
    {
        if (screenName == ActiveScreen)
            return;

        if (!Screens.ContainsKey(screenName))
            return;

        if (!string.IsNullOrEmpty(ActiveScreen))
        {
            Screens[ActiveScreen].SetActive(false);
        }

        ActiveScreen = screenName;
        Screens[ActiveScreen].SetActive(true);
    }

    private void DisableAllScreens()
    {
        foreach (var screen in Screens.Values)
        {
            screen.SetActive(false);
        }
    }
}
