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

    [Header("Reaction System")]
    [Tooltip("Reaction controller on this hat.")]
    public AIReactionController aiReactionController;

    [Tooltip("Reaction controller on the player.")]
    public PlayerReactionController playerReactionController;

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

    // prevent double triggering
    private bool hasTriggered = false;

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

        if (aiReactionController == null)
            aiReactionController = GetComponent<AIReactionController>();

        if (playerReactionController == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerReactionController = playerObj.GetComponent<PlayerReactionController>();
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
        if (hasTriggered)
            return;

        if (!hitObject.CompareTag(throwableTag))
            return;

        PotionReactionTracker potionTracker = hitObject.GetComponent<PotionReactionTracker>();

        if (requireEnableFromAI && !canTrigger)
        {
            Debug.Log("ShyHatMinigameTrigger: Hit, but capture not enabled by AI yet.");

            // It hit, but this was NOT a valid minigame hit
            if (potionTracker != null)
                potionTracker.ShowMissReaction();

            return;
        }

        hasTriggered = true;

        // Mark potion as valid so it won't show miss on destroy
        if (potionTracker != null)
            potionTracker.MarkValidHit();

        Debug.Log("ShyHatMinigameTrigger: Hit by throwable, stopping AI and starting minigame.");

        if (playerReactionController != null)
            playerReactionController.ShowHitReaction(true);

        if (aiReactionController != null)
            aiReactionController.ShowGettingHitReaction(true);

        if (shyHatAI != null)
        {
            Debug.Log("ShyHatMinigameTrigger: Disabling ShyHatAI.");
            shyHatAI.enabled = false;
        }

        if (focusCamera != null)
        {
            focusCamera.Follow = transform;
            focusCamera.LookAt = transform;
            focusCamera.Priority = focusPriority;
        }

        if (minigamePanel != null)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayHatCaptureMusic();

            StartCoroutine(StartMinigameAfterDelay());
        }
        else
        {
            Debug.LogWarning("ShyHatMinigameTrigger: minigamePanel is not assigned.");
        }
    }

    IEnumerator StartMinigameAfterDelay()
    {
        float elapsed = 0f;
        while (elapsed < minigameStartDelay)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.Log("ShyHatMinigameTrigger: Activating ShyHatMinigame UI.");
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
    /// Called by ShyHatMinigame when the minigame ends to restore the camera
    /// and optionally re-enable AI on fail.
    /// </summary>
    public void OnShyHatMinigameEnd(bool success)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.ReturnToBiomeMusic();

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
            // On success, optional different reaction
            if (aiReactionController != null)
                aiReactionController.ShowDifferentReaction(true);

            // Hat is considered captured / done
        }
        else
        {
            Debug.Log("ShyHatMinigameTrigger: Minigame failed, re-enabling ShyHatAI for another try.");

            hasTriggered = false;

            if (shyHatAI != null)
                shyHatAI.enabled = true;

            // Optional fail reaction
            if (aiReactionController != null)
                aiReactionController.ShowIdleReaction(true);
        }
    }
}