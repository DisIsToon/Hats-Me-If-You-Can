using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InteractableCauldron : MonoBehaviour
{
    public bool playerInRange;
    public bool isOpen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.cauldronDetected = true;
            playerInRange = true;
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CraftingSystem.Instance.CraftingScreenOff();
            SelectionManager.Instance.cauldronDetected = false;
            playerInRange = false;
            isOpen = true;

        }
    }

    private void Update()
    {
        // Player presses E while inside trigger
        if (playerInRange && Input.GetKeyDown(KeyCode.E) &&isOpen)
        {
            CraftingSystem.Instance.CraftingScreenOff();
            SelectionManager.Instance.cauldronDetected = false;
            playerInRange = false;
        }
    }
}
