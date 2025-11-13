using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InteractableCauldron : MonoBehaviour
{
    public bool playerInRange;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.cauldronDetected = true;
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.cauldronDetected = false;
            playerInRange = false;
            CraftingSystem.Instance.CraftingScreenOff();

        }
    }
}
