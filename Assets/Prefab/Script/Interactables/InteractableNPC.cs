using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableNPC : MonoBehaviour
{
    public bool playerInRange;
    public bool isOpen;
    // Reference to the specific NPC this interactable is linked to
    public NPC npc;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.interactableNpcDetected = true;
            SelectionManager.Instance.currentDetectedNPC = npc;   // ★ Pass NPC here
            playerInRange = true;
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.interactableNpcDetected = false;
            SelectionManager.Instance.currentDetectedNPC = null;  // ★ Remove NPC
            playerInRange = false;
            isOpen = false;
            if (npc != null)
            {
                npc.CloseDialogUI();
            }
        }
    }

    private void Update()
    {
        // Player presses E while inside trigger
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && isOpen)
        {
            SelectionManager.Instance.interactableNpcDetected = false;
            SelectionManager.Instance.currentDetectedNPC = null;  // ★ Remove NPC
            playerInRange = false;

            if (npc != null)
            {
                Debug.Log("npc is null");
                //npc.CloseDialogUI();
            }
        }
    }
}
    