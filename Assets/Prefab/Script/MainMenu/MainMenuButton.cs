using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler

{

    [Tooltip("Function to perform: NewGame, LoadGame, ShowSettings, ShowCredits, ShowMainMenu, Exit")]
    public string buttonFunction;

    [Header("Optional Panel Targets")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject loadGamePanel;
    public GameObject gameSceneLoader;

    [Header("Button Styles")]
    public GameObject style1;
    public GameObject style2;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (style2 != null)
            style2.SetActive(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (style2 != null)
            style2.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("MainMenuButton clicked: " + buttonFunction);
        OnClick(); // call your existing method
    }

    public void OnClick()
    {
        switch (buttonFunction)
        {
            /* case "NewGame":
                 SceneManager.LoadScene(1);
                 Debug.Log("New Game Started");
                 break;
            */
            case "LoadGame":
                Debug.Log("LoadGame ");
                TogglePanels(loadGamePanel);
                break;
           
            case "ShowSettings":
                Debug.Log("ShowSettings ");
                TogglePanels(settingsPanel);
                break;

            case "ShowCredits":
                Debug.Log("ShowCredits ");
                TogglePanels(creditsPanel);
                break;

            case "ShowMainMenu":
                Debug.Log("ShowMainMenu ");
                TogglePanels(mainMenuPanel);
                break;

            case "Exit":
                Debug.Log("Game Closed");

#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }

    private void TogglePanels(GameObject activePanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (loadGamePanel != null) loadGamePanel.SetActive(false);

        if (activePanel != null) activePanel.SetActive(true);
    }
}
