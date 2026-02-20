using UnityEngine;
using System.Collections;

public class FastHatAI : MonoBehaviour
{
    [Header("Waypoints (in order)")]
    [Tooltip("Empty GameObjects the Fast Hat will move between.")]
    public Transform[] waypoints;

    [Header("Player Detection")]
    [Tooltip("The player Transform. If left null, will try to find by tag 'Player'.")]
    public Transform player;

    [Tooltip("How close the player must be to make Fast Hat move.")]
    public float detectDistance = 10f;

    [Tooltip("If true, Fast Hat only moves when the player is in range.")]
    public bool requirePlayerNearby = true;

    [Header("Movement")]
    [Tooltip("How fast the Fast Hat moves between points.")]
    public float moveSpeed = 8f;

    [Tooltip("Small pause at each waypoint before moving again (if not in stop window).")]
    public float baseWaitAtPoint = 0.1f;

    [Header("Stop Window Settings")]
    [Tooltip("How many waypoints Fast Hat visits before it stops for a catch window.")]
    public int pointsBetweenStops = 3;

    [Tooltip("How long Fast Hat stays still (catch window) when it stops.")]
    public float stopDuration = 5f;

    [Header("Path Options")]
    [Tooltip("If true, loops 0→1→2→...→0.")]
    public bool loop = true;

    [Tooltip("If true, goes 0→1→2→1→0→1... (overrides loop).")]
    public bool pingPong = false;

    [Header("Visual")]
    [Tooltip("Rotate to face movement direction.")]
    public bool rotateTowardsMovement = true;
    public float rotationSpeed = 12f;

    [Header("Debug / State")]
    [Tooltip("True while Fast Hat is in the stop window (vulnerable to catch).")]
    public bool isInStopWindow = false;

    private int currentIndex = 0;
    private bool goingForward = true;
    private bool isMoving = false;
    private int pointsSinceLastStop = 0;

    bool IsTutorialActive()
    {
        return NewHatalougeManager.Instance != null &&
               NewHatalougeManager.Instance.notTutorial == false;
    }

    void Start()
    {

        // 🚫 If in tutorial, completely disable AI movement
        if (IsTutorialActive())
        {
            enabled = false;   // disables this entire script
            return;
        }

        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("FastHatAI: Need at least 2 waypoints.");
            enabled = false;
            return;
        }

        // Start at first waypoint
        transform.position = waypoints[0].position;

        StartCoroutine(MoveLoop());
    }

    IEnumerator MoveLoop()
    {
        while (true)
        {
            int nextIndex = GetNextIndex();
            if (nextIndex == currentIndex)
            {
                // No more indices and not looping/pingpong
                yield break;
            }

            // 🔹 Wait for player to be near, if required
            if (requirePlayerNearby)
            {
                while (!IsPlayerInRange())
                {
                    yield return null;
                }
            }

            // Small base wait before moving
            if (baseWaitAtPoint > 0f)
                yield return new WaitForSeconds(baseWaitAtPoint);

            // Move to the next waypoint
            yield return StartCoroutine(MoveToPoint(waypoints[currentIndex], waypoints[nextIndex]));

            currentIndex = nextIndex;
            pointsSinceLastStop++;

            // 🔹 If we've visited enough points, enter a stop window
            if (pointsSinceLastStop >= pointsBetweenStops)
            {
                pointsSinceLastStop = 0;
                yield return StartCoroutine(StopWindow());
            }
        }
    }

    IEnumerator MoveToPoint(Transform from, Transform to)
    {
        isMoving = true;
        isInStopWindow = false;

        Vector3 endPos = to.position;

        while (true)
        {
            Vector3 dir = (endPos - transform.position);
            float dist = dir.magnitude;

            if (dist < 0.01f)
                break;

            Vector3 step = dir.normalized * moveSpeed * Time.deltaTime;
            if (step.magnitude > dist)
                step = dir.normalized * dist;

            transform.position += step;

            // Optional rotation
            if (rotateTowardsMovement && dir.sqrMagnitude > 0.001f)
            {
                dir.y = 0f;
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            yield return null;
        }

        // Snap to exact waypoint position
        transform.position = endPos;
        isMoving = false;
    }

    IEnumerator StopWindow()
    {
        isInStopWindow = true;

        float timer = 0f;
        while (timer < stopDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        isInStopWindow = false;
    }

    bool IsPlayerInRange()
    {
        if (player == null) return false;

        Vector3 diff = player.position - transform.position;
        diff.y = 0f;
        return diff.magnitude <= detectDistance;
    }

    int GetNextIndex()
    {
        if (pingPong)
        {
            if (goingForward)
            {
                if (currentIndex >= waypoints.Length - 1)
                {
                    goingForward = false;
                    currentIndex = waypoints.Length - 1;
                    return currentIndex - 1;
                }
                else
                {
                    return currentIndex + 1;
                }
            }
            else
            {
                if (currentIndex <= 0)
                {
                    goingForward = true;
                    currentIndex = 0;
                    return currentIndex + 1;
                }
                else
                {
                    return currentIndex - 1;
                }
            }
        }
        else if (loop)
        {
            int next = currentIndex + 1;
            if (next >= waypoints.Length)
                next = 0;
            return next;
        }
        else
        {
            int next = currentIndex + 1;
            if (next >= waypoints.Length)
                return currentIndex; // stay, loop ends
            return next;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawSphere(waypoints[i].position, 0.2f);

            if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        // Draw detection radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
}
