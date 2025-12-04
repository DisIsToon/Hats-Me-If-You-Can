using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    [Header("References")]
    public GameObject pressFUI;   
    public GameObject dialogUI;   
    public GameObject MainScreen;

    [Header("NPC Name")]
    public string npcName;   

    [Header("NPC & Quest Data")]
    public List<Quest> quests;
    public Quest currentActiveQuest = null;
    public int activeQuestIndex = 0;
    public bool firstTimeInteraction = true;
    public int currentDialog;

    public bool playerInRange = false;
    public bool isTalkingWithPlayer = false;

    [Header("Button Options")]
    public bool disableOption2 = false;   // <--- NEW

    // UI References
    public TextMeshProUGUI npcDialogText;
    public Button optionButton1;
    public TextMeshProUGUI optionButton1Text;
    public Button optionButton2;
    public TextMeshProUGUI optionButton2Text;

    private Transform player;

    // Static list for multiple NPCs
    public static List<NPC> allNPCs = new List<NPC>();

    private void Awake()
    {
        // Add this NPC to the list
        if (!allNPCs.Contains(this))
            allNPCs.Add(this);
    }

    private void OnDestroy()
    {
        // Remove from list if destroyed
        if (allNPCs.Contains(this))
            allNPCs.Remove(this);
    }

    void Start()
    {
        if (disableOption2 && optionButton2 != null)
        {
            optionButton2.gameObject.SetActive(false);
        }

        if (DialogSystem.Instance == null)
        {
            Debug.LogError("DialogSystem.Instance is NULL! Make sure DialogSystem exists in the scene.");
            return;
        }

        npcDialogText = DialogSystem.Instance.dialogText;

        optionButton1 = DialogSystem.Instance.option1BTN;
        optionButton2 = DialogSystem.Instance.option2BTN;

        // --- OPTION BUTTON 1 ---
        Transform child1 = optionButton1.transform.Find("Text(TMP)");
        if (child1 != null)
            optionButton1Text = child1.GetComponent<TextMeshProUGUI>();
        else
            optionButton1Text = optionButton1.GetComponentInChildren<TextMeshProUGUI>();

        if (optionButton1Text == null)
            Debug.LogError("NPC ERROR: optionButton1Text NOT FOUND. Check the button hierarchy.");

        // --- OPTION BUTTON 2 ---
        Transform child2 = optionButton2.transform.Find("Text(TMP)");
        if (child2 != null)
            optionButton2Text = child2.GetComponent<TextMeshProUGUI>();
        else
            optionButton2Text = optionButton2.GetComponentInChildren<TextMeshProUGUI>();

        if (optionButton2Text == null)
            Debug.LogError("NPC ERROR: optionButton2Text NOT FOUND. Check the button hierarchy.");

        // Hide UI
        if (pressFUI != null) pressFUI.SetActive(false);
        if (dialogUI != null) dialogUI.SetActive(false);

        // Find player
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null) player = found.transform;
    }

    void Update()
    {
        if (!playerInRange) return;

        if (pressFUI != null && (dialogUI == null || !dialogUI.activeSelf))
            pressFUI.SetActive(true);

        if (Input.GetKeyDown(KeyCode.E))
        {
            SelectionManager.Instance.interactableNpcDetected = false;
            StartConversation();
            if (pressFUI != null) pressFUI.SetActive(false);
        }
    }

    private void HideOption2()
    {
        if (optionButton2 != null)
            optionButton2.gameObject.SetActive(false);
    }

    void CompleteLiraQuest()
    {
        QuestManager.Instance.liraPuzzleQuest.isCompleted = true;
        QuestManager.Instance.winterForestPass = true;

        DialogSystem.Instance.OpenDialogUI();
        DialogSystem.Instance.dialogText.text = "You solved it! Thank you so much.\nHere, take this. You can now pass through the Winter Forest.";

        QuestManager.Instance.puzzleObject.SetActive(false);
    }

    public string GetNPCName()
    {
        return npcName;
    }

    private bool CheckBoolRequirement(QuestInfo info)
    {
        if (!info.requireBoolToStart)
            return true; // No requirement, so good to go

        // Look up the bool by name
        switch (info.boolRequirementName)
        {
            case "puzzleComplete":
                return QuestManager.Instance.puzzleComplete;
        }

        Debug.LogWarning("Bool requirement name not found: " + info.boolRequirementName);
        return false;
    }

    public void StartConversation()
    {
        isTalkingWithPlayer = true;

        DialogSystem.Instance.ShowNPCImage(npcName);

        // --- First-time interaction ---
        if (firstTimeInteraction)
        {
            currentActiveQuest = quests[activeQuestIndex];

            // Check bool requirement
            if (!CheckBoolRequirement(currentActiveQuest.info))
            {
                ShowIncompleteRequirementMessage();
                return;
            }

            firstTimeInteraction = false;
            currentDialog = 0;
            StartQuestInitialDialog();
            return;
        }

        // --- Declined quest ---
        if (currentActiveQuest.declined)
        {
            DialogSystem.Instance.OpenDialogUI();
            npcDialogText.text = currentActiveQuest.info.comebackAfterDecline;
            SetAcceptAndDeclineOptions();
            return;
        }

        // --- Accepted but not completed ---
        if (currentActiveQuest.accepted && !currentActiveQuest.isCompleted)
        {
            // Check bool requirement first
            if (!CheckBoolRequirement(currentActiveQuest.info))
            {
                ShowIncompleteRequirementMessage();
                return;
            }

            // Check items and checkpoints
            if (AreQuestRequirmentsCompleted())
            {
                SubmitRequiredItems();
                DialogSystem.Instance.OpenDialogUI();
                npcDialogText.text = currentActiveQuest.info.comebackCompleted;
                optionButton1Text.text = "Take Reward";
                optionButton1.onClick.RemoveAllListeners();
                optionButton1.onClick.AddListener(() => ReceiveRewardAndCompleteQuest());
                if (!disableOption2)
                    optionButton2.gameObject.SetActive(false);

            }
            else
            {
                DialogSystem.Instance.OpenDialogUI();
                npcDialogText.text = currentActiveQuest.info.comebackInProgress;
                optionButton1Text.text = "Close";
                optionButton1.onClick.RemoveAllListeners();
                optionButton1.onClick.AddListener(() =>
                {
                    DialogSystem.Instance.CloseDialogUI();
                    isTalkingWithPlayer = false;
                });
                if (!disableOption2)
                    optionButton2.gameObject.SetActive(false);

            }
            return;
        }

        // --- Completed quest ---
        if (currentActiveQuest.isCompleted)
        {
            DialogSystem.Instance.OpenDialogUI();
            npcDialogText.text = currentActiveQuest.info.finalWords;
            optionButton1Text.text = "Close";
            optionButton1.onClick.RemoveAllListeners();
            optionButton1.onClick.AddListener(() =>
            {
                DialogSystem.Instance.CloseDialogUI();
                isTalkingWithPlayer = false;
            });
            if (!disableOption2)
                optionButton2.gameObject.SetActive(false);

            return;
        }

        // --- Safety fallback ---
        if (!currentActiveQuest.initialDialogCompleted)
            StartQuestInitialDialog();
    }

    // --- Helper function for showing "incomplete requirement" message ---
    public void ShowIncompleteRequirementMessage()
    {
        DialogSystem.Instance.OpenDialogUI();
        npcDialogText.text = "Hmm… something’s missing. Come back when you've handled that puzzle.";
        optionButton1Text.text = "Close";
        optionButton1.onClick.RemoveAllListeners();
        optionButton1.onClick.AddListener(() =>
        {
            DialogSystem.Instance.CloseDialogUI();
            isTalkingWithPlayer = false;
        });
        if (!disableOption2)
            optionButton2.gameObject.SetActive(false);

    }


    public void SetAcceptAndDeclineOptions()
    {
        optionButton1Text.text = currentActiveQuest.info.acceptOption;
        optionButton1.onClick.RemoveAllListeners();
        optionButton1.onClick.AddListener(AcceptedQuest);

        // Only enable and assign option 2 if it is not disabled
        if (!disableOption2 && optionButton2 != null)
        {
            optionButton2.gameObject.SetActive(true);
            optionButton2Text.text = currentActiveQuest.info.declineOption;
            optionButton2.onClick.RemoveAllListeners();
            optionButton2.onClick.AddListener(DeclinedQuest);
        }
        else
        {
            HideOption2(); // hide it just in case
        }
    }

    private void SubmitRequiredItems()
    {
        if (!string.IsNullOrEmpty(currentActiveQuest.info.firstRequirmentItem))
            InventorySystem.Instance.RemoveItem(currentActiveQuest.info.firstRequirmentItem, currentActiveQuest.info.firstRequirementAmount);

        if (!string.IsNullOrEmpty(currentActiveQuest.info.secondRequirmentItem))
            InventorySystem.Instance.RemoveItem(currentActiveQuest.info.secondRequirmentItem, currentActiveQuest.info.secondRequirementAmount);
    }

    private bool AreQuestRequirmentsCompleted()
    {
        // --- Check item requirements ---
        int firstCount = InventorySystem.Instance.CheckItemAmount(currentActiveQuest.info.firstRequirmentItem);
        int secondCount = InventorySystem.Instance.CheckItemAmount(currentActiveQuest.info.secondRequirmentItem);

        bool itemsDone = firstCount >= currentActiveQuest.info.firstRequirementAmount &&
                         secondCount >= currentActiveQuest.info.secondRequirementAmount;

        // --- Check checkpoints ---
        bool checkpointsDone = true;
        if (currentActiveQuest.info.hasCheckpoints && currentActiveQuest.info.checkpoints != null)
        {
            foreach (Checkpoints cp in currentActiveQuest.info.checkpoints)
            {
                if (!cp.isCompleted)
                {
                    checkpointsDone = false;
                    break;
                }
            }
        }

        // --- Check bool requirement ---
        bool boolRequirementDone = true;
        if (currentActiveQuest.info.requireBoolToStart)
        {
            switch (currentActiveQuest.info.boolRequirementName)
            {
                case "puzzleComplete":
                    boolRequirementDone = QuestManager.Instance.puzzleComplete;
                    break;

                // Add other bool names here if needed
                default:
                    Debug.LogWarning("Bool requirement not recognized: " + currentActiveQuest.info.boolRequirementName);
                    boolRequirementDone = false;
                    break;
            }
        }

        // Return true only if items, checkpoints, and bool requirement are all satisfied
        return itemsDone && checkpointsDone && boolRequirementDone;
    }


    private void StartQuestInitialDialog()
    {
        DialogSystem.Instance.OpenDialogUI();
        npcDialogText.text = currentActiveQuest.info.initialDialog[currentDialog];
        optionButton1Text.text = "Next";
        optionButton1.onClick.RemoveAllListeners();
        optionButton1.onClick.AddListener(() =>
        {
            currentDialog++;
            CheckIfDialogDone();
        });
        if (!disableOption2)
            optionButton2.gameObject.SetActive(false);

    }

    private void CheckIfDialogDone()
    {
        if (currentDialog >= currentActiveQuest.info.initialDialog.Count - 1)
        {
            npcDialogText.text = currentActiveQuest.info.initialDialog[currentDialog];
            currentActiveQuest.initialDialogCompleted = true;
            SetAcceptAndDeclineOptions();
        }
        else
        {
            npcDialogText.text = currentActiveQuest.info.initialDialog[currentDialog];
            optionButton1Text.text = "Next";
            optionButton1.onClick.RemoveAllListeners();
            optionButton1.onClick.AddListener(() =>
            {
                currentDialog++;
                CheckIfDialogDone();
            });
        }
    }

    private void AcceptedQuest()
    {
        QuestManager.Instance.AddActiveQuest(currentActiveQuest);
        currentActiveQuest.accepted = true;
        currentActiveQuest.declined = false;

        if (currentActiveQuest.hasNoRequirements)
        {
            npcDialogText.text = currentActiveQuest.info.comebackCompleted;
            optionButton1Text.text = "Take Reward";
            optionButton1.onClick.RemoveAllListeners();
            optionButton1.onClick.AddListener(() => ReceiveRewardAndCompleteQuest());
            if (!disableOption2)
                optionButton2.gameObject.SetActive(false);

        }
        else
        {
            npcDialogText.text = currentActiveQuest.info.acceptAnswer;
            CloseDialogUI();
        }
    }

    public void CloseDialogUI()
    {

        optionButton1Text.text = "Close";
        optionButton1.onClick.RemoveAllListeners();
        optionButton1.onClick.AddListener(() =>
        {
            DialogSystem.Instance.CloseDialogUI();
            isTalkingWithPlayer = false;
            DialogSystem.Instance.HideAllPortraits();

            // --- Play HeadMaster video only after closing dialog ---
            QuestManager.Instance.PlayHeadMasterVideoAfterDialog();
        });
        if (!disableOption2)
            optionButton2.gameObject.SetActive(false);



    }

    private void ReceiveRewardAndCompleteQuest()
    {
        QuestManager.Instance.MarkQuestCompleted(currentActiveQuest);

        if (!string.IsNullOrEmpty(currentActiveQuest.info.rewardItem1))
            InventorySystem.Instance.AddToInventory(currentActiveQuest.info.rewardItem1);

        if (!string.IsNullOrEmpty(currentActiveQuest.info.rewardItem2))
            InventorySystem.Instance.AddToInventory(currentActiveQuest.info.rewardItem2);

        activeQuestIndex++;
        if (activeQuestIndex < quests.Count)
        {
            currentActiveQuest = quests[activeQuestIndex];
            currentDialog = 0;
            DialogSystem.Instance.CloseDialogUI();
            isTalkingWithPlayer = false;
        }
        else
        {
            DialogSystem.Instance.CloseDialogUI();
            isTalkingWithPlayer = false;
        }
    }

    private void DeclinedQuest()
    {
        currentActiveQuest.declined = true;
        npcDialogText.text = currentActiveQuest.info.declineAnswer;
        CloseDialogUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DialogSystem.Instance.HideAllPortraits();
            playerInRange = false;
            MainScreen.SetActive(true);
            if (pressFUI != null) pressFUI.SetActive(false);
            isTalkingWithPlayer = false;
            if (dialogUI != null && dialogUI.activeSelf)
                dialogUI.SetActive(false);
        }
    }
}
