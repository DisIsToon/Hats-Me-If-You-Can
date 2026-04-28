using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    public bool playerInRange;
    public GameObject helpPickUpDetectUI;
    public string ItemName;
    public bool canBePickedUp = false;

    // Global flag to track if an item is being picked up
    private static bool isPickingUp = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) &&
            playerInRange &&
            canBePickedUp &&
            !isPickingUp &&
            SelectionManager.Instance.selectedObject == gameObject)

        {
            isPickingUp = true;
            StartCoroutine(PickupItem());
        }
    }

    private IEnumerator PickupItem()
    {
        // Always go to inventory now
        if (InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            

            NotifUIManager.Instance.NotifyItemPicked(ItemName);
            //SoundManager.Instance.PlaySound(SoundManager.Instance.pickUpSound);
            InventorySystem.Instance.AddToInventory(ItemName);

            // Optional pickup sound here
            // SoundManager.Instance.PlaySound(SoundManager.Instance.pickUpSound);

            // Small delay for safety
            yield return new WaitForSeconds(0.1f);

            if (SelectionManager.Instance != null)
                SelectionManager.Instance.pickableItemDetected = false;
            SoundManager.Instance.PlaySFX(SoundManager.Instance.itemCollectSound.clip);
            Destroy(gameObject);

            // Check if item is LostHat
            if (ItemName == "Lost Hat")
            {
                foreach (NPC npc in NPC.allNPCs)
                {
                    if (npc.npcName == "Tulip")
                    {
                        npc.StartConversation();
                        //break;
                    }
                }
            }

            // Check if item is LostHat
            if (ItemName == "Luminshroom")
            {
                foreach (NPC npc in NPC.allNPCs)
                {
                    if (npc.npcName == "Mallow")
                    {
                        npc.StartConversation();
                        //break;
                    }
                }
            }
        }
        else
        {
            Debug.Log("Inventory is full");
            isPickingUp = false;
        }
    }

    public string GetItemName()
    {
        return ItemName;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!playerInRange && other.CompareTag("Player"))
        {
            SelectionManager.Instance.pickableItemDetected = true;
            SelectionManager.Instance.selectedObject = gameObject; 
            playerInRange = true;
            canBePickedUp = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.pickableItemDetected = false;
            SelectionManager.Instance.selectedObject = null; //
            playerInRange = false;
            canBePickedUp = false;
        }
    }

    private void OnDestroy()
    {
        isPickingUp = false;
    }
}
