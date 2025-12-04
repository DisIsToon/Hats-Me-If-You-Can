using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableNotes : MonoBehaviour
{
    public bool playerInRange;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (SelectionManager.Instance != null)
                SelectionManager.Instance.readableNoteDetected = true;

            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
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
