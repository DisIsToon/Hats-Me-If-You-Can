using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class FastHatMinigameTrigger : MonoBehaviour
{
    [Header("Minigame UI Root")]
    [Tooltip("The GameObject that holds the FastHatMinigame script (usually a panel or canvas child).")]
    public GameObject fastHatMinigameUI;

    [Header("Cinemachine 3 Cameras")]
    [Tooltip("Your normal gameplay CinemachineCamera.")]
    public CinemachineCamera playerCamera;

    [Tooltip("CinemachineCamera that focuses on the Fast Hat (can reuse CM_JumpHat).")]
    public CinemachineCamera fastHatCamera;

    [Header("Hat Focus Target")]
    [Tooltip("Transform the camera should follow/look at (usually a child above the hat).")]
    public Transform hatFocusTarget;

    [Header("Fast Hat AI")]
    [Tooltip("FastHatAI script controlling movement between points.")]
    public FastHatAI fastHatAI;

    [Header("Timing")]
    [Tooltip("Delay after focusing on hat before minigame UI appears.")]
    public float startMinigameDelay = 0.6f;

    [Header("Trigger Settings")]
    public bool canTriggerMultipleTimes = false;
    public string throwableTag = "Throwable";

    [Header("Hat Root (hidden or kept after minigame)")]
    [Tooltip("Hat GameObject to hide when minigame ends (on success). If empty, uses this GameObject.")]
    public GameObject hatRoot;

    [Tooltip("Hide the hat only if minigame was successful.")]
    public bool hideOnlyOnSuccess = true;

    bool hasTriggered = false;
    Coroutine pendingStart;

    void Awake()
    {
        if (hatFocusTarget == null)
            hatFocusTarget = transform;

        if (hatRoot == null)
            hatRoot = gameObject;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(throwableTag))
            TryStartMinigame();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(throwableTag))
            TryStartMinigame();
    }

    void TryStartMinigame()
    {
        if (hasTriggered && !canTriggerMultipleTimes)
            return;

        // Only allow catching during stop window
        if (fastHatAI != null && !fastHatAI.isInStopWindow)
        {
            Debug.Log("Tried to hit Fast Hat, but not in stop window.");
            return;
        }

        hasTriggered = true;

        // Stop AI so he doesn't run away
        if (fastHatAI != null)
            fastHatAI.enabled = false;

        FocusHatCamera();

        if (pendingStart != null)
            StopCoroutine(pendingStart);

        pendingStart = StartCoroutine(StartMinigameAfterDelay());
    }

    void FocusHatCamera()
    {
        if (fastHatCamera == null)
            return;

        // Same pattern as Jump Hat: Follow + LookAt
        fastHatCamera.Follow = hatFocusTarget;
        fastHatCamera.LookAt = hatFocusTarget;

        if (playerCamera != null)
        {
            playerCamera.Priority = 5;
            fastHatCamera.Priority = 20;
        }
        else
        {
            fastHatCamera.gameObject.SetActive(true);
        }
    }

    IEnumerator StartMinigameAfterDelay()
    {
        float t = 0f;
        while (t < startMinigameDelay)
        {
            t += Time.deltaTime;
            yield return null;
        }

        if (fastHatMinigameUI != null && !fastHatMinigameUI.activeSelf)
            fastHatMinigameUI.SetActive(true);   // triggers FastHatMinigame.OnEnable()
    }

    // Called by FastHatMinigame when it ends
    public void OnFastHatMinigameEnd(bool success)
    {
        // Camera back to player
        if (playerCamera != null && fastHatCamera != null)
        {
            playerCamera.Priority = 20;
            fastHatCamera.Priority = 5;
        }
        else if (fastHatCamera != null)
        {
            fastHatCamera.gameObject.SetActive(false);
        }

        // Hat visibility behavior
        if (hatRoot != null)
        {
            if (success && hideOnlyOnSuccess)
            {
                hatRoot.SetActive(false);
            }
            else if (!hideOnlyOnSuccess)
            {
                // Always hide on end
                hatRoot.SetActive(false);
            }
            else
            {
                // If we only hide on success and it failed, optionally re-enable AI
                if (fastHatAI != null)
                    fastHatAI.enabled = true;
            }
        }
    }
}
