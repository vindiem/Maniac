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
    
    public Slider MusicSlider;
    public Slider SFXSlider;
    
    private float musicVolume;
    private float sfxVolume;
    
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
        ResolutionDropdown.value = options.Count;
        ResolutionDropdown.RefreshShownValue();
        LoadSettings(currentResolutionIndex);
        
        musicVolume = PlayerPrefs.GetFloat("MusicVolume");
        sfxVolume = PlayerPrefs.GetFloat("SoundVolume");
        MusicSlider.value = musicVolume;
        SFXSlider.value = sfxVolume;
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
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SoundVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadSettings(int currentResolutionIndex)
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

    public void SetMusicVolume()
    {
        musicVolume = MusicSlider.value;
        AudioSource music = GameObject.FindGameObjectWithTag("MusicAudioSource").GetComponent<AudioSource>();
        music.volume = musicVolume;
    }

    public void SetSFXVolume()
    {
        sfxVolume = SFXSlider.value;
        AudioSource sfx = GameObject.FindGameObjectWithTag("SFXAudioSource").GetComponent<AudioSource>();
        sfx.volume = sfxVolume;
    }
    
}
