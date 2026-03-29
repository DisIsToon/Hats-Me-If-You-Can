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


    public GameData data = new GameData();

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
        CardsController.Instance.SaveData(data);
        WinterBarrierSystem.Instance.SaveData(data);
        RealWinterBarrierSystem.Instance.SaveData(data);
        GameTracker.Instance.SaveData(data);

        // Start the popup coroutine
        StartCoroutine(ShowSavePopup());
    }

    public void OnLoadButton()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);
        CardsController.Instance.LoadData(data);
        WinterBarrierSystem.Instance.LoadData(data);
        RealWinterBarrierSystem.Instance.LoadData(data);
        GameTracker.Instance.LoadData(data);
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
