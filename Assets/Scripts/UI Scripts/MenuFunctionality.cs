using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFunctionality : MonoBehaviour
{
    int sceneBuildIndex; 
    //This should allow the main play button to hold any level id therefore enabling the player to continue at whatever their
    //last played level was. If this works correctly as is, it should have the same function as a new game option when the player first starts.
    public void StartContinue()

    {
        SceneManager.LoadSceneAsync(sceneBuildIndex);
    }
}
