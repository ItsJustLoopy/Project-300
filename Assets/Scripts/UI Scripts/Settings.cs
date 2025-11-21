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
        ResDropDown.AddOptions(ResolutionStringList);
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
