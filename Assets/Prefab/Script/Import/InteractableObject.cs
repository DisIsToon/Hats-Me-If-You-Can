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
        // CHANGE INPUT INTO MOUSE CLICK FOR 1ST POV
        if ((Input.GetKeyDown(KeyCode.E) && playerInRange && canBePickedUp && !isPickingUp))
        {
            StartCoroutine(PickupItem());
        }
    }

    private IEnumerator PickupItem()
    {

        isPickingUp = true;

        // If inventory is NOT full, then pick up the item
        if (InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            NotifUIManager.Instance.NotifyItemPicked(ItemName);
            //SoundManager.Instance.PlaySound(SoundManager.Instance.pickUpSound);
            InventorySystem.Instance.AddToInventory(ItemName);

            // Delay before destroying the item to ensure pickup process is completed
            yield return new WaitForSeconds(0.1f);

            SelectionManager.Instance.pickableItemDetected = false;
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventory is full");

            // Reset the flag if the inventory is full
            isPickingUp = false;
        }
    }

    public string GetItemName()
    {
        return ItemName;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.pickableItemDetected = true;
            playerInRange = true;
            canBePickedUp = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.pickableItemDetected = false;
            playerInRange = false;
            canBePickedUp = false;
        }
    }

    private void OnDestroy()
    {
        // Ensure the flag is reset when the item is destroyed
        isPickingUp = false;
    }
    
}
