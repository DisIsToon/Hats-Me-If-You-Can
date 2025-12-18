using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; set; }

    public GameObject pauseScreenUI;
    public GameObject settingScreenUI;
    public bool isOpen;

    public GameData data = new GameData();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isOpen)
                OpenPause();
            else
                ClosePause();
        }
    }

    public void OpenPauseScreen()
    {
        pauseScreenUI.SetActive(true);
    }

    public void ClosePauseScreen()
    {
        pauseScreenUI.SetActive(false);
    }

    public void OpenSettingScreen()
    {
        settingScreenUI.SetActive(true);
    }

    public void CloseSettingScreen()
    {
        settingScreenUI?.SetActive(false);
    }

    public void OpenPause()
    {
        OpenPauseScreen();
        Time.timeScale = 0f;       // Freeze the world
        isOpen = true;
    }

    public void ClosePause()
    {
        ClosePauseScreen();
        pauseScreenUI.SetActive(false);
        Time.timeScale = 1f;       // Unfreeze the world
        isOpen = false;
    }

    public void OnResumeButton()
    {
        ClosePause();
    }

    public void OnSaveButton()
    {
        CardsController.Instance.SaveData(data);
        WinterBarrierSystem.Instance.SaveData(data);
        CRBarrierSystem.Instance.SaveData(data);
        GameTracker.Instance.SaveData(data);
    }

    public void OnLoadButton()
    {
        CardsController.Instance.LoadData(data);
        WinterBarrierSystem.Instance.LoadData(data);
        CRBarrierSystem.Instance.LoadData(data);
        GameTracker.Instance.LoadData(data);
    }

    public void OnSettingButton()
    {
        ClosePauseScreen();
        OpenSettingScreen();
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f; // Just to be safe before switching scenes
        SceneManager.LoadScene("MainMenu");
    }
}
