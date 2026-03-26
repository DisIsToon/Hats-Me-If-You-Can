using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rigidbody-based character with sprint, crouch, double jump, stamina,
/// NPC interaction, and data persistence.
/// Animation is driven via parameters:
/// - Speed (float)
/// - IsCrouching (bool)
/// - IsGrounded (bool)
/// - IsSprinting (bool)
/// - Jump (trigger)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class RigidbodyPlayerWithSprintAndStamina : MonoBehaviour, IDataPersistence
{
    #region Inspector - Movement
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 11f;
    public float jumpForce = 8f;
    public float crouchSpeed = 3f;

    [Header("Movement Smoothing")]
    [Tooltip("How quickly the player accelerates to target speed.")]
    public float acceleration = 55f;
    [Tooltip("How quickly the player slows down when releasing input.")]
    public float deceleration = 40f;
    [Tooltip("Multiplier for acceleration/deceleration while in the air.")]
    public float airControlMultiplier = 0.8f;
    #endregion

    #region Inspector - Stamina
    [Header("Stamina Settings")]
    public float maxStamina = 6f;
    public float staminaDrainRate = 0.8f;
    public float staminaRegenRate = 2.5f;
    #endregion

    #region Inspector - Ground Check
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundMask;
    [Tooltip("Extra grace time after leaving ground where jump still counts.")]
    public float coyoteTime = 0.1f;
    #endregion

    #region Inspector - NPC Interaction
    [Header("NPC Interaction")]
    public NPC currentInteractingNPC;
    #endregion

    #region Inspector - Animation
    [Header("Animation")]
    public Animator animator;

    [Tooltip("Float parameter for movement speed in the Animator.")]
    public string speedParam = "Speed";
    [Tooltip("Bool parameter for crouching in the Animator.")]
    public string crouchParam = "IsCrouching";
    [Tooltip("Bool parameter for grounded state in the Animator.")]
    public string groundedParam = "IsGrounded";
    [Tooltip("Bool parameter for sprint/run state in the Animator.")]
    public string sprintParam = "IsSprinting";
    [Tooltip("Trigger parameter for jump in the Animator.")]
    public string jumpTriggerParam = "Jump";

    [Header("Animation Tuning")]
    [Tooltip("Speeds below this are treated as 0 (prevents jitter causing walk).")]
    public float speedDeadzone = 0.1f;
    [Tooltip("Multiplies the speed value sent to the Animator to hit thresholds.")]
    public float speedMultiplier = 3.0f;
    #endregion

    #region Inspector - Camera
    [Header("References")]
    public Transform cameraTransform;
    public bool shouldFaceMoveDirection = true;
    #endregion

    #region Private Cached
    Rigidbody rb;
    CapsuleCollider col;
    #endregion

    #region Private State
    float originalHeight;
    Vector3 originalCenter;

    bool isGrounded;
    bool isSprinting;
    bool isCrouching;
    int jumpCount;
    const int maxJumps = 2;
    float currentStamina;
    float lastGroundedTime;
    bool isHidden;

    // Move input cached between Update and FixedUpdate
    Vector2 moveInput;
    #endregion

    #region Unity - Init
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        currentStamina = maxStamina;

        originalHeight = col.height;
        originalCenter = col.center;

        // Prevent physics tipping the player over
        rb.freezeRotation = true;
    }
    #endregion

    #region Data Persistence
    public void LoadData(GameData data)
    {
        transform.position = data.playerPosition;
    }

    public void SaveData(GameData data)
    {
        data.playerPosition = transform.position;
    }
    #endregion

    #region Unity - Update
    void Update()
    {
        // UI / menu gating with null checks
        bool uiBlocked = false;
        if (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen) uiBlocked = true;
        if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen) uiBlocked = true;
        if (CardsController.Instance != null && CardsController.Instance.isOpen) uiBlocked = true;
        if (PuzzleManagerUI.Instance != null && PuzzleManagerUI.Instance.isOpen) uiBlocked = true;
        if (NewHatalougeManager.Instance != null && NewHatalougeManager.Instance.isOpen) uiBlocked = true;
        if (DialogSystem.Instance != null && DialogSystem.Instance.dialogUIActive) uiBlocked = true;

        if (uiBlocked)
        {
            moveInput = Vector2.zero;

            // Stop sprinting & crouching while UI is open
            isSprinting = false;
            isCrouching = false;

            return; // Skip the rest of Update
        }

        // 1) Read input
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveInput = Vector2.ClampMagnitude(moveInput, 1f);

        // 2) Ground / crouch / jump / stamina
        HandleGroundCheck();
        HandleCrouch();
        HandleJump();
        HandleFallBoost();
        HandleStamina();

        // 3) Animator parameters
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        float speed =
            isCrouching ? crouchSpeed :
            (isSprinting ? sprintSpeed : moveSpeed);

        // Camera-relative movement
        Vector3 forward = cameraTransform ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform ? cameraTransform.right : transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // x = strafe, y = forward/back
        Vector3 moveDir = right * moveInput.x + forward * moveInput.y;
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        Vector3 targetHorizontalVel = moveDir * speed;

        // Current planar velocity
        Vector3 currentVel = rb.linearVelocity;
        Vector3 currentHorizontalVel = new Vector3(currentVel.x, 0f, currentVel.z);

        // Accel / decel
        float moveAmount = moveDir.sqrMagnitude;
        float accel = (moveAmount > 0.01f) ? acceleration : deceleration;
        if (!isGrounded) accel *= airControlMultiplier;

        Vector3 newHorizontalVel = Vector3.MoveTowards(
            currentHorizontalVel,
            targetHorizontalVel,
            accel * Time.fixedDeltaTime
        );

        // Apply (keep vertical)
        rb.linearVelocity = new Vector3(newHorizontalVel.x, currentVel.y, newHorizontalVel.z);

        // Face move direction
        if (shouldFaceMoveDirection && moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 10f * Time.deltaTime));
        }
    }
    #endregion

    #region Movement Helpers
    void HandleGroundCheck()
    {
        bool groundedNow = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        if (groundedNow)
        {
            if (!isGrounded)
            {
                // Just landed
                jumpCount = 0;
            }

            isGrounded = true;
            lastGroundedTime = Time.time;
        }
        else
        {
            // Coyote time
            isGrounded = (Time.time - lastGroundedTime) <= coyoteTime;
        }
    }

    void HandleCrouch()
    {
        isCrouching = Input.GetKey(KeyCode.C);

        if (isCrouching)
        {
            col.height = Mathf.Max(0.5f, originalHeight * 0.5f);
            col.center = originalCenter - new Vector3(0, originalHeight * 0.25f, 0);
        }
        else
        {
            col.height = originalHeight;
            col.center = originalCenter;
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps && !isCrouching)
        {
            // Reset vertical velocity for consistent jump height
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCount++;

            // Fire jump animation ONLY on first jump
            if (animator && jumpCount == 1 && !string.IsNullOrEmpty(jumpTriggerParam))
            {
                animator.SetTrigger(jumpTriggerParam);
            }
        }
    }

    void HandleFallBoost()
    {
        if (!isGrounded && rb.linearVelocity.y < 0f)
        {
            // Faster fall for snappier jumps
            rb.linearVelocity += Vector3.up * Physics.gravity.y * 3f * Time.deltaTime;
        }
    }

    void HandleStamina()
    {
        Vector3 flatVel = rb.linearVelocity;
        flatVel.y = 0;
        bool moving = flatVel.sqrMagnitude > 0.01f;

        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);

        // Sprint only when: Shift, stamina, not crouching, and moving
        isSprinting = wantsToSprint && currentStamina > 0f && !isCrouching && moving;

        if (isSprinting)
        {
            currentStamina = Mathf.Max(0, currentStamina - staminaDrainRate * Time.deltaTime);
        }
        else if (isGrounded)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
        }
    }
    #endregion

    #region Animator Helper
    void UpdateAnimator()
    {
        if (!animator) return;

        Vector3 flatVel = rb.linearVelocity;
        flatVel.y = 0f;
        float rawSpeed = flatVel.magnitude;

        bool hasInput = moveInput.sqrMagnitude > 0.01f;

        float animSpeed = 0f;

        if (!isGrounded)
        {
            // In air: freeze locomotion speed (jump anim handled by Jump state)
            animSpeed = 0f;
        }
        else
        {
            if (!hasInput)
            {
                // Grounded + no input = idle
                animSpeed = 0f;
            }
            else
            {
                // Grounded + input = walk/run
                animSpeed = rawSpeed * speedMultiplier;
                if (animSpeed < speedDeadzone)
                    animSpeed = speedDeadzone;
            }
        }

        animator.SetFloat(speedParam, animSpeed);
        animator.SetBool(crouchParam, isCrouching);
        animator.SetBool(groundedParam, isGrounded);
        animator.SetBool(sprintParam, isSprinting);
    }
    #endregion

    #region Utils
    public bool IsCrouching() => isCrouching;

    public void SetHidden(bool hidden)
    {
        isHidden = hidden;
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend) rend.material.color = hidden ? Color.gray : Color.white;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
#endif
    #endregion

    #region NPC Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            currentInteractingNPC = other.GetComponent<NPC>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC") && currentInteractingNPC != null)
        {
            currentInteractingNPC = null;
        }
    }
    #endregion
}
