using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    [Header("References")]
    public GameObject pressFUI;   // UI prompt ("Press F")
    public GameObject dialogUI;   // Dialog panel UI
    public GameObject MainScreen;

    [Header("NPC & Quest Data")]
    public List<Quest> quests;
    public Quest currentActiveQuest = null;
    public int activeQuestIndex = 0;
    public bool firstTimeInteraction = true;
    public int currentDialog;

    public bool playerInRange = false;
    public bool isTalkingWithPlayer = false;

    // UI References
    private TextMeshProUGUI npcDialogText;
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
        // Initialize references from DialogSystem
        if (DialogSystem.Instance != null)
        {
            npcDialogText = DialogSystem.Instance.dialogText;
            optionButton1 = DialogSystem.Instance.option1BTN;
            optionButton1Text = DialogSystem.Instance.option1BTN.transform.Find("Text(TMP)").GetComponent<TextMeshProUGUI>();
            optionButton2 = DialogSystem.Instance.option2BTN;
            optionButton2Text = DialogSystem.Instance.option2BTN.transform.Find("Text(TMP)").GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("DialogSystem.Instance is null. Make sure DialogSystem exists in the scene.");
        }

        if (pressFUI != null) pressFUI.SetActive(false);
        if (dialogUI != null) dialogUI.SetActive(false);

        // Find player automatically
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null) player = found.transform;
        else Debug.LogWarning("NPCInteraction: No GameObject with tag 'Player' found.");
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

    public void StartConversation()
    {
        isTalkingWithPlayer = true;

        if (firstTimeInteraction)
        {
            firstTimeInteraction = false;
            currentActiveQuest = quests[activeQuestIndex]; // first quest
            StartQuestInitialDialog();
            currentDialog = 0;
        }
        else
        {
            if (currentActiveQuest.declined)
            {
                DialogSystem.Instance.OpenDialogUI();
                npcDialogText.text = currentActiveQuest.info.comebackAfterDecline;
                SetAcceptAndDeclineOptions();
            }

            if (currentActiveQuest.accepted && !currentActiveQuest.isCompleted)
            {
                if (AreQuestRequirmentsCompleted())
                {
                    SubmitRequiredItems();
                    DialogSystem.Instance.OpenDialogUI();
                    npcDialogText.text = currentActiveQuest.info.comebackCompleted;
                    optionButton1Text.text = "[Take Reward]";
                    optionButton1.onClick.RemoveAllListeners();
                    optionButton1.onClick.AddListener(() => ReceiveRewardAndCompleteQuest());
                }
                else
                {
                    DialogSystem.Instance.OpenDialogUI();
                    npcDialogText.text = currentActiveQuest.info.comebackInProgress;
                    optionButton1Text.text = "[Close]";
                    optionButton1.onClick.RemoveAllListeners();
                    optionButton1.onClick.AddListener(() =>
                    {
                        DialogSystem.Instance.CloseDialogUI();
                        isTalkingWithPlayer = false;
                    });
                }
            }

            if (currentActiveQuest.isCompleted)
            {
                DialogSystem.Instance.OpenDialogUI();
                npcDialogText.text = currentActiveQuest.info.finalWords;
                optionButton1Text.text = "[Close]";
                optionButton1.onClick.RemoveAllListeners();
                optionButton1.onClick.AddListener(() =>
                {
                    DialogSystem.Instance.CloseDialogUI();
                    isTalkingWithPlayer = false;
                });
            }

            if (!currentActiveQuest.initialDialogCompleted)
            {
                StartQuestInitialDialog();
            }
        }
    }

    private void SetAcceptAndDeclineOptions()
    {
        optionButton1Text.text = currentActiveQuest.info.acceptOption;
        optionButton1.onClick.RemoveAllListeners();
        optionButton1.onClick.AddListener(AcceptedQuest);

        optionButton2.gameObject.SetActive(true);
        optionButton2Text.text = currentActiveQuest.info.declineOption;
        optionButton2.onClick.RemoveAllListeners();
        optionButton2.onClick.AddListener(DeclinedQuest);
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
        int firstCount = InventorySystem.Instance.CheckItemAmount(currentActiveQuest.info.firstRequirmentItem);
        int secondCount = InventorySystem.Instance.CheckItemAmount(currentActiveQuest.info.secondRequirmentItem);

        bool itemsDone = firstCount >= currentActiveQuest.info.firstRequirementAmount &&
                         secondCount >= currentActiveQuest.info.secondRequirementAmount;

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

        return itemsDone && checkpointsDone;
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
            optionButton1Text.text = "[Take Reward]";
            optionButton1.onClick.RemoveAllListeners();
            optionButton1.onClick.AddListener(() => ReceiveRewardAndCompleteQuest());
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
        optionButton1Text.text = "[Close]";
        optionButton1.onClick.RemoveAllListeners();
        optionButton1.onClick.AddListener(() =>
        {
            DialogSystem.Instance.CloseDialogUI();
            isTalkingWithPlayer = false;
        });
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
            playerInRange = false;
            MainScreen.SetActive(true);
            if (pressFUI != null) pressFUI.SetActive(false);
            isTalkingWithPlayer = false;
            if (dialogUI != null && dialogUI.activeSelf)
                dialogUI.SetActive(false);
        }
    }
}
