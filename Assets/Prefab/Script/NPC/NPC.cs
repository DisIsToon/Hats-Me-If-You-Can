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
    public GameObject QuickSlotScreen;
    public GameObject InventoryBTN;
    public bool isOpen;
    public bool questDone = false;


    [Header("NPC Name")]
    public string npcName;

    [Header("Save ID")]
    public string npcID;

    [Header("Simple NPC Settings")]
    public bool isSimpleNPC = false;
    public bool isSnowmanNPC = false;
    private bool hasTalkedSimpleNPC = false;

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
    public Button nextButton;
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
        if (GameDataManager2.Instance != null && GameDataManager2.Instance.currentData != null)
        {
            LoadData(GameDataManager2.Instance.currentData);
        }

        if (DialogSystem.Instance == null)
        {
            Debug.LogError("DialogSystem.Instance is NULL! Make sure DialogSystem exists in the scene.");
            return;
        }

        npcDialogText = DialogSystem.Instance.dialogText;

        nextButton = DialogSystem.Instance.nextBTN;
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
        DialogSystem.Instance.ResetUI();

        // Find player
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null) player = found.transform;

        foreach (var q in quests)
        {
            if (q.info == null)
            {
                Debug.LogError($"{npcName}: A quest is missing QuestInfo!");
            }
        }
    }

    private bool ValidateCurrentQuest()
    {
        if (quests == null || quests.Count == 0)
        {
            Debug.LogError($"NPC {npcName}: No quests assigned!");
            return false;
        }

        if (activeQuestIndex < 0 || activeQuestIndex >= quests.Count)
        {
            Debug.LogError($"NPC {npcName}: activeQuestIndex out of range!");
            return false;
        }

        currentActiveQuest = quests[activeQuestIndex];

        if (currentActiveQuest == null)
        {
            Debug.LogError($"NPC {npcName}: currentActiveQuest is NULL!");
            return false;
        }

        if (currentActiveQuest.info == null)
        {
            Debug.LogError($"NPC {npcName}: Quest INFO is NULL");
            return false;
        }

        // STEP 5 SAFETY
        if (currentActiveQuest.info.initialDialog == null || currentActiveQuest.info.initialDialog.Count == 0)
        {
            Debug.LogWarning($"NPC {npcName}: initialDialog missing — auto-fixing");
            currentActiveQuest.info.initialDialog = new List<string> { "..." };
        }

        return true;
    }


    public void ValidateAllNPCs()
    {
        foreach (var npc in NPC.allNPCs)
        {
            if (npc.quests == null || npc.quests.Count == 0)
            {
                Debug.LogWarning($"NPC {npc.npcName} missing quests");
                continue;
            }

            foreach (var q in npc.quests)
            {
                if (q.info == null)
                    Debug.LogWarning($"NPC {npc.npcName} has quest missing info");
            }
        }
    }

    void Update()
    {
        if (!playerInRange) return;

        if (pressFUI != null && (dialogUI == null || !dialogUI.activeSelf))
            pressFUI.SetActive(true);

        if (Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            SelectionManager.Instance.interactableNpcDetected = false;
            StartConversation();
            if (pressFUI != null) pressFUI.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.E) && isOpen)
        {
            isOpen = false;
            CloseDialogUI();
        }

        // -------------------------------------------------
        // SPACE KEY = NEXT BUTTON
        // -------------------------------------------------
        if (dialogUI != null && dialogUI.activeSelf)
        {
            // Only trigger if next button is visible
            if (nextButton != null && nextButton.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    nextButton.onClick.Invoke();
                }
            }
        }
    }

    public void SaveData(GameData2 data)
    {
        if (string.IsNullOrEmpty(npcID))
        {
            Debug.LogError($"NPC '{npcName}' has NO npcID! Cannot save.");
            return;
        }

        if (data.npcData == null)
            data.npcData = new SerializableDictionary<string, GameData2.NPCSaveData>();

        if (!data.npcData.ContainsKey(npcID))
            data.npcData[npcID] = new GameData2.NPCSaveData();

        var npcSave = data.npcData[npcID];

        npcSave.firstTimeInteraction = firstTimeInteraction;
        npcSave.questDone = questDone;
        npcSave.activeQuestIndex = activeQuestIndex;

        if (currentActiveQuest != null)
        {
            npcSave.accepted = currentActiveQuest.accepted;
            npcSave.completed = currentActiveQuest.isCompleted;
        }
    }

    public void LoadData(GameData2 data)
    {
        if (string.IsNullOrEmpty(npcID))
        {
            Debug.LogError($"NPC '{npcName}' has NO npcID! Cannot load.");
            return;
        }

        if (data == null || data.npcData == null)
            return;

        if (!data.npcData.ContainsKey(npcID))
            return;

        var npcSave = data.npcData[npcID];

        firstTimeInteraction = npcSave.firstTimeInteraction;
        questDone = npcSave.questDone;
        activeQuestIndex = npcSave.activeQuestIndex;

        // SAFETY CHECK
        if (quests == null || quests.Count == 0)
        {
            currentActiveQuest = null;
            return;
        }

        // Clamp index to avoid crashes
        activeQuestIndex = Mathf.Clamp(activeQuestIndex, 0, quests.Count - 1);

        currentActiveQuest = quests[activeQuestIndex];
        currentActiveQuest.accepted = npcSave.accepted;
        currentActiveQuest.isCompleted = npcSave.completed;
    }

    private void HideOption2()
    {
        if (optionButton2 != null)
            optionButton2.gameObject.SetActive(false);
    }

    public void CompleteLiraQuest()
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
        MainScreen.SetActive(false);
        InventoryBTN.SetActive(false);
        QuickSlotScreen.SetActive(false);

        DialogSystem.Instance.ShowNPCImage(npcName);

        //  GET NAME FROM DIALOG SYSTEM (optional but you asked for it)
        string speakerName = DialogSystem.Instance.GetSpeakerName();

        if (isSimpleNPC)
        {
            StartSimpleDialogue();
            return;
        }

        if(isSnowmanNPC)
        {
            StartSnowmanDialogue();
            return;
        }
        // --- First-time interaction ---
        if (firstTimeInteraction)
        {
            if (!ValidateCurrentQuest())
            {
                Debug.LogWarning($"NPC {npcName}: Quest validation failed. Dialogue blocked.");
                return;
            }

            // Check bool requirement
            if (!CheckBoolRequirement(currentActiveQuest.info))
            {
                ShowIncompleteRequirementMessage();
                return;
            }

            firstTimeInteraction = false;
            if (GameDataManager2.Instance != null)
            {
                SaveData(GameDataManager2.Instance.currentData);
            }
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
                if(questDone == false)
                {
                    SubmitRequiredItems();
                    DialogSystem.Instance.OpenDialogUI();
                    npcDialogText.text = currentActiveQuest.info.comebackCompleted;

                    // --- Reward button uses Option1 ---
                    optionButton1.gameObject.SetActive(true);
                    optionButton1Text.text = "Take Reward";
                    optionButton1.onClick.RemoveAllListeners();
                    optionButton1.onClick.AddListener(() => ReceiveRewardAndCompleteQuest());
                    questDone = true;

                    if (!disableOption2)
                        optionButton2.gameObject.SetActive(false);
                    Debug.Log("AreQuestRequirmentsCompleted 1111111111111");
                }
                else
                {
                    DialogSystem.Instance.OpenDialogUI();
                    npcDialogText.text = currentActiveQuest.info.finalWords;

                    // --- Reward button uses Option1 ---
                    optionButton1.gameObject.SetActive(true);
                    optionButton1Text.text = "Close";
                    optionButton1.onClick.RemoveAllListeners();
                    optionButton1.onClick.AddListener(() =>
                    {
                        DialogSystem.Instance.CloseDialogUI();
                        isTalkingWithPlayer = false;
                    });
                    if (!disableOption2)
                        optionButton2.gameObject.SetActive(false);

                    Debug.Log("AreQuestRequirmentsCompleted 22222222222");
                }
            }
            else
            {
                Debug.Log("AreQuestRequirmentsCompleted 3333333333");
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

    private void StartSimpleDialogue()
    {
        DialogSystem.Instance.OpenDialogUI();

        // If already talked → go straight to final words
        if (hasTalkedSimpleNPC)
        {
            npcDialogText.text = quests[0].info.finalWords;

            optionButton1.gameObject.SetActive(true);
            optionButton1Text.text = "Close";
            optionButton1.onClick.RemoveAllListeners();
            optionButton1.onClick.AddListener(() =>
            {
                DialogSystem.Instance.CloseDialogUI();
                isTalkingWithPlayer = false;
            });

            optionButton2.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);

            return;
        }

        // FIRST TIME DIALOGUE
        currentDialog = 0;
        npcDialogText.text = quests[0].info.initialDialog[currentDialog];

        optionButton1.gameObject.SetActive(false);
        optionButton2.gameObject.SetActive(false);

        nextButton.gameObject.SetActive(true);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            currentDialog++;

            if (currentDialog < quests[0].info.initialDialog.Count)
            {
                npcDialogText.text = quests[0].info.initialDialog[currentDialog];
            }
            else
            {
                // Mark as talked AFTER finishing dialogue
                hasTalkedSimpleNPC = true;

                npcDialogText.text = quests[0].info.finalWords;

                nextButton.gameObject.SetActive(false);

                optionButton1.gameObject.SetActive(true);
                optionButton1Text.text = "Close";
                optionButton1.onClick.RemoveAllListeners();
                optionButton1.onClick.AddListener(() =>
                {
                    DialogSystem.Instance.CloseDialogUI();
                    isTalkingWithPlayer = false;
                });
            }
        });
    }

    private void StartSnowmanDialogue()
    {
        DialogSystem.Instance.OpenDialogUI();

        // If already talked → go straight to final words
        if (hasTalkedSimpleNPC)
        {
            npcDialogText.text = quests[0].info.finalWords;

            optionButton1.gameObject.SetActive(true);
            optionButton1Text.text = "Close";
            optionButton1.onClick.RemoveAllListeners();
            optionButton1.onClick.AddListener(() =>
            {
                DialogSystem.Instance.CloseDialogUI();
                isTalkingWithPlayer = false;
            });

            optionButton2.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(false);

            return;
        }

        // FIRST TIME DIALOGUE
        currentDialog = 0;
        npcDialogText.text = quests[0].info.initialDialog[currentDialog];

        optionButton1.gameObject.SetActive(false);
        optionButton2.gameObject.SetActive(false);

        nextButton.gameObject.SetActive(true);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            currentDialog++;

            if (currentDialog < quests[0].info.initialDialog.Count)
            {
                npcDialogText.text = quests[0].info.initialDialog[currentDialog];
            }
            else
            {
                npcDialogText.text = quests[0].info.finalWords;

                nextButton.gameObject.SetActive(false);

                optionButton1.gameObject.SetActive(true);
                optionButton1Text.text = "Close";
                optionButton1.onClick.RemoveAllListeners();
                optionButton1.onClick.AddListener(() =>
                {
                    DialogSystem.Instance.CloseDialogUI();
                    isTalkingWithPlayer = false;
                });
            }
        });
    }

    // --- Helper function for showing "incomplete requirement" message ---
    public void ShowIncompleteRequirementMessage()
    {
        DialogSystem.Instance.OpenDialogUI();
        npcDialogText.text = "Hmm… something’s missing. Come back when you've handled that puzzle.";

        // Hide Option buttons
        optionButton1.gameObject.SetActive(false);
        optionButton2.gameObject.SetActive(false);

        // Show Next button to close dialogue
        nextButton.gameObject.SetActive(true);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            DialogSystem.Instance.CloseDialogUI();
            isTalkingWithPlayer = false;
        });
        if (!disableOption2)
            optionButton2.gameObject.SetActive(false);

    }


    public void SetAcceptAndDeclineOptions()
    {
        if (optionButton1 != null) optionButton1.gameObject.SetActive(true);
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
        if (currentActiveQuest == null)
        {
            Debug.LogError($"NPC {npcName}: currentActiveQuest is NULL");
            return;
        }

        if (currentActiveQuest.info == null)
        {
            Debug.LogError($"NPC {npcName}: Quest info is NULL");
            return;
        }

        // STEP 5 SAFETY FIX (corrected typo here)
        if (currentActiveQuest.info.initialDialog == null || currentActiveQuest.info.initialDialog.Count == 0)
        {
            Debug.LogError($"NPC {npcName}: initialDialog is EMPTY");
            currentActiveQuest.info.initialDialog = new List<string> { "..." };
        }

        if (currentDialog >= currentActiveQuest.info.initialDialog.Count)
        {
            Debug.LogError($"NPC {npcName}: currentDialog index OUT OF RANGE");
            currentDialog = 0;
        }

        DialogSystem.Instance.OpenDialogUI();

        currentDialog = Mathf.Clamp(currentDialog, 0, currentActiveQuest.info.initialDialog.Count - 1);

        npcDialogText.text = currentActiveQuest.info.initialDialog[currentDialog];

        optionButton1.gameObject.SetActive(false);
        optionButton2.gameObject.SetActive(false);

        DialogSystem.Instance.ResetUI();
        DialogSystem.Instance.nextBTN.onClick.RemoveAllListeners();
        DialogSystem.Instance.nextBTN.onClick.AddListener(() =>
        {
            currentDialog++;
            CheckIfDialogDone();
        });
    }

    private void CheckIfDialogDone()
    {
        if (currentDialog >= currentActiveQuest.info.initialDialog.Count) {
            // Dialogue finished: hide Next, show Accept/Decline
            DialogSystem.Instance.nextBTN.gameObject.SetActive(false);
            currentActiveQuest.initialDialogCompleted = true;

            DialogSystem.Instance.ShowChoices();

            SetAcceptAndDeclineOptions();
        } else {
            // Continue dialogue
            npcDialogText.text = currentActiveQuest.info.initialDialog[currentDialog];
        }
    }

    private void AcceptedQuest()
    {
        QuestManager.Instance.AddActiveQuest(currentActiveQuest);
        currentActiveQuest.accepted = true;
        currentActiveQuest.declined = false;

        if (GameDataManager2.Instance != null)
        {
            SaveData(GameDataManager2.Instance.currentData);
        }

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

        DialogSystem.Instance.ResetUI();
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(() =>
        {
            DialogSystem.Instance.CloseDialogUI();
            isTalkingWithPlayer = false;
            DialogSystem.Instance.HideAllPortraits();

            // --- Play HeadMaster video only after closing dialog ---
            QuestManager.Instance.PlayHeadMasterVideoAfterDialog();
        });
        if (!disableOption2)
            optionButton2.gameObject.SetActive(false);

        MainScreen.SetActive(true);
        QuickSlotScreen.SetActive(true);
        InventoryBTN.SetActive(true);

    }

    private void ReceiveRewardAndCompleteQuest()
    {
        StartCoroutine(QuestManager.Instance.MarkQuestCompleted(currentActiveQuest));

        if (GameDataManager2.Instance != null)
        {
            SaveData(GameDataManager2.Instance.currentData);
        }

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
            CloseDialogUI();
        }
        else
        {
            DialogSystem.Instance.CloseDialogUI();
            isTalkingWithPlayer = false;
            CloseDialogUI();
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
            QuickSlotScreen.SetActive(true);
            InventoryBTN.SetActive(true);
            if (pressFUI != null) pressFUI.SetActive(false);
            isTalkingWithPlayer = false;
            if (dialogUI != null && dialogUI.activeSelf)
                dialogUI.SetActive(false);
        }
    }
}
