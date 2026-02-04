using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Settings : ScreenBase
{
    public TMP_Dropdown ResDropDown;
    public Toggle FullScreenToggle;

    Resolution[] AllResolutions;
    bool IsFullScreen;
    int SelectedResolution;
    List<Resolution> SelectedResolutionsList = new List<Resolution>();

    private void Start()
    {
        IsFullScreen = true;
        AllResolutions = Screen.resolutions;

        List<string> ResolutionStringList = new List<string>();
        string newRes;
        foreach (Resolution res in AllResolutions)
        {
            newRes = res.width.ToString() +  " x " + res.height.ToString();
            if(!ResolutionStringList.Contains(newRes))
            {
                ResolutionStringList.Add(newRes);
                SelectedResolutionsList.Add(res);
            }
        }
        ResDropDown.ClearOptions();
        ResDropDown.AddOptions(ResolutionStringList);

        int currentIndex = 0;
        for (int i = 0; i < SelectedResolutionsList.Count; i++)
        {
            if (SelectedResolutionsList[i].width == Screen.currentResolution.width && SelectedResolutionsList[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
                break;
            }
        }

        SelectedResolution = currentIndex;   //Store it
        ResDropDown.value = currentIndex;    //Select it in the UI
        ResDropDown.RefreshShownValue();     //Force display update

        //Sync fullscreen toggle with actual state
        FullScreenToggle.isOn = Screen.fullScreen;
        IsFullScreen = Screen.fullScreen;
    }
    public void ChangeResolution()
    {
        SelectedResolution = ResDropDown.value;
        Screen.SetResolution(SelectedResolutionsList[SelectedResolution].width, 
                             SelectedResolutionsList[SelectedResolution].height, IsFullScreen);
    }
    public void ChangeFullScreen()
    {
        IsFullScreen = FullScreenToggle.isOn;
        Screen.SetResolution(SelectedResolutionsList[SelectedResolution].width,
                             SelectedResolutionsList[SelectedResolution].height, IsFullScreen);
    }
}
