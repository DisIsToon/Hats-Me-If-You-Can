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

    public bool isSpeaking = false;
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
    public GameObject ivyImage;
    public GameObject chaseImage;
    public GameObject louImage;
    public GameObject snowmanImage;
    public GameObject headMasterImage;

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

    public void TryDiscoverCharacter(string npcName)
    {
        if (NewHatalougeManager.Instance == null) return;

        switch (npcName)
        {
            case "Ivy":
                NewHatalougeManager.Instance.DiscoverCharIvy();
                break;

            case "Chase":
                NewHatalougeManager.Instance.DiscoverCharChase();
                break;

            case "Lou":
                NewHatalougeManager.Instance.DiscoverCharLouire();
                break;
        }
    }

    public void ShowNPCImage(string npcName)
    {
        isSpeaking = true;
        currentSpeakerName = npcName; //  ADD THIS LINE

        liraImage.SetActive(npcName == "Lira");
        mallowImage.SetActive(npcName == "Mallow");
        tulipImage.SetActive(npcName == "Tulip");

        ivyImage.SetActive(npcName == "Ivy");
        chaseImage.SetActive(npcName == "Chase");
        louImage.SetActive(npcName == "Lou");

        snowmanImage.SetActive(npcName == "Snowman");
        headMasterImage.SetActive(npcName == "Headmaster");

        speakerNameText.text = npcName;
        //  ADD THIS LINE
        TryDiscoverCharacter(npcName);
    }

    

    public void ClearSpeakerName()
    {
        isSpeaking = false;
        currentSpeakerName = ""; //  ADD THIS

        if (speakerNameText != null)
            speakerNameText.text = "";
    }

    public void HideAllPortraits()
    {   
        ClearSpeakerName();
        liraImage.SetActive(false);
        mallowImage.SetActive(false);
        tulipImage.SetActive(false);
        ivyImage.SetActive(false);
        chaseImage.SetActive(false);
        louImage.SetActive(false);
        snowmanImage.SetActive(false);
        headMasterImage.SetActive(false);
    }

    public void OpenDialogUI()
    {
        MainScreen.SetActive(false);
        dialogUI.gameObject.SetActive(true);
        dialogUIActive = true;

        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
        ResetUI();
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

    public void ResetUI() {
        // Default state: hide options, show next button
        option1BTN.gameObject.SetActive(false);
        option2BTN.gameObject.SetActive(false);
        nextBTN.gameObject.SetActive(true);
    }

    public void ShowChoices() {
        option1BTN.gameObject.SetActive(true);
        option2BTN.gameObject.SetActive(true);
        nextBTN.gameObject.SetActive(false);
    }

    public void StartDialogSequence(string[] lines)
    {
        StartCoroutine(ShowDialogSequence(lines));
    }

    private IEnumerator ShowDialogSequence(string[] lines) {
        OpenDialogUI();

        // Hide option buttons during normal dialogue
        ResetUI();

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
