using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractablePuzzle : MonoBehaviour
{
    public bool playerInRange;
    public bool isOpen;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.puzzleMirrorDetected = true;
            playerInRange = true;
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.puzzleMirrorDetected = false;
            playerInRange = false;
            isOpen = false;

        }
    }

    private void Update()
    {
        // Player presses E while inside trigger
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && isOpen)
        {
            SelectionManager.Instance.puzzleMirrorDetected = false;
            playerInRange = false;
        }
    }
}
