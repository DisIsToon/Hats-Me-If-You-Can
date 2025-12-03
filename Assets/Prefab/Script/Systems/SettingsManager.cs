using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Slider aaSlider;

    private void Start()
    {
        bgmSlider.onValueChanged.AddListener((v) => SoundManager.Instance.SetBGMVolume(v));
        sfxSlider.onValueChanged.AddListener((v) => SoundManager.Instance.SetSFXVolume(v));

        // Load saved values (optional)
        bgmSlider.value = SoundManager.Instance.bgmVolume;
        sfxSlider.value = SoundManager.Instance.sfxVolume;

        // AA Slider
        aaSlider.onValueChanged.AddListener(SetAntiAliasing);

        // Load current AA level into UI
        aaSlider.value = ConvertAAToSlider(QualitySettings.antiAliasing);
    }

    private int ConvertAAToSlider(int aa)
    {
        switch (aa)
        {
            case 2: return 1;
            case 4: return 2;
            case 8: return 3;
            default: return 0;
        }
    }

    public void SetAntiAliasing(float sliderValue)
    {
        int level = Mathf.RoundToInt(sliderValue);

        switch (level)
        {
            case 0: QualitySettings.antiAliasing = 0; break;
            case 1: QualitySettings.antiAliasing = 2; break;
            case 2: QualitySettings.antiAliasing = 4; break;
            case 3: QualitySettings.antiAliasing = 8; break;
        }
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
}
