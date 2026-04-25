using UnityEngine;

public class PuzzleTrigger2 : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (CardsController.Instance != null)
        {
            CardsController.Instance.SetCanPlayPuzzle(true);
            SelectionManager.Instance.puzzleMirrorDetected = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (CardsController.Instance == null) return;

        // Always disable interaction
        CardsController.Instance.SetCanPlayPuzzle(false);
        SelectionManager.Instance.puzzleMirrorDetected = false;

        // Only close if open and not transitioning
        if (CardsController.Instance.isOpen && !CardsController.Instance.isTransitioning)
        {
            CardsController.Instance.PuzzleScreenOff();
        }
    }
}