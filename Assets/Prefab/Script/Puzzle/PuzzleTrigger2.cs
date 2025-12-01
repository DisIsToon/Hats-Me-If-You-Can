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
            if (CardsController.Instance != null)
            {
                CardsController.Instance.SetCanPlayPuzzle(false);
            }

        }
    }
}
