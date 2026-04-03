using UnityEngine;
using System.Collections;

public class JumpHatAI : MonoBehaviour
{
    public enum JumpHatState
    {
        Idle,
        NearPlayer,
        WaitingToJump,
        Jumping,
        Finished
    }

    [Header("Jump Points (in order)")]
    [Tooltip("Empty GameObjects in the scene that the hat will jump between.")]
    public Transform[] jumpPoints;

    [Header("Player Detection")]
    public Transform player;
    [Tooltip("How close the player must be to make the hat jump.")]
    public float detectDistance = 8f;
    public bool requirePlayerToJump = true;

    [Header("Near Reaction")]
    public float nearReactionDistance = 3f;

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
    public string loopAnimationState = "HatLoop";

    [Header("Reaction System")]
    public AIReactionController aiReactionController;

    private int currentIndex = 0;
    private bool goingForward = true;
    private bool isJumping = false;
    private bool playerWasNear = false;
    private JumpHatState currentState = JumpHatState.Idle;

    void Start()
    {
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

        transform.position = jumpPoints[0].position;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        if (aiReactionController == null)
            aiReactionController = GetComponent<AIReactionController>();

        PlayLoopAnimation();
        SetState(JumpHatState.Idle, true);
        StartCoroutine(JumpLoop());
    }

    void Update()
    {
        HandleNearReaction();
    }

    void PlayLoopAnimation()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(loopAnimationState)) return;

        animator.Play(loopAnimationState);
    }

    IEnumerator JumpLoop()
    {
        while (true)
        {
            int nextIndex = GetNextIndex();
            if (nextIndex == currentIndex)
                yield break;

            if (requirePlayerToJump)
            {
                while (!IsPlayerInRange())
                    yield return null;
            }

            SetState(JumpHatState.WaitingToJump);

            if (waitAtPoint > 0f)
                yield return new WaitForSeconds(waitAtPoint);

            SetState(JumpHatState.Jumping);
            yield return StartCoroutine(JumpToPoint(jumpPoints[currentIndex], jumpPoints[nextIndex]));

            currentIndex = nextIndex;
            SetState(JumpHatState.Idle);
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

            Vector3 basePos = Vector3.Lerp(startPos, endPos, t);
            float arc = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            basePos.y += arc;

            transform.position = basePos;

            if (rotateTowardsNextPoint)
            {
                Vector3 direction = (endPos - startPos);
                direction.y = 0f;
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

        transform.position = endPos;
        isJumping = false;
    }

    void HandleNearReaction()
    {
        if (player == null || aiReactionController == null || isJumping)
            return;

        Vector3 diff = player.position - transform.position;
        diff.y = 0f;
        bool playerIsNear = diff.magnitude <= nearReactionDistance;

        if (playerIsNear && !playerWasNear)
            SetState(JumpHatState.NearPlayer);

        if (!playerIsNear && playerWasNear && !isJumping)
            SetState(JumpHatState.Idle);

        playerWasNear = playerIsNear;
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
                return currentIndex;
            return next;
        }
    }

    void SetState(JumpHatState newState, bool force = false)
    {
        if (!force && currentState == newState)
            return;

        currentState = newState;

        if (aiReactionController == null)
            return;

        switch (newState)
        {
            case JumpHatState.Idle:
                aiReactionController.ShowIdleReaction();
                break;

            case JumpHatState.NearPlayer:
                aiReactionController.ShowPlayerNearReaction();
                break;

            case JumpHatState.WaitingToJump:
                aiReactionController.ShowDifferentReaction(true);
                break;

            case JumpHatState.Jumping:
                break;

            case JumpHatState.Finished:
                aiReactionController.HideReaction();
                break;
        }
    }

    void OnDisable()
    {
        if (aiReactionController != null)
            aiReactionController.HideReaction();
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
                Gizmos.DrawLine(jumpPoints[i].position, jumpPoints[i + 1].position);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, nearReactionDistance);
    }
}