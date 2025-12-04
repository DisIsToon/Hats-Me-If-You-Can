using UnityEngine;
using Unity.Cinemachine;   // Cinemachine 3 namespace
using System.Collections;

public class JumpHatMinigameTrigger : MonoBehaviour
{
    [Header("Minigame UI Root")]
    [Tooltip("The GameObject that holds the JumpHatMinigame script (usually a panel or canvas child).")]
    public GameObject jumpHatMinigameUI;

    [Header("Cinemachine 3 Cameras")]
    [Tooltip("Your normal gameplay CinemachineCamera.")]
    public CinemachineCamera playerCamera;

    [Tooltip("CinemachineCamera that focuses on the hat (CM_JumpHat).")]
    public CinemachineCamera jumpHatCamera;

    [Header("Hat Focus Target")]
    [Tooltip("Transform the camera should follow/look at (usually a child above the hat).")]
    public Transform hatFocusTarget;

    [Header("Stop Hat Movement")]
    [Tooltip("Assign the JumpHatAI script here so it stops moving when hit.")]
    public MonoBehaviour hatAI;  // e.g. JumpHatAI

    [Header("Timing")]
    [Tooltip("Delay after camera focuses on hat before minigame UI appears.")]
    public float startMinigameDelay = 0.6f;

    [Header("Trigger Settings")]
    [Tooltip("Tag used by your throwable object.")]
    public string throwableTag = "Throwable";

    [Header("Hat Root (hidden on success)")]
    [Tooltip("Main hat object to hide when minigame ends successfully. If empty, uses this GameObject.")]
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
            Debug.Log("JumpHatMinigameTrigger: OnCollisionEnter with throwable");
            TriggerSequence();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(throwableTag))
        {
            Debug.Log("JumpHatMinigameTrigger: OnTriggerEnter with throwable");
            TriggerSequence();
        }
    }

    void TriggerSequence()
    {
        Debug.Log("JumpHatMinigameTrigger: TriggerSequence called.");

        // 1) Stop hat movement immediately
        if (hatAI != null)
        {
            Debug.Log("JumpHatMinigameTrigger: Disabling JumpHatAI.");
            hatAI.enabled = false;
        }

        // 2) Switch camera to JumpHat vcam and focus on it
        FocusHatCamera();

        // 3) Start minigame after delay
        if (pendingStart != null)
            StopCoroutine(pendingStart);

        pendingStart = StartCoroutine(StartMinigameAfterDelay());
    }

    void FocusHatCamera()
    {
        if (jumpHatCamera == null)
        {
            Debug.LogWarning("JumpHatMinigameTrigger: jumpHatCamera not assigned.");
            return;
        }

        // CM3: Follow + LookAt (same pattern as FastHat)
        jumpHatCamera.Follow = hatFocusTarget;
        jumpHatCamera.LookAt = hatFocusTarget;

        if (playerCamera != null)
        {
            playerCamera.Priority = 5;
            jumpHatCamera.Priority = 20;
        }
        else
        {
            jumpHatCamera.gameObject.SetActive(true);
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

        if (jumpHatMinigameUI != null)
        {
            if (!jumpHatMinigameUI.activeSelf)
            {
                Debug.Log("JumpHatMinigameTrigger: Activating JumpHatMinigame UI.");
                jumpHatMinigameUI.SetActive(true);   // triggers JumpHatMinigame.OnEnable()
            }
        }
        else
        {
            Debug.LogWarning("JumpHatMinigameTrigger: jumpHatMinigameUI is not assigned.");
        }
    }

    // 🔹 Called by JumpHatMinigame when it ends
    public void OnJumpHatMinigameEnd(bool success)
    {
        Debug.Log("JumpHatMinigameTrigger: OnJumpHatMinigameEnd(" + success + ")");

        // 1) Camera back to player
        if (playerCamera != null && jumpHatCamera != null)
        {
            playerCamera.Priority = 20;
            jumpHatCamera.Priority = 5;
        }
        else if (jumpHatCamera != null)
        {
            jumpHatCamera.gameObject.SetActive(false);
        }

        // 2) Handle success vs fail
        if (success)
        {
            // ✅ SUCCESS: hide the hat (capture handled by TestJumpHatBarrier)
            if (hatRoot != null)
                hatRoot.SetActive(false);
            else
                gameObject.SetActive(false);
        }
        else
        {
            // ❌ FAIL: give another chance → re-enable AI
            Debug.Log("JumpHatMinigameTrigger: Minigame failed, re-enabling JumpHatAI for another try.");

            if (hatAI != null)
                hatAI.enabled = true;
        }
    }
}
