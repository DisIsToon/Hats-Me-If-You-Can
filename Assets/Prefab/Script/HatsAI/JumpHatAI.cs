using UnityEngine;
using System.Collections;

public class JumpHatAI : MonoBehaviour
{
    [Header("Jump Points (in order)")]
    [Tooltip("Empty GameObjects in the scene that the hat will jump between.")]
    public Transform[] jumpPoints;

    [Header("Player Detection")]
    public Transform player;
    [Tooltip("How close the player must be to make the hat jump.")]
    public float detectDistance = 8f;
    public bool requirePlayerToJump = true;   // if false, it just loops jumps forever

    [Header("Jump Settings")]
    [Tooltip("How long one jump takes (seconds).")]
    public float jumpDuration = 0.7f;

    [Tooltip("Maximum height of the jump above the line between two points.")]
    public float jumpHeight = 2f;

    [Tooltip("How long to wait after each landing before starting the next jump.")]
    public float waitAtPoint = 0.2f;

    [Header("Loop Options")]
    [Tooltip("If true, 0→1→2→...→0.")]
    public bool loop = true;

    [Tooltip("If true, 0→1→2→1→0→1... (overrides loop).")]
    public bool pingPong = false;

    [Header("Visual")]
    [Tooltip("Rotate to face the direction of the jump.")]
    public bool rotateTowardsNextPoint = true;
    public float rotationSpeed = 10f;

    [Header("Animation")]
    [Tooltip("Animator on the hat (or child). If left empty, will auto-find on this GameObject or children.")]
    public Animator animator;

    [Tooltip("Name of the looping animation state that should play the whole time.")]
    public string loopAnimationState = "HatLoop"; // change to your state name

    private int currentIndex = 0;
    private bool goingForward = true;
    private bool isJumping = false;

    void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (jumpPoints == null || jumpPoints.Length < 2)
        {
            Debug.LogWarning("JumpHatAI: Need at least 2 jump points for jumping to work.");
            enabled = false;
            return;
        }

        // Start at the first point
        transform.position = jumpPoints[0].position;

        // 🔹 Auto-find animator if not set
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        // 🔹 Start the looping animation once
        PlayLoopAnimation();

        StartCoroutine(JumpLoop());
    }

    void PlayLoopAnimation()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(loopAnimationState)) return;

        // We just tell the animator to play this state – 
        // make sure that state is set to Loop in the animation import.
        animator.Play(loopAnimationState);
    }

    IEnumerator JumpLoop()
    {
        while (true)
        {
            int nextIndex = GetNextIndex();
            if (nextIndex == currentIndex)
            {
                // No valid next index; stop the loop
                yield break;
            }

            // 🔹 Wait for player to come close (if required)
            if (requirePlayerToJump)
            {
                // Wait until we have a player and they are within range
                while (!IsPlayerInRange())
                {
                    yield return null;
                }
            }

            // Small delay before jumping (optional)
            if (waitAtPoint > 0f)
                yield return new WaitForSeconds(waitAtPoint);

            // Perform the jump
            yield return StartCoroutine(JumpToPoint(jumpPoints[currentIndex], jumpPoints[nextIndex]));

            currentIndex = nextIndex;
        }
    }

    IEnumerator JumpToPoint(Transform from, Transform to)
    {
        isJumping = true;

        Vector3 startPos = from.position;
        Vector3 endPos = to.position;

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);

            // Horizontal interpolation
            Vector3 basePos = Vector3.Lerp(startPos, endPos, t);

            // Vertical arc using a sine curve (0→1→0)
            float arc = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            basePos.y += arc;

            transform.position = basePos;

            // Optional rotation towards movement direction
            if (rotateTowardsNextPoint)
            {
                Vector3 direction = (endPos - startPos);
                direction.y = 0f; // keep rotation flat
                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction.normalized);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }

            yield return null;
        }

        // Snap to exact end position
        transform.position = endPos;
        isJumping = false;
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
                if (currentIndex >= jumpPoints.Length - 1)
                {
                    goingForward = false;
                    currentIndex = jumpPoints.Length - 1;
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
            if (next >= jumpPoints.Length)
                next = 0;
            return next;
        }
        else
        {
            int next = currentIndex + 1;
            if (next >= jumpPoints.Length)
                return currentIndex; // stay there, loop will end
            return next;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (jumpPoints == null || jumpPoints.Length == 0) return;

        Gizmos.color = Color.magenta;
        for (int i = 0; i < jumpPoints.Length; i++)
        {
            if (jumpPoints[i] == null) continue;
            Gizmos.DrawSphere(jumpPoints[i].position, 0.2f);

            if (i < jumpPoints.Length - 1 && jumpPoints[i + 1] != null)
            {
                Gizmos.DrawLine(jumpPoints[i].position, jumpPoints[i + 1].position);
            }
        }

        // Draw detection radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }
}
