using UnityEngine;

public class TestShyHatBarrier : MonoBehaviour
{
    private GameTracker gt;

    private void Start()
    {
        // Find the GameTracker in the scene
        gt = FindObjectOfType<GameTracker>();
        if (gt == null)
            Debug.LogError("GameTracker not found in the scene!");
    }

    // ⭐ CALL THIS FROM SHYHATMINIGAME AFTER SUCCESS ⭐
    public void CaptureShyHat()
    {
        if (gt == null)
        {
            Debug.LogWarning("GameTracker not found when calling CaptureShyHat.");
            return;
        }

        Debug.Log("ShyHat captured (triggered by minigame).");

        // Record capture in GameTracker
        gt.CaptureHat("ShyHat");

        // Disable or remove the hat/barrier
        gameObject.SetActive(false);
        // Or Destroy(gameObject);
    }
}
