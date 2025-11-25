using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableNPC : MonoBehaviour
{
    public bool playerInRange;

    // Reference to the specific NPC this interactable is linked to
    public NPC npc;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.interactableNpcDetected = true;
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.interactableNpcDetected = false;
            playerInRange = false;

            if (npc != null)
            {
                npc.CloseDialogUI();
            }
        }
    }
}
    