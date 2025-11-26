using UnityEngine;

public class PuzzleTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (PuzzleManagerUI.Instance != null)
            {
                PuzzleManagerUI.Instance.SetCanPlayPuzzle(true);
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (PuzzleManagerUI.Instance != null)
            {
                PuzzleManagerUI.Instance.SetCanPlayPuzzle(false);
            }

        }
    }
}
