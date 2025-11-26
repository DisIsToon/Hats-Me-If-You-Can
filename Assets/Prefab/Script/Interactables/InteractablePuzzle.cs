using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractablePuzzle : MonoBehaviour
{
    public bool playerInRange;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.puzzleDetected = true;
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SelectionManager.Instance.puzzleDetected = false;
            playerInRange = false;
            PuzzleManagerUI.Instance.PuzzleScreenOff();

        }
    }
}
