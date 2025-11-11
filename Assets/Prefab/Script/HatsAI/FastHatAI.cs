using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Collider))]
public class FastHatAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Awareness")]
    public float alertRange = 10f;
    public float leashRadius = 30f;
    public Vector3 homePosition;

    [Header("Movement (fast)")]
    public float cruiseSpeed = 4.5f;   // idle/roam
    public float sprintSpeed = 12f;    // burst speed
    public float acceleration = 70f;   // strong accel
    public float angularSpeed = 900f;  // turns fast

    [Header("Bursts")]
    public float burstDuration = 1.1f;     // sprint window
    public float restBetweenBursts = 0.15f;
    public int burstsUntilTired = 2;
    public float tiredDuration = 0.9f;     // brief “catchable” slow

    [Header("Zigzag")]
    public float zigzagAmplitude = 2.0f;
    public float zigzagFrequency = 2.5f;

    [Header("Pathing")]
    public float repathInterval = 0.2f; // rate-limit SetDestination
    public float minTargetDelta = 0.5f; // only update if far enough
    public float fleeStep = 8f;         // step distance per burst

    [Header("Ground Visual Fix (Option 1)")]
    public float baseOffset = 0.45f;    // lifts the agent so mesh doesn’t clip

    NavMeshAgent agent;
    float burstTimer, restTimer, repathTimer;
    int burstsInCycle;
    bool fleeing, tired;
    Vector3 fleeDirCached;
    Vector3 lastIssuedDest;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Fast, responsive agent
        agent.speed = cruiseSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.autoBraking = false;
        agent.stoppingDistance = 0.15f;

        // Visual anti-clipping (Option 1)
        agent.baseOffset = baseOffset;

        if (homePosition == Vector3.zero)
            homePosition = transform.position;
    }

    void Start()
    {
        // Ensure we’re on the NavMesh
        if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);
    }

    void Update()
    {
        if (!player) return;

        repathTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);
        fleeing = dist <= alertRange && !tired;

        if (tired)
        {
            agent.speed = cruiseSpeed * 0.6f;
            return;
        }

        if (fleeing) DoFleeLogic();
        else RoamNearHome();

        // Unstick if turning in place
        if (agent.hasPath && agent.velocity.sqrMagnitude < 0.001f)
            agent.ResetPath();
    }

    void DoFleeLogic()
    {
        // Start burst
        if (burstTimer <= 0f && restTimer <= 0f)
        {
            burstTimer = burstDuration;
            burstsInCycle++;
            agent.speed = sprintSpeed;

            Vector3 away = transform.position - player.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.0001f) away = transform.forward;
            fleeDirCached = away.normalized;
        }

        // During burst
        if (burstTimer > 0f)
        {
            burstTimer -= Time.deltaTime;

            Vector3 lateral = Vector3.Cross(Vector3.up, fleeDirCached);
            float sway = Mathf.Sin(Time.time * zigzagFrequency) * zigzagAmplitude;
            Vector3 desired = transform.position + fleeDirCached * fleeStep + lateral * sway;

            // Keep within leash
            if (Vector3.Distance(homePosition, desired) > leashRadius)
            {
                Vector3 back = (homePosition - transform.position).normalized;
                desired = transform.position + back * fleeStep;
            }

            TrySetDestination(desired);

            if (burstTimer <= 0f)
            {
                restTimer = restBetweenBursts;
                agent.speed = cruiseSpeed;
            }
        }
        else if (restTimer > 0f)
        {
            restTimer -= Time.deltaTime;

            Vector3 drift = transform.position + fleeDirCached * Mathf.Max(2f, fleeStep * 0.3f);
            TrySetDestination(drift);

            if (restTimer <= 0f && burstsInCycle >= burstsUntilTired)
            {
                burstsInCycle = 0;
                StartCoroutine(TiredWindow());
            }
        }
    }

    System.Collections.IEnumerator TiredWindow()
    {
        tired = true;
        float t = tiredDuration;
        float prev = agent.speed;
        agent.speed = cruiseSpeed * 0.5f;
        while (t > 0f) { t -= Time.deltaTime; yield return null; }
        tired = false;
        agent.speed = prev;
    }

    void RoamNearHome()
    {
        agent.speed = cruiseSpeed;

        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector2 rnd = Random.insideUnitCircle * Mathf.Min(leashRadius * 0.4f, 6f);
            Vector3 goal = homePosition + new Vector3(rnd.x, 0f, rnd.y);
            TrySetDestination(goal, force: true);
        }
    }

    void TrySetDestination(Vector3 desired, bool force = false)
    {
        if (!force && repathTimer > 0f) return;

        if (NavMesh.SamplePosition(desired, out var hit, 2f, NavMesh.AllAreas))
        {
            if (force || Vector3.Distance(lastIssuedDest, hit.position) > minTargetDelta)
            {
                agent.SetDestination(hit.position);
                lastIssuedDest = hit.position;
                repathTimer = repathInterval;
            }
        }
    }
}
