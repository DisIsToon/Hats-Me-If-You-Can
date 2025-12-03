using UnityEngine;
using Unity.Cinemachine;   // Cinemachine 3

public class ShyHatMinigameTrigger : MonoBehaviour
{
    [Header("Minigame UI")]
    [Tooltip("UI panel that contains the ShyHatMinigame script.")]
    public GameObject minigamePanel;

    [Header("Detection")]
    [Tooltip("Tag of objects that can trigger the minigame (e.g. 'Throwable').")]
    public string throwableTag = "Throwable";

    [Tooltip("If true, minigame can only be triggered once.")]
    public bool oneTimeOnly = true;

    [Header("Cinemachine Focus")]
    [Tooltip("Cinemachine 3 camera that should focus on this hat when the minigame starts.")]
    public CinemachineCamera focusCamera;

    [Tooltip("Priority while focusing on the hat. Must be higher than your normal gameplay camera.")]
    public int focusPriority = 20;

    [Header("Start Timing")]
    [Tooltip("How long to wait after switching cameras before showing the minigame panel.")]
    public float minigameStartDelay = 0.5f;

    [Header("Capture Gating")]
    [Tooltip("If true, the AI must call EnableCapture() before this trigger will work.")]
    public bool requireEnableFromAI = true;

    private bool hasTriggered = false;
    private bool canTrigger = false;   // set by AI

    // original camera state so we can restore it
    private int originalPriority;
    private Transform originalFollow;
    private Transform originalLookAt;

    void Start()
    {
        if (minigamePanel != null)
            minigamePanel.SetActive(false);

        if (focusCamera != null)
        {
            originalPriority = focusCamera.Priority;
            originalFollow = focusCamera.Follow;
            originalLookAt = focusCamera.LookAt;
        }
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

        if (!hitObject.CompareTag(throwableTag))
            return;

        if (requireEnableFromAI && !canTrigger)
        {
            // Hat not at the final point yet
            return;
        }

        hasTriggered = true;
        Debug.Log("ShyHatMinigameTrigger: Hit by throwable, focusing camera and starting minigame.");

        // Focus camera on THIS hat
        if (focusCamera != null)
        {
            focusCamera.Follow = transform;
            focusCamera.LookAt = transform;
            focusCamera.Priority = focusPriority;
        }

        // Start minigame after a short delay (camera settles)
        if (minigamePanel != null)
        {
            StartCoroutine(StartMinigameAfterDelay());
        }
    }

    System.Collections.IEnumerator StartMinigameAfterDelay()
    {
        // Wait in unscaled time so pausing Time.timeScale later doesn't matter
        float elapsed = 0f;
        while (elapsed < minigameStartDelay)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        minigamePanel.SetActive(true);
    }

    /// <summary>
    /// Called by the AI when the hat reaches / leaves the final point.
    /// </summary>
    public void EnableCapture()
    {
        canTrigger = true;
    }

    public void DisableCapture()
    {
        canTrigger = false;
    }

    /// <summary>
    /// Called by ShyHatMinigame when the minigame ends to restore the camera.
    /// </summary>
    public void ReleaseCameraFocus()
    {
        if (focusCamera != null)
        {
            focusCamera.Priority = originalPriority;
            focusCamera.Follow = originalFollow;
            focusCamera.LookAt = originalLookAt;
        }
    }
}
