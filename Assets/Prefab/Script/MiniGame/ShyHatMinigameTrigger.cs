using UnityEngine;

public class ShyHatMinigameTrigger : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [Tooltip("UI panel that contains the ShyHatMinigame script.")]
    public GameObject minigamePanel;

    [Header("Detection")]
    [Tooltip("Tag of objects that can trigger the minigame (e.g. 'Throwable').")]
    public string throwableTag = "Throwable";

    [Tooltip("If true, minigame can only be triggered once.")]
    public bool oneTimeOnly = true;

    private bool hasTriggered = false;

    void Start()
    {
        if (minigamePanel != null)
            minigamePanel.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckHit(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        CheckHit(other.gameObject);
    }

    void CheckHit(GameObject hitObject)
    {
        if (hasTriggered && oneTimeOnly)
            return;

        if (minigamePanel == null) return;

        // ✅ Check by tag instead of exact reference
        if (hitObject.CompareTag(throwableTag))
        {
            hasTriggered = true;
            Debug.Log("ShyHatMinigameTrigger: Hit by throwable, opening minigame.");
            minigamePanel.SetActive(true);
        }
    }
}
