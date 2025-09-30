using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get;set;}
    public bool onTarget;

    public GameObject selectedObject;
    public GameObject interaction_Info_UI;
    public GameObject helpPickUpDetectUI;
    public GameObject helpTalkUI;

    public Image centerDotImage;
    public Image handIcon;

    public bool pickableItemDetected;
    public bool handIsVisible;
    public bool willTalk;

    public GameObject selectedTree;
    public GameObject selectedStone;
    public GameObject selectedIron;
    public GameObject selectedCoal;
    public GameObject selectedEnemy;
    public GameObject selectedHumanEnemy;
    public GameObject selectedGoblinEnemy;
    public GameObject selectedAnimal;
    public GameObject selectedCampFire;


    private CraftingSystem craftingSystem; // Reference to the CraftingSystemScript

    private void Start()
    {
        onTarget = false;
        craftingSystem = FindObjectOfType<CraftingSystem>(); // Find the CraftingSystemScript in the scene
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
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selectionTransform = hit.transform;

            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();

            // ...... NPC ...... \\
            NPC npc = selectionTransform.GetComponent<NPC>();

            if (npc && npc.playerInRange)
            {
                interaction_Info_UI.SetActive(true);
                helpTalkUI.SetActive(true);
                willTalk = true; 


                if(DialogSystem.Instance.dialogUIActive)
                {
                    interaction_Info_UI.SetActive(false);
                    centerDotImage.gameObject.SetActive(false);

                }
            }
            else
            {
                interaction_Info_UI.SetActive(false);
                helpTalkUI.SetActive(false);
                willTalk = false;
            }

            // interactbale \\
           if(pickableItemDetected == true)
            {
                helpPickUpDetectUI.SetActive(true);

            }
            else if(pickableItemDetected == false)
            {
                helpPickUpDetectUI.SetActive(false);
            }
          
               /*
                if (interactable && interactable.playerInRange)
                {
                    onTarget = true;
                    selectedObject = interactable.gameObject; 

                    interaction_text.text = interactable.GetItemName();
                    interaction_Info_UI.SetActive(true);
                    helpPickUpUI.SetActive(true);

                    if(interactable.CompareTag("pickable"))
                    {
                        centerDotImage.gameObject.SetActive(false);
                        handIcon.gameObject.SetActive(true);

                        handIsVisible = true;
                    }
                    else
                    {
                        handIcon.gameObject.SetActive(false); 
                        centerDotImage.gameObject.SetActive(true);
                
                        handIsVisible = false;
                    }
                    }
               */
            else 
            {
                onTarget = false;
                // interaction_Info_UI.SetActive(false);
                handIcon.gameObject.SetActive(false); 
                centerDotImage.gameObject.SetActive(true);
                handIsVisible = false;
                willTalk = false;
            }
            /* // ...... CampFire ...... \\
            Campfire interactableCampfire = selectionTransform.GetComponent<Campfire>();

            if (interactableCampfire && interactableCampfire.playerInRange)
            {
                selectedCampFire = interactableCampfire.gameObject;
                campfireInteractionUIHolder.gameObject.SetActive(true);

                // Check if the player presses the 'F' key to open the food crafting UI
                if (Input.GetKeyDown(KeyCode.F))
                {
                    craftingSystem.ToggleFoodScreen(); // Toggle the food screen UI when pressing F
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    DisableSelection();
                }
            }
            else
            {
                if (selectedCampFire != null)
                {
                    selectedCampFire = null;
                    campfireInteractionUIHolder.gameObject.SetActive(false);
                    craftingSystem.CloseFoodScreen(); // Close the food screen UI if the campfire is not selected
                    Cursor.lockState = CursorLockMode.Locked; // Lock the cursor again
                    Cursor.visible = false; // Hide the cursor again
                    EnableSelection(); // Re-enable the selection mechanism
                }
            }*/

        }
        else 
        { 
            onTarget = false;
            interaction_Info_UI.SetActive(false);
            handIcon.gameObject.SetActive(false); 
            centerDotImage.gameObject.SetActive(true);

            handIsVisible = false;
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
