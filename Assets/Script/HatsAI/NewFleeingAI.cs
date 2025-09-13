using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections;

public class NewFleeingAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Animator animator;
    public GameObject spookedUI;
    public TrailRenderer trail;

    [Header("AI Settings")]
    public float detectionRange = 10f;
    public float safeDistance = 15f;
    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float wanderRadius = 8f;
    public float wanderCooldown = 3f;
    public float spookDuration = 1.5f;

    private bool isFleeing;
    private bool isSpooked;
    private Coroutine wanderRoutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        if (agent != null)
        {
            agent.updatePosition = true;
            agent.updateRotation = true;
        }
    }

    void Start()
    {
        if (trail != null) trail.emitting = false;
        if (spookedUI != null) spookedUI.SetActive(false);

        StartWandering();
    }

    void Update()
    {
        if (player == null || agent == null) return;

        // Drive animation based on movement speed
        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);

        float distance = Vector3.Distance(transform.position, player.position);

        // Trigger spook only once if in range
        if (!isSpooked && distance <= detectionRange && !isFleeing)
        {
            StartCoroutine(SpookRoutine());
        }
        else if (isFleeing && distance >= safeDistance)
        {
            // Stop fleeing and resume wandering
            isFleeing = false;
            if (trail != null) trail.emitting = false;
            agent.speed = walkSpeed;
            StartWandering();
        }
    }

    void StartWandering()
    {
        if (wanderRoutine != null)
            StopCoroutine(wanderRoutine);

        wanderRoutine = StartCoroutine(WanderRoutine());
    }

    IEnumerator WanderRoutine()
    {
        while (!isFleeing && !isSpooked)
        {
            yield return new WaitForSeconds(wanderCooldown);
            Wander();

            // Wait until agent reaches destination
            while (!agent.pathPending && agent.remainingDistance > 0.3f && !isFleeing && !isSpooked)
                yield return null;
        }
    }

    void Wander()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.speed = walkSpeed;
            agent.SetDestination(hit.position);
        }
    }

    IEnumerator SpookRoutine()
    {
        isSpooked = true;
        agent.isStopped = true; // freeze agent movement

        if (animator != null) animator.SetTrigger("Spooked");
        if (spookedUI != null) spookedUI.SetActive(true);

        // Wait for animation (or fixed duration if you prefer)
        yield return new WaitForSeconds(spookDuration);

        if (spookedUI != null) spookedUI.SetActive(false);

        // Resume movement after animation is done
        agent.isStopped = false;
        isSpooked = false;

        Flee();
    }

    // Called AFTER spook animation finishes (optional: link from Animator event)
    public void OnSpookAnimationEnd()
    {
       // if (!isFleeing) Flee();

        // This will be called when the spook animation ends
        Debug.Log("Spook animation finished!");

        // Resume fleeing if needed
        isSpooked = false;
        agent.isStopped = false;
        Flee();
    }

    void Flee()
    {
        isFleeing = true;
        agent.speed = runSpeed;

        if (trail != null) trail.emitting = true;

        Vector3 fleeDirection = (transform.position - player.position).normalized * safeDistance;
        Vector3 newGoal = transform.position + fleeDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newGoal, out hit, safeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
