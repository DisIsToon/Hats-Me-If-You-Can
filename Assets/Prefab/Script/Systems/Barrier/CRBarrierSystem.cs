using UnityEngine;
using UnityEngine.UI;
public class CRBarrierSystem : MonoBehaviour
{

    [Header("References")]
    public GameObject barrierObject;        // The physical barrier (wall, collider, etc.)
    public GameObject messageUI;            // The UI text or panel (e.g. "A force is stopping you")

    [Header("Settings")]
    public bool barrierRemoved = false;     // This will be set true externally when condition is met

    private bool playerInRange = false;

    void Start()
    {
        // Make sure message UI is hidden initially
        if (messageUI != null)
            messageUI.SetActive(false);

        // Make sure barrier is active at start
        if (barrierObject != null)
            barrierObject.SetActive(true);
    }
        
    void Update()
    {
        // When the bool becomes true, remove the barrier
        if (barrierRemoved && barrierObject.activeSelf)
        {
            barrierObject.SetActive(false);
            if (messageUI != null)
                messageUI.SetActive(false);
        }

        // Optionally hide the message if player walks away
        if (!playerInRange && messageUI.activeSelf)
            messageUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player Collided");
            playerInRange = true;

            // Only show message if barrier still exists
            if (!barrierRemoved && messageUI != null)
                messageUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (messageUI != null)
                messageUI.SetActive(false);
        }
    }
}

