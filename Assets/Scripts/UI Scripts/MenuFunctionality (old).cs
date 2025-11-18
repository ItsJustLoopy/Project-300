using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFunctionality : MonoBehaviour
{
    //int sceneBuildIndex;
    //This should allow the main play button to hold any level id therefore enabling the player to continue at whatever their
    //last played level was. If this works correctly as is,
    //it should have the same function as a new game option when the player first starts.
    public void Start()
    {
        //SceneManager.LoadSceneAsync(sceneBuildIndex); This is the code that applies to the now redundant comments above

        SceneManager.LoadScene("Game"); //This code loads the scene called game
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            EscapeButtonLogic();
        }
    }

    //As all levels (scenes) are formatted as "Level [number]", we can call a method like this upon clicking a level button.
    //Having an int in the method parameters means that each time this method is assigned in the editor, it will ask for a number.
    //We hardcode that number through the editor, making it the same as the levels number in the scene name,
    //then once its recorded, assign a string variable representing
    //the unique ID of the level (its full scene name) to be a combination of "Level " and the number.
    //Once we have this, we can call the loadscene method to open a scene with with the same name as our generated LevelID
    //which will be the correct level/scene every time.
    public void LevelLoad(int LevelNumber)
    {
        string LevelID = $"Level {LevelNumber}";
        SceneManager.LoadScene(LevelID);
    }

    public void EscapeButtonLogic()
    {
        /*if(GameObject.("Level Selection Screen").SetActive = true)
        {
            GameObject.SetActive("Level Selection Screen") = false;
        }
        */

        GameObject LevelSelection = GameObject.Find("Level Selection Screen");

        GameObject LevelLogic = GameObject.Find("Level Logic Screen");
        GameObject LevelExplanation = GameObject.Find("LevelExplanations");

        GameObject LevelSettings = GameObject.Find("Settings Screen");

        if (LevelSelection.activeSelf)
        {
            LevelSelection.SetActive(false);
        }
        if (LevelLogic.activeSelf)
        {
            LevelLogic.SetActive(false);
        }
        if (LevelExplanation.activeSelf)
        {
            LevelExplanation.SetActive(false);
        }
        if (LevelSettings.activeSelf)
        {
            LevelSettings.SetActive(false);
        }
    }

    public void ExitApp()
    {
        Application.Quit();
    }
}
