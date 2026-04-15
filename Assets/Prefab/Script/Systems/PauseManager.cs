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
    public bool isOpenPause;   // Tracks pause screen
    public bool isOpenSetting; // Tracks settings screen

    [Header("Save Popup Settings")]
    public GameObject savedPopup; // Assign in Inspector
    public float popupFadeDuration = 1f; // how long it fades out
    public float popupDelay = 1f; // wait before showing


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else Instance = this;
    }

    void Start()
    {
        if (GameDataManager2.Instance.currentData != null)
        {
            OnLoadButton();
        }
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
        SoundManager.Instance.PlayMainMenuMusic();
        pauseScreenUI.SetActive(true);
    }

    public void ClosePauseScreen()
    {
        pauseScreenUI.SetActive(false);
        CheckTutorialSettingClosed();
    }

    public void OpenSettingScreen()
    {
        settingScreenUI.SetActive(true);
        isOpenSetting = true;
    }

    public void CloseSettingScreen()
    {
        settingScreenUI?.SetActive(false);
        isOpenSetting = false;

        CheckTutorialSettingClosed();
    }

    public void OpenPause()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);

        OpenPauseScreen();
        Time.timeScale = 0f;       // Freeze the world
        isOpen = true;
        isOpenPause = true;

        if (NewHatalougeManager.Instance != null &&
NewHatalougeManager.Instance.notTutorial == false)
        {
            TutorialManager.Instance.OnSettingsOpened();
        }


    }

    public void ClosePause()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);
        SoundManager.Instance.ReturnToBiomeMusic();
        ClosePauseScreen();
        pauseScreenUI.SetActive(false);
        Time.timeScale = 1f;       // Unfreeze the world
        isOpen = false;
        isOpenPause = false;
        CheckTutorialSettingClosed();

    }

    public void OnResumeButton()
    {
        ClosePause();
    }

    private IEnumerator ShowSavePopup()
    {
        Debug.Log("Saving");

        // Wait before showing, ignoring pause
        yield return new WaitForSecondsRealtime(popupDelay);

        // Activate popup
        savedPopup.SetActive(true);

        // Make sure it has a CanvasGroup for fading
        CanvasGroup cg = savedPopup.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = savedPopup.AddComponent<CanvasGroup>();
        }
        cg.alpha = 1f;

        float elapsed = 0f;

        // Fade out
        while (elapsed < popupFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // still ignore timescale
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / popupFadeDuration);
            yield return null;
        }

        cg.alpha = 0f;
        savedPopup.SetActive(false);
        Debug.Log("Save Complete");
    }
    public void OnSaveButton()
    {
        Debug.Log("Save Clicked");
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);

        // Only allow saving in BiomeOptimized
        if (SceneManager.GetActiveScene().name != "BiomeOptimized")
        {
            Debug.LogWarning("Saving only allowed in BiomeOptimized scene!");
            return;
        }

        // Use GameDataManager2's data
        GameData2 saveData = GameDataManager2.Instance.currentData;

        if (saveData == null)
        {
            Debug.LogError("No GameData2 found! Did you start/load a slot?");
            return;
        }

       
        // Transfer your current systems into saveData
        CardsController.Instance.SaveData(saveData);
        WinterBarrierSystem.Instance.SaveData(saveData);
        RealWinterBarrierSystem.Instance.SaveData(saveData);
        GameTracker.Instance.SaveData(saveData);
        
        // SAVE TO SLOT
        GameDataManager2.Instance.SaveGame();

        // Show popup
        StartCoroutine(ShowSavePopup());
    }

    public void OnLoadButton()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);

        GameData2 loadData = GameDataManager2.Instance.currentData;

        if (loadData == null)
        {
            Debug.LogError("No GameData2 loaded!");
            return;
        }

      
        CardsController.Instance.LoadData(loadData);
        WinterBarrierSystem.Instance.LoadData(loadData);
        RealWinterBarrierSystem.Instance.LoadData(loadData);
        GameTracker.Instance.LoadData(loadData);
        
    }

    public void OnSettingButton()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);
        ClosePauseScreen();
        OpenSettingScreen();
    }

    public void OnMainMenuButton()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);
        Time.timeScale = 1f; // Just to be safe before switching scenes
        SceneManager.LoadScene("MainMenu");
    }

    private void CheckTutorialSettingClosed()
    {
        // Only trigger tutorial when both pause and settings are closed
        if (!isOpenPause && !isOpenSetting)
        {
            if (NewHatalougeManager.Instance != null &&
                NewHatalougeManager.Instance.notTutorial == false)
            {
                TutorialManager.Instance.OnSettingsClosedForTutorial();
            }
        }
    }
}
