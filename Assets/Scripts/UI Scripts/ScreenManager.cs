using RotaryHeart.Lib.SerializableDictionary;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

//SerializableDictionaryBase requires the Serialized Dictionary Lite package to be installed.
[Serializable]
public class ScreenDictionary: SerializableDictionaryBase<string, GameObject> 
{ 

}

public class ScreenManager : Singleton<ScreenManager>
{
    [Space(10)]
    [Header("Screens")]
    [SerializeField]
    private string StartingScreen;

    [SerializeField]
    private ScreenDictionary Screens;

    private string PreviousScreen; //new
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoBackToPreviousScreen();
        }
    }

    public void ShowScreen(string screenName)
    {   
        if (screenName == ActiveScreen)
            return;

        if (!Screens.ContainsKey(screenName))
            return;

        PreviousScreen = ActiveScreen;

        if (!string.IsNullOrEmpty(ActiveScreen))
        {
            Screens[ActiveScreen].SetActive(false); //Turns the previous screen off  
        }
        ActiveScreen = screenName;
        Screens[ActiveScreen].SetActive(true);
    }

    public void GoBackToPreviousScreen()
    {
        if (string.IsNullOrEmpty(PreviousScreen))
            return;

        //Switches off the current screen
        Screens[ActiveScreen].SetActive(false);

        //turns on previous screen
        ActiveScreen = PreviousScreen;
        Screens[ActiveScreen].SetActive(true);

        //Clears previous as what previousscreen is now logged as is the one we just returned from
        PreviousScreen = null;
    }
    private void DisableAllScreens()
    {
        foreach (var screen in Screens.Values)
        {
            screen.SetActive(false);
        }
    }
}
