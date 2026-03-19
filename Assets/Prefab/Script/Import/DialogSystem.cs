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
    public TMPro.TextMeshProUGUI speakerNameText;
    public string currentSpeakerName = "";

    private bool nextClicked = false;
    public Button nextBTN;
    public Button option1BTN;
    public Button option2BTN;

    public Canvas dialogUI;

    public bool dialogUIActive;

    public GameObject MainScreen;

    public GameObject liraImage;
    public GameObject mallowImage;
    public GameObject tulipImage;

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

    public void SetSpeakerName(string name)
    {
        currentSpeakerName = name ?? "";
        if (speakerNameText != null)
            speakerNameText.text = currentSpeakerName;
    }
    public string GetSpeakerName()
    {
        return currentSpeakerName;
    }

    public void ShowNPCImage(string npcName)
    {
        liraImage.SetActive(npcName == "Lira");
        mallowImage.SetActive(npcName == "Mallow");
        tulipImage.SetActive(npcName == "Tulip");

        speakerNameText.text = npcName;
    }

    public void ClearSpeakerName()
    {
        if (speakerNameText != null)
            speakerNameText.text = "";
    }

    public void HideAllPortraits()
    {
        
        ClearSpeakerName();
        liraImage.SetActive(false);
        mallowImage.SetActive(false);
        tulipImage.SetActive(false);
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
        HideAllPortraits();
        ClearSpeakerName();
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }
    public void StartDialogSequence(string[] lines)
    {
        StartCoroutine(ShowDialogSequence(lines));
    }

    private IEnumerator ShowDialogSequence(string[] lines) {
        OpenDialogUI();

        // Hide option buttons during normal dialogue
        option1BTN.gameObject.SetActive(false);
        option2BTN.gameObject.SetActive(false);

        nextBTN.gameObject.SetActive(true);

        for (int i = 0; i < lines.Length; i++) {
            dialogText.text = lines[i];

            nextClicked = false;

            nextBTN.onClick.RemoveAllListeners();
            nextBTN.onClick.AddListener(() =>
            {
                Debug.Log("CLICK WORKS");
                nextClicked = true;
            });

            yield return new WaitUntil(() => nextClicked);
        }

        nextBTN.gameObject.SetActive(false);
        CloseDialogUI();
    }
}
