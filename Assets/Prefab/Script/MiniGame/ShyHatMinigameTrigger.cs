using UnityEngine;
using Unity.Cinemachine;   // Cinemachine 3
using System.Collections;

public class ShyHatMinigameTrigger : MonoBehaviour
{
    [Header("Minigame UI")]
    [Tooltip("UI panel that contains the ShyHatMinigame script.")]
    public GameObject minigamePanel;

    [Header("Detection")]
    [Tooltip("Tag of objects that can trigger the minigame (e.g. 'Throwable').")]
    public string throwableTag = "CakePotion";

    [Header("Shy Hat AI")]
    [Tooltip("Assign the ShyHatAI script here so it stops moving when hit.")]
    public MonoBehaviour shyHatAI;  // e.g. ShyHatAI

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

    // gating controlled by AI
    private bool canTrigger = false;

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
        if (!hitObject.CompareTag(throwableTag))
            return;

        if (requireEnableFromAI && !canTrigger)
        {
            // AI hasn't allowed capture yet (e.g., not at final spot)
            Debug.Log("ShyHatMinigameTrigger: Hit, but capture not enabled by AI yet.");
            return;
        }

        Debug.Log("ShyHatMinigameTrigger: Hit by throwable, stopping AI and starting minigame.");

        // 1) Stop Shy Hat movement immediately
        if (shyHatAI != null)
        {
            Debug.Log("ShyHatMinigameTrigger: Disabling ShyHatAI.");
            shyHatAI.enabled = false;
        }

        // 2) Focus camera on THIS hat
        if (focusCamera != null)
        {
            focusCamera.Follow = transform;
            focusCamera.LookAt = transform;
            focusCamera.Priority = focusPriority;
        }

        // 3) Start minigame after a short delay (camera settles)
        if (minigamePanel != null)
        {
            StartCoroutine(StartMinigameAfterDelay());
        }
        else
        {
            Debug.LogWarning("ShyHatMinigameTrigger: minigamePanel is not assigned.");
        }
    }

    IEnumerator StartMinigameAfterDelay()
    {
        // Wait in unscaled time so pausing Time.timeScale later doesn't matter
        float elapsed = 0f;
        while (elapsed < minigameStartDelay)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.Log("ShyHatMinigameTrigger: Activating ShyHatMinigame UI.");
        minigamePanel.SetActive(true); // ShyHatMinigame.OnEnable()
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
    /// Called by ShyHatMinigame when the minigame ends to restore the camera
    /// and optionally re-enable AI on fail.
    /// </summary>
    public void OnShyHatMinigameEnd(bool success)
    {
        Debug.Log("ShyHatMinigameTrigger: OnShyHatMinigameEnd(" + success + ")");

        // Restore camera
        if (focusCamera != null)
        {
            focusCamera.Priority = originalPriority;
            focusCamera.Follow = originalFollow;
            focusCamera.LookAt = originalLookAt;
        }

        if (success)
        {
            // SUCCESS: ShyHatBarrier handles disabling / capture via TestShyHatBarrier
            // No need to re-enable AI here; hat is effectively done.
        }
        else
        {
            // FAIL: give another chance → re-enable AI so Shy Hat can move again
            Debug.Log("ShyHatMinigameTrigger: Minigame failed, re-enabling ShyHatAI for another try.");

            if (shyHatAI != null)
                shyHatAI.enabled = true;

            // Keep canTrigger = true so you can hit it again without re-waiting,
            // or leave capture logic to AI if you want it to re-gate.
        }
    }
}
