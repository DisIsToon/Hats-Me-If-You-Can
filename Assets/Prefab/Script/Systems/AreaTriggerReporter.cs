using UnityEngine;

public class AreaTriggerReporter : MonoBehaviour
{
    public GameTracker gameTracker; // Assign in Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == gameTracker.player)
        {
            gameTracker.CheckPlayerInTrigger(this.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == gameTracker.player)
        {
            Debug.Log("Player exited " + this.gameObject.name);
        }
    }
}
