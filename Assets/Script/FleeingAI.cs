using UnityEngine;
using UnityEngine.AI;

public class FleeingAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;

    [Header("AI Settings")]
    public float detectionRange = 10f;
    public float safeDistance = 15f;
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float wanderRadius = 8f;
    public float wanderCooldown = 3f;

    private float nextWanderTime;
    private bool isFleeing;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        Wander();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            // Player detected → run away
            Flee();
        }
        else if (isFleeing && distance >= safeDistance)
        {
            // Cooldown done → return to wandering
            isFleeing = false;
            agent.speed = walkSpeed;
            Wander();
        }
        else if (!isFleeing && Time.time >= nextWanderTime && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Keep wandering
            Wander();
        }
    }

    void Wander()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        agent.speed = walkSpeed;
        nextWanderTime = Time.time + wanderCooldown;
    }

    void Flee()
    {
        isFleeing = true;
        agent.speed = runSpeed;

        // Run in the opposite direction from the player
        Vector3 fleeDirection = (transform.position - player.position).normalized * safeDistance;
        Vector3 newGoal = transform.position + fleeDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newGoal, out hit, safeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
