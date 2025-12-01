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
        if (Input.GetKeyDown(KeyCode.E) && playerInRange && canBePickedUp && !isPickingUp)
        {
            StartCoroutine(PickupItem());
        }
    }

    private IEnumerator PickupItem()
    {
        isPickingUp = true;

        // Always go to inventory now
        if (InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            InventorySystem.Instance.AddToInventory(ItemName);

            // Optional pickup sound here
            // SoundManager.Instance.PlaySound(SoundManager.Instance.pickUpSound);

            // Small delay for safety
            yield return new WaitForSeconds(0.1f);

            if (SelectionManager.Instance != null)
                SelectionManager.Instance.pickableItemDetected = false;

            Destroy(gameObject);
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
        isPickingUp = false;
    }
}
