using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuTest : Menu
{
    [Header("Menu Navigation")]
    [SerializeField] private SaveSlotsMenu saveSlotsMenu;


    [Header("Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continuegameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitGameButton;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!DataPersistenceManager.instance.HasGameData())
        {
            continuegameButton.interactable = false;
            loadGameButton.interactable = false;
        }
    }

    void Update()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void OnNewGameClicked()
    {
        Debug.Log("OnNewGameClicked Clicked");
        saveSlotsMenu.ActivateMenu(false);
        this.DeactivateMenu();
    }

    public void OnLoadGameClicked()
    {
        Debug.Log("Load Game Clicked");
        saveSlotsMenu.ActivateMenu(true);
        this.DeactivateMenu();
    }

    public void OnContinueGameClicked()
    {
        Debug.Log("OnContinueGameClicked Clicked");
        DisableMenuButtons();

        DataPersistenceManager.instance.SaveGame();
        SceneManager.LoadSceneAsync("BiomeOptimized");
    }

    public void ExitGame()
    {
        Debug.Log("ExitGame button clicked. Quitting application...");

#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    private void DisableMenuButtons()
    {
        newGameButton.interactable = false;
        continuegameButton.interactable = false;
    }

    public void ActivateMenu()
    {
        this.gameObject.SetActive(true);
    }

    public void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }
}
