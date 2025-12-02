using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; set; }

    public GameObject pauseScreenUI;
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
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!isOpen)
                OpenPause();
            else
                ClosePause();
        }
    }

    public void OpenPause()
    {
        pauseScreenUI.SetActive(true);
        Time.timeScale = 0f;       // Freeze the world
        isOpen = true;
    }

    public void ClosePause()
    {
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

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f; // Just to be safe before switching scenes
        SceneManager.LoadScene("MainMenu");
    }
}
