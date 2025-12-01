using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rigidbody-based character with sprint, crouch, double jump, stamina,
/// NPC interaction, and data persistence.
/// Throwing / item logic has been moved to the separate `Throw` component.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class RigidbodyPlayerWithSprintAndStamina : MonoBehaviour, IDataPersistence
{
    #region Inspector - Movement
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
    public float jumpForce = 8f;
    public float crouchSpeed = 3f;

    [Header("Stamina Settings")]
    public float maxStamina = 5f;
    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.3f;
    public LayerMask groundMask;
    [Tooltip("Extra grace time after leaving ground where jump still counts.")]
    public float coyoteTime = 0.1f;

    [Header("NPC Interaction")]
    public NPC currentInteractingNPC;
    #endregion

    #region Inspector - Animation
    [Header("Animation")]
    public Animator animator;
    public string speedParam = "Speed";
    public string crouchParam = "IsCrouching";

    [Header("Animation Tuning")]
    [Tooltip("Speeds below this are treated as 0 (prevents jitter causing walk).")]
    public float speedDeadzone = 0.15f;
    [Tooltip("Multiplies the speed value sent to the Animator to hit thresholds.")]
    public float speedMultiplier = 2.0f;
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
    #endregion

    #region Unity - Init
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        currentStamina = maxStamina;

        originalHeight = col.height;
        originalCenter = col.center;
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
        HandleGroundCheck();
        HandleCrouch();
        HandleJump();
        HandleFallBoost();
        HandleStamina();

        // --- Animator movement speed (with dead-zone + scaling) ---
        if (animator)
        {
            Vector3 flatVel = rb.linearVelocity;
            flatVel.y = 0f;
            float s = flatVel.magnitude;

            // Dead-zone to prevent micro movement from triggering Walk
            if (s < speedDeadzone) s = 0f;

            // Scale up so it reliably crosses thresholds in transitions/blend trees
            s *= speedMultiplier;

            animator.SetFloat(speedParam, s);
        }
    }

    void FixedUpdate()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float speed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : moveSpeed);

        // Camera-relative movement
        Vector3 forward = cameraTransform ? cameraTransform.forward : transform.forward;
        Vector3 right = cameraTransform ? cameraTransform.right : transform.right;
        forward.y = 0; right.y = 0;
        forward.Normalize(); right.Normalize();

        Vector3 move = (right * x + forward * z);
        if (move.sqrMagnitude > 1f) move.Normalize();

        Vector3 targetVel = move * speed;
        Vector3 planarVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 velChange = targetVel - planarVel;

        rb.AddForce(velChange, ForceMode.VelocityChange);

        if (shouldFaceMoveDirection && move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
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
            isGrounded = true;
            lastGroundedTime = Time.time;
            jumpCount = 0;
        }
        else
        {
            isGrounded = (Time.time - lastGroundedTime) <= coyoteTime;
        }
    }

    void HandleCrouch()
    {
        isCrouching = Input.GetKey(KeyCode.C);
        if (animator) animator.SetBool(crouchParam, isCrouching);

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
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCount++;
        }
    }

    void HandleFallBoost()
    {
        if (!isGrounded && rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector3.up * Physics.gravity.y * 3f * Time.deltaTime;
    }

    void HandleStamina()
    {
        Vector3 flatVel = rb.linearVelocity; flatVel.y = 0;
        bool moving = flatVel.sqrMagnitude > 0.01f;

        isSprinting = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && !isCrouching;
        if (isSprinting && moving)
            currentStamina = Mathf.Max(0, currentStamina - staminaDrainRate * Time.deltaTime);
        else if (!isSprinting && isGrounded)
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
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
}
