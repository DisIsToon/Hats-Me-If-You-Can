using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableNotes : MonoBehaviour
{
    public bool playerInRange;
    public bool isOpen;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (SelectionManager.Instance != null)
                SelectionManager.Instance.readableNoteDetected = true;
            isOpen = true;
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (SelectionManager.Instance != null)
                SelectionManager.Instance.readableNoteDetected = false;
            isOpen = false;
            playerInRange = false;

            // SAFETY CHECK
            if (Notes.Instance != null)
                Notes.Instance.CloseDialog();
        }
    }

    private void Update()
    {
        // Player presses E while inside trigger
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && isOpen)
        {
            if (SelectionManager.Instance != null)
                SelectionManager.Instance.readableNoteDetected = false;

            playerInRange = false;

            // SAFETY CHECK
            if (Notes.Instance != null)
                Notes.Instance.CloseDialog();
        }
    }
}
