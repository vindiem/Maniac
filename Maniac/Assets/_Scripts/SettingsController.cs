using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsController : MonoBehaviour
{
    public Dropdown ResolutionDropdown;
    public Dropdown QualityDropdown;
    
    private Resolution[] resolutions;
    
    private void Start()
    {
        ResolutionDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;
        
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        ResolutionDropdown.AddOptions(options);
        ResolutionDropdown.RefreshShownValue();
        LoadSettings(currentResolutionIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    
    public void SetResolution(int currentResolutionIndex)
    {
        Resolution resolution = resolutions[currentResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetString("ResolutionPreference", ResolutionDropdown.options[ResolutionDropdown.value].text);
        PlayerPrefs.SetInt("QualityPreference", QualityDropdown.value);
        PlayerPrefs.SetInt("FullscreenPreference", Convert.ToInt32(Screen.fullScreen));
        PlayerPrefs.Save();
        
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        ResolutionDropdown.value = 
            PlayerPrefs.HasKey("ResolutionPreference") ? 
                PlayerPrefs.GetInt("ResolutionPreference") : currentResolutionIndex;

        QualityDropdown.value = 
            PlayerPrefs.HasKey("QualityPreference") ? 
                PlayerPrefs.GetInt("QualityPreference") : 3;

        if (PlayerPrefs.HasKey("FullscreenPreference"))
        {
            Screen.fullScreen = PlayerPrefs.GetInt("FullscreenPreference") == 1 ? true : false;
        }
        else
        {
            Screen.fullScreen = true;
        }
    }
    
}
