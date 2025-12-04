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
    public bool canTriggerMultipleTimes = false;
    public string throwableTag = "Throwable";

    [Header("Hat Root (hidden after minigame ends)")]
    [Tooltip("Main hat object to hide when minigame ends. If empty, uses this GameObject.")]
    public GameObject hatRoot;

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
            TriggerSequence();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(throwableTag))
            TriggerSequence();
    }

    void TriggerSequence()
    {
        if (hasTriggered && !canTriggerMultipleTimes)
            return;

        hasTriggered = true;

        // 🔹 Stop hat jumping when hit
        if (hatAI != null)
            hatAI.enabled = false;

        // 🔹 Switch to CM_JumpHat and track the hat
        FocusHatCamera();

        // 🔹 Start minigame after a small delay
        if (pendingStart != null)
            StopCoroutine(pendingStart);

        pendingStart = StartCoroutine(StartMinigameAfterDelay());
    }

    void FocusHatCamera()
    {
        if (jumpHatCamera == null)
            return;

        // ✅ CM3: use Follow + LookAt (Inspector "Tracking Target")
        jumpHatCamera.Follow = hatFocusTarget;
        jumpHatCamera.LookAt = hatFocusTarget;

        if (playerCamera != null)
        {
            // Priority-based switching
            playerCamera.Priority = 5;
            jumpHatCamera.Priority = 20;
        }
        else
        {
            // Fallback if you're not using priorities
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

        if (jumpHatMinigameUI != null && !jumpHatMinigameUI.activeSelf)
            jumpHatMinigameUI.SetActive(true); // JumpHatMinigame.OnEnable()
    }

    // Called by JumpHatMinigame when it ends
    public void ReleaseCameraFocus()
    {
        // 🔹 Return control to player camera
        if (playerCamera != null && jumpHatCamera != null)
        {
            playerCamera.Priority = 20;
            jumpHatCamera.Priority = 5;
        }
        else if (jumpHatCamera != null)
        {
            jumpHatCamera.gameObject.SetActive(false);
        }

        // 🔹 Hide the hat after the encounter
        if (hatRoot != null)
            hatRoot.SetActive(false);
        else
            gameObject.SetActive(false);
    }
}
