using UnityEngine;

public class TestJumpHatBarrier : MonoBehaviour
{
    private GameTracker gt;

    private void Start()
    {
        // Find the GameTracker in the scene
        gt = FindObjectOfType<GameTracker>();
        if (gt == null)
        {
            Debug.LogError("GameTracker not found in the scene!");
        }
    }

    // ⭐ CALL THIS FROM JumpHatMinigame AFTER SUCCESS ⭐
    public void CaptureJumpHat()
    {
        if (gt == null)
        {
            Debug.LogWarning("GameTracker not found when calling CaptureJumpHat.");
            return;
        }

        Debug.Log("JumpHat captured (triggered by minigame).");

        // Record capture in GameTracker
        gt.CaptureHat("JumpHat");

        // Disable or remove the hat / barrier
        gameObject.SetActive(false);
        // Or: Destroy(gameObject);
    }
}
