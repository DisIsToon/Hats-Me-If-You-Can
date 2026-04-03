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

    [Header("Reaction System")]
    public AIReactionController aiReactionController;
    public PlayerReactionController playerReactionController;

    [Header("Timing")]
    [Tooltip("Delay after camera focuses on hat before minigame UI appears.")]
    public float startMinigameDelay = 0.6f;

    [Header("Trigger Settings")]
    [Tooltip("Tag used by your throwable object.")]
    public string throwableTag = "CottonCraze";

    [Header("Hat Root (hidden on success)")]
    [Tooltip("Main hat object to hide when minigame ends successfully. If empty, uses this GameObject.")]
    public GameObject hatRoot;

    private Coroutine pendingStart;
    private bool hasTriggered;

    void Awake()
    {
        if (hatFocusTarget == null)
            hatFocusTarget = transform;

        if (hatRoot == null)
            hatRoot = gameObject;

        if (aiReactionController == null)
            aiReactionController = GetComponent<AIReactionController>();

        if (playerReactionController == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerReactionController = playerObj.GetComponent<PlayerReactionController>();
        }
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
        if (hasTriggered)
            return;

        hasTriggered = true;

        Debug.Log("JumpHatMinigameTrigger: TriggerSequence called.");

        // Reactions
        if (playerReactionController != null)
            playerReactionController.ShowHitReaction(true);

        if (aiReactionController != null)
            aiReactionController.ShowGettingHitReaction(true);

        // 1) Stop hat movement immediately
        if (hatAI != null)
        {
            Debug.Log("JumpHatMinigameTrigger: Disabling JumpHatAI.");
            hatAI.enabled = false;
        }

        // 2) Switch camera to JumpHat vcam and focus on it
        FocusHatCamera();

        // 3) Restart delayed minigame safely
        if (pendingStart != null)
        {
            StopCoroutine(pendingStart);
            pendingStart = null;
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayHatCaptureMusic();

        pendingStart = StartCoroutine(StartMinigameAfterDelay());
    }

    void FocusHatCamera()
    {
        if (jumpHatCamera == null)
        {
            Debug.LogWarning("JumpHatMinigameTrigger: jumpHatCamera not assigned.");
            return;
        }

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
                jumpHatMinigameUI.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("JumpHatMinigameTrigger: jumpHatMinigameUI is not assigned.");
        }
    }

    // Called by JumpHatMinigame when it ends
    public void OnJumpHatMinigameEnd(bool success)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.ReturnToBiomeMusic();

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
            if (aiReactionController != null)
                aiReactionController.ShowDifferentReaction(true);

            if (hatRoot != null)
                hatRoot.SetActive(false);
            else
                gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("JumpHatMinigameTrigger: Minigame failed, re-enabling JumpHatAI for another try.");

            hasTriggered = false;

            if (hatAI != null)
                hatAI.enabled = true;

            if (aiReactionController != null)
                aiReactionController.ShowIdleReaction(true);
        }
    }
}