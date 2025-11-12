using UnityEngine;

public class TestJumpHat : MonoBehaviour
{
    private GameTracker gt;

    private void Start()
    {
        // Find the GameTracker in the scene
        gt = FindObjectOfType<GameTracker>();
        if (gt == null)
            Debug.LogError("GameTracker not found in the scene!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && gt != null)
        {
            Debug.Log("Player collided with JumpHat");

            // Record capture in GameTracker
            gt.CaptureHat("JumpHat");

            // Optional: disable or destroy the hat so it can't be collected again
            gameObject.SetActive(false);
            // or: Destroy(gameObject);
        }
    }
}
