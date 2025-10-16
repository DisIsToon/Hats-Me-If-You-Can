using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DialogSystem : MonoBehaviour
{
    public static DialogSystem Instance { get; set; }

    public TextMeshProUGUI dialogText;

    public Button option1BTN;
    public Button option2BTN;

    public Canvas dialogUI;

    public bool dialogUIActive;

    public GameObject MainScreen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        dialogUIActive = false;
    }

    public void OpenDialogUI()
    {
        MainScreen.SetActive(false);
        dialogUI.gameObject.SetActive(true);
        dialogUIActive = true;
        
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
    }

    public void CloseDialogUI()
    {
        MainScreen.SetActive(true);
        dialogUI.gameObject.SetActive(false);
        dialogUIActive = false;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }
}
