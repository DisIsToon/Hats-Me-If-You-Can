using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;

    public GameObject settingScreenUI;
    public GameObject menuScreen;

    private void Start()
    {
        bgmSlider.onValueChanged.AddListener((v) => SoundManager.Instance.SetBGMVolume(v));
        sfxSlider.onValueChanged.AddListener((v) => SoundManager.Instance.SetSFXVolume(v));

        // Load saved values (optional)
        bgmSlider.value = SoundManager.Instance.bgmVolume;
        sfxSlider.value = SoundManager.Instance.sfxVolume;

    }



    public void BackButton()
    {
        PauseManager.Instance.CloseSettingScreen();
        PauseManager.Instance.OpenPauseScreen();

    }

    public void ExitButton()
    {
        PauseManager.Instance.CloseSettingScreen();
        PauseManager.Instance.ClosePause();
    }

    public void CloseSettingScreen()
    {
        settingScreenUI.SetActive(false);
        menuScreen.SetActive(true);
    }
}
