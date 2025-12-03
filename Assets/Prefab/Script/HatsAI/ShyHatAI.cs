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

    private int currentIndex = 0;          // Which point we are currently "on"
    private int targetIndex = 0;           // Which point we are moving toward
    private bool isMoving = false;         // Are we currently moving to a point?
    private bool finished = false;         // Reached final point and done

    void Start()
    {
        // Put points into array in order
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

        // Optional: snap to first point
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

        // auto-grab trigger on same object if not set
        if (minigameTrigger == null)
            minigameTrigger = GetComponent<ShyHatMinigameTrigger>();

        // At start we are not at capture point, so make sure capture is disabled
        UpdateCaptureState();
    }

    void Update()
    {
        if (finished) return;
        if (player == null) return;

        // --------- TRIGGER MOVEMENT BY PLAYER PROXIMITY ---------
        // Only trigger if we are NOT moving and NOT at final point
        if (!isMoving && currentIndex < fleePoints.Length - 1)
        {
            float playerDist = DistanceToPlayer();

            if (playerDist <= detectDistance)
            {
                // Move to the NEXT point
                targetIndex = currentIndex + 1;

                // Make sure that point exists
                if (fleePoints[targetIndex] != null)
                {
                    isMoving = true;
                }
                else
                {
                    // If it's null, just mark finished so it doesn't hang
                    finished = true;
                    return;
                }
            }
        }

        // --------- HANDLE MOVEMENT IF ACTIVE ---------
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

            // If extremely close, treat as arrived
            if (toTarget.magnitude <= arrivalDistance)
            {
                ArriveAtPoint();
                return;
            }

            Vector3 dir = toTarget.normalized;

            // Move
            transform.position += dir * moveSpeed * Time.deltaTime;

            // Rotate to face movement
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

    float DistanceToPlayer()
    {
        Vector3 diff = player.position - transform.position;
        if (lockY) diff.y = 0f;
        return diff.magnitude;
    }

    void ArriveAtPoint()
    {
        // Snap to target position to avoid tiny drift
        if (fleePoints[targetIndex] != null)
        {
            transform.position = fleePoints[targetIndex].position;
        }

        currentIndex = targetIndex;
        isMoving = false;

        // If we're at the last point, we're done moving
        if (currentIndex >= fleePoints.Length - 1)
        {
            finished = true;
        }

        // 🔑 Update whether the hat is catchable (only at capturePointIndex)
        UpdateCaptureState();
    }

    void UpdateCaptureState()
    {
        if (minigameTrigger == null) return;

        if (currentIndex == capturePointIndex)
        {
            // We are on the capture point (point7 by default) → enable
            minigameTrigger.EnableCapture();
            // Debug.Log("ShyHatAI: Capture ENABLED at point index " + currentIndex);
        }
        else
        {
            // Any other point → cannot trigger minigame
            minigameTrigger.DisableCapture();
            // Debug.Log("ShyHatAI: Capture DISABLED at point index " + currentIndex);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
}
