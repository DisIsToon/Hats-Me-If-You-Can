using UnityEngine;

public class ShyHatAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Flee Points (in order)")]
    public Transform fleePoint1; // start
    public Transform fleePoint2;
    public Transform fleePoint3;
    public Transform fleePoint4;
    public Transform fleePoint5;
    public Transform fleePoint6;
    public Transform fleePoint7; // final stop

    private Transform[] fleePoints;

    [Header("Behavior Settings")]
    public float detectDistance = 6f;      // Distance to trigger moving to NEXT point
    public float moveSpeed = 10f;          // Movement speed
    public float rotationSpeed = 10f;      // Turn speed
    public float arrivalDistance = 0.3f;   // How close counts as "arrived"

    [Header("Options")]
    public bool lockY = true;              // Keep on ground (XZ only)
    public bool snapToFirstPoint = true;   // Snap hat to point 1 at start

    [Header("Minigame / Capture")]
    [Tooltip("Trigger on this hat that handles minigame + camera.")]
    public ShyHatMinigameTrigger minigameTrigger;

    [Tooltip("0-based index of the capture point: 0=point1 ... 6=point7.")]
    public int capturePointIndex = 6;      // 6 = 7th point

    [Header("Animation (visual child)")]
    [Tooltip("Animator on the visual child object (HatVisual).")]
    public Animator animator;

    [Tooltip("Name of the trigger parameter that plays the shocked animation.")]
    public string shockedTriggerName = "Shocked";

    [Tooltip("How long to wait after playing the shocked animation before moving.")]
    public float shockedDelay = 0.4f;

    private int currentIndex = 0;          // Which point we are currently "on"
    private int targetIndex = 0;           // Which point we are moving toward
    private bool isMoving = false;         // Are we currently moving to a point?
    private bool finished = false;         // Reached final point and done
    private bool isPreparingMove = false;  // waiting during shocked anim

    void Start()
    {
        fleePoints = new Transform[]
        {
            fleePoint1,
            fleePoint2,
            fleePoint3,
            fleePoint4,
            fleePoint5,
            fleePoint6,
            fleePoint7
        };

        if (snapToFirstPoint && fleePoints[0] != null)
        {
            transform.position = fleePoints[0].position;
        }

        currentIndex = 0;
        targetIndex = 0;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (minigameTrigger == null)
            minigameTrigger = GetComponent<ShyHatMinigameTrigger>();

        // Try to auto-grab animator from child if not set
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        UpdateCaptureState();
    }

    void Update()
    {
        if (finished) return;
        if (player == null) return;

        // --- Decide to start shocked + move ---
        if (!isMoving && !isPreparingMove && currentIndex < fleePoints.Length - 1)
        {
            float playerDist = DistanceToPlayer();

            if (playerDist <= detectDistance)
            {
                int nextIndex = currentIndex + 1;
                if (fleePoints[nextIndex] != null)
                {
                    StartCoroutine(PlayShockedThenMove(nextIndex));
                }
                else
                {
                    finished = true;
                    return;
                }
            }
        }

        // --- Handle actual movement ---
        if (isMoving)
        {
            Transform targetPoint = fleePoints[targetIndex];
            if (targetPoint == null)
            {
                isMoving = false;
                return;
            }

            Vector3 toTarget = targetPoint.position - transform.position;
            if (lockY) toTarget.y = 0f;

            if (toTarget.magnitude <= arrivalDistance)
            {
                ArriveAtPoint();
                return;
            }

            Vector3 dir = toTarget.normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    System.Collections.IEnumerator PlayShockedThenMove(int nextIndex)
    {
        isPreparingMove = true;

        if (animator != null && !string.IsNullOrEmpty(shockedTriggerName))
        {
            animator.SetTrigger(shockedTriggerName);
        }

        if (shockedDelay > 0f)
            yield return new WaitForSeconds(shockedDelay);

        targetIndex = nextIndex;
        isMoving = true;
        isPreparingMove = false;
    }

    float DistanceToPlayer()
    {
        Vector3 diff = player.position - transform.position;
        if (lockY) diff.y = 0f;
        return diff.magnitude;
    }

    void ArriveAtPoint()
    {
        if (fleePoints[targetIndex] != null)
        {
            transform.position = fleePoints[targetIndex].position;
        }

        currentIndex = targetIndex;
        isMoving = false;

        if (currentIndex >= fleePoints.Length - 1)
        {
            finished = true;
        }

        UpdateCaptureState();
    }

    void UpdateCaptureState()
    {
        if (minigameTrigger == null) return;

        if (currentIndex == capturePointIndex)
        {
            minigameTrigger.EnableCapture();
        }
        else
        {
            minigameTrigger.DisableCapture();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
}
