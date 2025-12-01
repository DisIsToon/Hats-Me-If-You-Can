using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get;set;}
    public bool onTarget;

    public GameObject mainScreen;
    public GameObject inventoryButton;
    public GameObject quickslotUI;
    public GameObject selectedObject;
    public GameObject interaction_Info_UI;
    public GameObject helpPickUpDetectUI;
    public GameObject helpPickUpNotesUI;
    public GameObject helpInteractCauldronUI;
    public GameObject helpInteractNpcUI;
    public GameObject helpTalkUI;
    public GameObject playRotatingPuzzleUI;

    public TextMeshProUGUI npcNameText;
    public NPC currentDetectedNPC;   // Assigned by InteractableNPC

    public Image centerDotImage;
    public Image handIcon;

    public bool pickableItemDetected;
    public bool readableNoteDetected;
    public bool cauldronDetected;
    public bool puzzleDetected;
    public bool interactableNpcDetected;


    public bool handIsVisible;
    public bool willTalk;

    bool anyNpcTalking = false;
    
    private CraftingSystem craftingSystem; // Reference to the CraftingSystemScript

    private void Start()
    {
        onTarget = false;
        craftingSystem = FindObjectOfType<CraftingSystem>(); // Find the CraftingSystemScript in the scene

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    private void Awake()
    {
        if (Instance != null&& Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
 
    void Update()
    {
        anyNpcTalking = NPC.allNPCs.Exists(n => n.isTalkingWithPlayer);

        foreach (var npc in NPC.allNPCs)
        {
            if (npc.isTalkingWithPlayer)
            {
                anyNpcTalking = true;
                break;
            }
        }

        if (DialogSystem.Instance == null)
            Debug.LogWarning("DialogSystem.Instance is NULL!");
        if (CraftingSystem.Instance == null)
            Debug.LogWarning("CraftingSystem.Instance is NULL!");
        if (InventorySystem.Instance == null)
            Debug.LogWarning("InventorySystem.Instance is NULL!");
        if (QuestManager.Instance == null)
            Debug.LogWarning("QuestManager.Instance is NULL!");
        if (PuzzleManagerUI.Instance == null)
            Debug.LogWarning("PuzzleManagerUI.Instance is NULL!");
        if (CardsController.Instance == null)
            Debug.LogWarning("CardsController.Instance is NULL!");
        if (NewHatalougeManager.Instance == null)
            Debug.LogWarning("HatalougeManager.Instance is NULL!");

        if (DialogSystem.Instance.dialogUIActive == true ||
            CraftingSystem.Instance.isOpen == true ||
            QuestManager.Instance.isQuestMenuOpen == true ||
            PuzzleManagerUI.Instance.isOpen == true ||
            CardsController.Instance.isOpen == true ||
            anyNpcTalking ||
            Notes.Instance.activeNote == true ||
            NewHatalougeManager.Instance.isOpen == true)
        {
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
            mainScreen.SetActive(false);
            inventoryButton.SetActive(false);
            quickslotUI.SetActive(false);

        }
        else
        {
            //Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            quickslotUI.SetActive(true);
            if(InventorySystem.Instance.isOpen==false)
            {
                inventoryButton.SetActive(true);
                mainScreen.SetActive(true);
            }

        }

        // Item
        if (pickableItemDetected == true)
        {
            helpPickUpDetectUI.SetActive(true);

        }
        else if(pickableItemDetected == false)
        {
            helpPickUpDetectUI.SetActive(false);
        }

        // Notes
        if (readableNoteDetected == true)
        {
            helpPickUpNotesUI.SetActive(true);

        }
        else if (readableNoteDetected == false)
        {
            helpPickUpNotesUI.SetActive(false);
        }

        // Cauldron
        if (cauldronDetected == true)
        {
            helpInteractCauldronUI.SetActive(true);

        }
        else if (cauldronDetected == false)
        {
            helpInteractCauldronUI.SetActive(false);
        }

        // Puzzle Rotating
        if (puzzleDetected == true)
        {
            playRotatingPuzzleUI.SetActive(true);

        }
        else if (puzzleDetected == false)
        {
            playRotatingPuzzleUI.SetActive(false);
        }

        //NPC
        if (interactableNpcDetected)
        {
            if (currentDetectedNPC != null)
            {
                string npcName = currentDetectedNPC.npcName; // Get name directly from NPC

                // Update the dialog system name
                DialogSystem.Instance.SetSpeakerName(npcName);

                // Update your UI label
                npcNameText.text = npcName;
            }

            helpInteractNpcUI.SetActive(true);
        }
        else
        {
            npcNameText.text = "";
            helpInteractNpcUI.SetActive(false);
        }
    }

    public void DisableSelection()
    {
        handIcon.enabled = false;
        centerDotImage.enabled = false;
        interaction_Info_UI.SetActive(false);

        selectedObject = null;
    }
    public void EnableSelection()
    {
        handIcon.enabled = true;
        centerDotImage.enabled = true;
        interaction_Info_UI.SetActive(true);
    }
}
