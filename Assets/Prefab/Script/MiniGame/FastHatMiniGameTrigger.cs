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
    public float startMinigameDelay = 2f;

    [Header("Trigger Settings")]
    [Tooltip("Tag used by your throwable object.")]
    public string throwableTag = "Throwable";

    [Header("Hat Root (hidden on success)")]
    [Tooltip("Hat GameObject to hide when minigame ends successfully. If empty, uses this GameObject.")]
    public GameObject hatRoot;

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
        {
            Debug.Log("FastHatMinigameTrigger: OnCollisionEnter with throwable");
            TriggerSequence();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(throwableTag))
        {
            Debug.Log("FastHatMinigameTrigger: OnTriggerEnter with throwable");
            TriggerSequence();
        }
    }

    void TriggerSequence()
    {
        Debug.Log("FastHatMinigameTrigger: TriggerSequence called.");

        // 1) STOP THE HAT MOVEMENT IMMEDIATELY
        if (fastHatAI != null)
        {
            Debug.Log("FastHatMinigameTrigger: Disabling FastHatAI.");
            fastHatAI.enabled = false;
        }

        // 2) Switch camera to Fast Hat and focus on it
        FocusHatCamera();

        // 3) Start minigame after a delay
        if (pendingStart != null)
            StopCoroutine(pendingStart);

        pendingStart = StartCoroutine(StartMinigameAfterDelay());
    }

    void FocusHatCamera()
    {
        if (fastHatCamera == null)
        {
            Debug.LogWarning("FastHatMinigameTrigger: fastHatCamera not assigned.");
            return;
        }

        // Follow + LookAt same as Jump Hat
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

        if (fastHatMinigameUI != null)
        {
            if (!fastHatMinigameUI.activeSelf)
            {
                Debug.Log("FastHatMinigameTrigger: Activating FastHatMinigame UI.");
                fastHatMinigameUI.SetActive(true);   // triggers FastHatMinigame.OnEnable()
            }
        }
        else
        {
            Debug.LogWarning("FastHatMinigameTrigger: fastHatMinigameUI is not assigned.");
        }
    }

    // Called by FastHatMinigame when it ends
    public void OnFastHatMinigameEnd(bool success)
    {
        Debug.Log("FastHatMinigameTrigger: OnFastHatMinigameEnd(" + success + ")");

        // 1) Camera back to player
        if (playerCamera != null && fastHatCamera != null)
        {
            playerCamera.Priority = 20;
            fastHatCamera.Priority = 5;
        }
        else if (fastHatCamera != null)
        {
            fastHatCamera.gameObject.SetActive(false);
        }

        // 2) Handle success vs fail
        if (success)
        {
            // ✅ SUCCESS: hat disappears, no more catching
            if (hatRoot != null)
                hatRoot.SetActive(false);
        }
        else
        {
            // ❌ FAIL: give another chance
            Debug.Log("FastHatMinigameTrigger: Minigame failed, re-enabling FastHatAI for another try.");

            // Turn AI back on so it starts running again
            if (fastHatAI != null)
                fastHatAI.enabled = true;
        }
    }
}
