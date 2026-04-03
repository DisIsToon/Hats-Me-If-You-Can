using UnityEngine;
using System.Collections;

public class ShyHatAI : MonoBehaviour
{
    public enum HatState
    {
        Idle,
        NearPlayer,
        PreparingMove,
        Moving,
        Capturable,
        Finished
    }

    [Header("References")]
    public Transform player;
    public AIReactionController aiReactionController;

    [Header("Flee Points (in order)")]
    public Transform fleePoint1;
    public Transform fleePoint2;
    public Transform fleePoint3;
    public Transform fleePoint4;
    public Transform fleePoint5;
    public Transform fleePoint6;
    public Transform fleePoint7;

    private Transform[] fleePoints;

    [Header("Behavior Settings")]
    public float detectDistance = 6f;
    public float nearReactionDistance = 2.5f;
    public float moveSpeed = 10f;
    public float rotationSpeed = 10f;
    public float arrivalDistance = 0.3f;

    [Header("Options")]
    public bool lockY = true;
    public bool snapToFirstPoint = true;

    [Header("Minigame / Capture")]
    public ShyHatMinigameTrigger minigameTrigger;
    public int capturePointIndex = 6;

    [Header("Animation")]
    public Animator animator;
    public string shockedTriggerName = "Shocked";
    public float shockedDelay = 0.4f;

    [Header("Tutorial Float")]
    public bool floatDuringTutorial = true;
    public float floatAmplitude = 0.2f;
    public float floatSpeed = 2f;

    private Vector3 startPosition;

    private int currentIndex = 0;
    private int targetIndex = 0;
    private bool isMoving = false;
    private bool finished = false;
    private bool isPreparingMove = false;

    private bool playerWasNear = false;
    private HatState currentState = HatState.Idle;

    bool IsTutorialActive()
    {
        return NewHatalougeManager.Instance != null &&
               NewHatalougeManager.Instance.notTutorial == false;
    }

    void Start()
    {
        startPosition = transform.position;

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
            transform.position = fleePoints[0].position;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (minigameTrigger == null)
            minigameTrigger = GetComponent<ShyHatMinigameTrigger>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (aiReactionController == null)
            aiReactionController = GetComponent<AIReactionController>();

        currentIndex = 0;
        targetIndex = 0;

        UpdateCaptureState();
        SetState(HatState.Idle, true);
    }

    void Update()
    {
        if (IsTutorialActive())
        {
            if (floatDuringTutorial)
            {
                float newY = startPosition.y +
                             Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

                transform.position = new Vector3(
                    transform.position.x,
                    newY,
                    transform.position.z
                );
            }

            return;
        }

        if (finished || player == null)
            return;

        HandleNearReaction();

        if (!isMoving && !isPreparingMove && currentIndex < fleePoints.Length - 1)
        {
            float playerDist = DistanceToPlayer();

            if (playerDist <= detectDistance)
            {
                int nextIndex = currentIndex + 1;
                if (fleePoints[nextIndex] != null)
                    StartCoroutine(PlayShockedThenMove(nextIndex));
                else
                    FinishHat();
            }
        }

        if (isMoving)
        {
            Transform targetPoint = fleePoints[targetIndex];
            if (targetPoint == null)
            {
                isMoving = false;
                SetState(HatState.Idle);
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

    IEnumerator PlayShockedThenMove(int nextIndex)
    {
        isPreparingMove = true;
        SetState(HatState.PreparingMove);

        if (animator != null && !string.IsNullOrEmpty(shockedTriggerName))
            animator.SetTrigger(shockedTriggerName);

        if (shockedDelay > 0f)
            yield return new WaitForSeconds(shockedDelay);

        targetIndex = nextIndex;
        isMoving = true;
        isPreparingMove = false;
        SetState(HatState.Moving);
    }

    void HandleNearReaction()
    {
        if (player == null || aiReactionController == null || isMoving || isPreparingMove)
            return;

        bool playerIsNear = DistanceToPlayer() <= nearReactionDistance;

        if (playerIsNear && !playerWasNear)
            SetState(HatState.NearPlayer);

        if (!playerIsNear && playerWasNear && !finished)
            SetState(HatState.Idle);

        playerWasNear = playerIsNear;
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
            transform.position = fleePoints[targetIndex].position;

        currentIndex = targetIndex;
        isMoving = false;

        UpdateCaptureState();

        if (currentIndex >= fleePoints.Length - 1)
        {
            FinishHat();
            return;
        }

        if (currentIndex == capturePointIndex)
            SetState(HatState.Capturable);
        else
            SetState(HatState.Idle);
    }

    void FinishHat()
    {
        finished = true;
        SetState(HatState.Finished);
    }

    void UpdateCaptureState()
    {
        if (minigameTrigger == null)
            return;

        if (currentIndex == capturePointIndex)
            minigameTrigger.EnableCapture();
        else
            minigameTrigger.DisableCapture();
    }

    void SetState(HatState newState, bool force = false)
    {
        if (!force && currentState == newState)
            return;

        currentState = newState;

        if (aiReactionController == null)
            return;

        switch (newState)
        {
            case HatState.Idle:
                aiReactionController.ShowIdleReaction();
                break;

            case HatState.NearPlayer:
                aiReactionController.ShowPlayerNearReaction();
                break;

            case HatState.PreparingMove:
                aiReactionController.ShowDifferentReaction(true);
                break;

            case HatState.Moving:
                break;

            case HatState.Capturable:
                aiReactionController.ShowDifferentReaction(true);
                break;

            case HatState.Finished:
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, nearReactionDistance);
    }
}