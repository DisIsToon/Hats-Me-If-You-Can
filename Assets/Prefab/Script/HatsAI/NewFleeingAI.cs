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
    public ParticleSystem glitterParticles;

    [Header("AI Settings")]
    public float detectionRange = 10f;
    public float safeDistance = 5f;
    public float walkSpeed = 1f;
    public float runSpeed = 3f;
    public float wanderRadius = 1f;
    public float wanderCooldown = 3f;
    public float spookDuration = 1.5f;

    private bool isFleeing;
    private bool isSpooked;
    private Coroutine wanderRoutine;

    void Awake()
    {
    

        agent = GetComponent<NavMeshAgent>();

        if (animator != null)
        {
            animator.applyRootMotion = false;
            // Force model to stick to root
            animator.transform.localPosition = Vector3.zero;
            animator.transform.localRotation = Quaternion.identity;
        }

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
        if (glitterParticles != null) glitterParticles.Stop();

        StartWandering();
    }

    void Update()
    {
        if (player == null || agent == null) return;

        // Drive animation based on movement speed
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
            animator.applyRootMotion = false; // force OFF
            animator.transform.localPosition = Vector3.zero; // lock model to root
            animator.transform.localRotation = Quaternion.identity;
        }
            

        float distance = Vector3.Distance(transform.position, player.position);

        if (agent.velocity.magnitude <= 0.05f) // basically standing still
        {
            if (glitterParticles.isPlaying) glitterParticles.Stop();
        }
        // Trigger spook only once if in range
        if (!isSpooked && distance <= detectionRange && !isFleeing)
        {
            Debug.Log("Spook Trigger");
            StartCoroutine(SpookRoutine());
        }
        else if (isFleeing && distance >= safeDistance)
        {
            Debug.Log("Stop Fleing Start Wandering");
            // Stop fleeing and resume wandering
            isFleeing = false;
            if (trail != null) trail.emitting = false;
            if (glitterParticles != null) glitterParticles.Stop();
            agent.speed = walkSpeed;
            StartWandering();
        }
    }

    void StartWandering()
    {
        Debug.Log("Start Wandering");
        if (wanderRoutine != null)
            StopCoroutine(wanderRoutine);

        wanderRoutine = StartCoroutine(WanderRoutine());
    }

    IEnumerator WanderRoutine()
    {
        Debug.Log("WanderRoutine");
        while (!isFleeing && !isSpooked)
        {
            Debug.Log("WanderRoutine2");
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
        Debug.Log("Wander");
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            Debug.Log("Wander2");
            agent.speed = walkSpeed;
            agent.SetDestination(hit.position);
            Debug.Log("Agent speed? " + agent.speed);
        }
    }

    IEnumerator SpookRoutine()
    {
        isSpooked = true;
        agent.isStopped = true; // freeze agent movement

        Debug.Log("SpookRoutine");


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
        Debug.Log("Spook End");
    }

    void Flee()
    {
        isFleeing = true;
        agent.speed = runSpeed;
        Debug.Log("Flee");
        Debug.Log("Agent stopped when fleeing? " + agent.isStopped + " | Velocity: " + agent.velocity);
        Debug.Log("Agent speed? " + agent.speed );

        if (trail != null) trail.emitting = true;
        if (glitterParticles != null) glitterParticles.Play(); // 🔹 Start glitter
        Vector3 fleeDirection = (transform.position - player.position).normalized * safeDistance;
        Vector3 newGoal = transform.position + fleeDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newGoal, out hit, safeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
