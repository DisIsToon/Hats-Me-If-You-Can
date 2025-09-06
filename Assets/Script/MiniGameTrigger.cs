using UnityEngine;

public class MiniGameTrigger : MonoBehaviour
{
    public GameObject miniGamePanel; // Assign in Inspector

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Throwable"))
        {
            // Show the mini game panel
            miniGamePanel.SetActive(true);

            // Optional: Pause the game
            Time.timeScale = 0f;
        }
    }
}
