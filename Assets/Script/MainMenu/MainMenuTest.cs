using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuTest : MonoBehaviour
{
    [Header("Menu Buttons")]

    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continuegameButton;

    private void Start()
    {
        if(!DataPersistenceManager.instance.HasGameData())
        {
            continuegameButton.interactable = false;
        }
    }
    public void OnNewGameClicked()
    {
        DisableMenuButtons();
        Debug.Log("New Game CLicked");
        DataPersistenceManager.instance.NewGame();
        SceneManager.LoadSceneAsync("TestPlayground");
    }

    public void OnContinueGameClicked()
    {
        DisableMenuButtons();
        Debug.Log("Load Game Clicked");
        SceneManager.LoadSceneAsync("TestPlayground");
    }

    private void DisableMenuButtons()
    {
        newGameButton.interactable = false;
        continuegameButton.interactable = false;
    }
}
