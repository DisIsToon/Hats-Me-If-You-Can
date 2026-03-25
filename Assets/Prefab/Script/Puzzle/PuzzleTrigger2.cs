using UnityEngine;

public class PuzzleTrigger2 : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (CardsController.Instance != null)
            {
                CardsController.Instance.SetCanPlayPuzzle(true);
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (CardsController.Instance == null) return;

            //  NEW: block exit during transition
            if (CardsController.Instance.isTransitioning)
            {
                // Debug.Log("[Trigger] Exit ignored (transitioning)");
                return;
            }

            if (!CardsController.Instance.isOpen) return;

            // Debug.Log("[Trigger] Player exited puzzle area");

            CardsController.Instance.SetCanPlayPuzzle(false);
            CardsController.Instance.PuzzleScreenOff();
        }
    }
}
