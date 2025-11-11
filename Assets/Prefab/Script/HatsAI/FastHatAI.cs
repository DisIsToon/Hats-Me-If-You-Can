using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Collider))]
public class FastHatAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Awareness")]
    public float alertRange = 7f;       // how close player must be before FastHat starts running
    public float leashRadius = 12f;     // how far FastHat can move from home
    public Vector3 homePosition;

    [Header("Movement Speeds")]
    public float runSpeed = 11f;        // fast sprint speed
    public float tiredSpeed = 4f;       // slow catchable speed
    public float acceleration = 60f;
    public float angularSpeed = 900f;

    [Header("Behavior Timing")]
    public float runDuration = 1.4f;    // how long he runs
    public float tiredDuration = 0.8f;  // how long he’s tired
    public float zigzagAmplitude = 1.5f;
    public float zigzagFrequency = 3.5f;
    public float fleeStep = 3.5f;       // how far each target jump is while fleeing

    [Header("Ground Offset Fix")]
    public float baseOffset = 0.45f;    // lifts agent so it doesn’t clip ground

    [Header("Debug")]
    public bool drawGizmos = true;

    private NavMeshAgent agent;
    private bool isRunning = false;
    private bool isTired = false;
    private float stateTimer = 0f;
    private Vector3 lastDirection;
    private Vector3 lastDestination;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.autoBraking = false;
        agent.stoppingDistance = 0.15f;
        agent.baseOffset = baseOffset;

        if (homePosition == Vector3.zero)
            homePosition = transform.position;
    }

    void Start()
    {
        // Snap to NavMesh
        if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        StartRunning(); // start the infinite run/tired loop
    }

    void Update()
    {
        if (!player) return;

        stateTimer -= Time.deltaTime;

        if (isRunning)
        {
            RunLogic();

            // when run timer ends → go tired
            if (stateTimer <= 0f)
                StartTired();
        }
        else if (isTired)
        {
            // tired = slow wander but still watch player
            TiredLogic();

            // when tired timer ends → go back to running
            if (stateTimer <= 0f)
                StartRunning();
        }
    }

    // --------------------
    // STATE LOGIC
    // --------------------

    void StartRunning()
    {
        isRunning = true;
        isTired = false;
        stateTimer = runDuration;
        agent.speed = runSpeed;
    }

    void StartTired()
    {
        isRunning = false;
        isTired = true;
        stateTimer = tiredDuration;
        agent.speed = tiredSpeed;
    }

    void RunLogic()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= alertRange)
        {
            Vector3 awayDir = (transform.position - player.position);
            awayDir.y = 0f;
            if (awayDir.sqrMagnitude < 0.01f) awayDir = transform.forward;
            awayDir.Normalize();

            // add zigzag for more life
            Vector3 lateral = Vector3.Cross(Vector3.up, awayDir).normalized;
            float sway = Mathf.Sin(Time.time * zigzagFrequency) * zigzagAmplitude;
            Vector3 moveDir = (awayDir + lateral * sway * 0.2f).normalized;

            Vector3 target = transform.position + moveDir * fleeStep;

            // stay near home
            if (Vector3.Distance(homePosition, target) > leashRadius)
            {
                Vector3 back = (homePosition - transform.position).normalized;
                target = transform.position + back * fleeStep;
            }

            SetDestinationSafe(target);
            lastDirection = moveDir;
        }
        else
        {
            // if player far, idle near home
            RoamNearHome();
        }
    }

    void TiredLogic()
    {
        // small slow movement in last known direction
        Vector3 target = transform.position + lastDirection * (fleeStep * 0.5f);
        SetDestinationSafe(target);
    }

    void RoamNearHome()
    {
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector2 rnd = Random.insideUnitCircle * Mathf.Min(leashRadius * 0.4f, 4f);
            Vector3 goal = homePosition + new Vector3(rnd.x, 0f, rnd.y);
            SetDestinationSafe(goal);
        }
    }

    void SetDestinationSafe(Vector3 target)
    {
        if (NavMesh.SamplePosition(target, out var hit, 2f, NavMesh.AllAreas))
        {
            if (Vector3.Distance(lastDestination, hit.position) > 0.2f)
            {
                agent.SetDestination(hit.position);
                lastDestination = hit.position;
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRange);

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireSphere(homePosition == Vector3.zero ? transform.position : homePosition, leashRadius);
    }
#endif
}
