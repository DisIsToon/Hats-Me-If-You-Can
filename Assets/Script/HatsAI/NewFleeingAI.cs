using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

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
    public float rotateBeforeWalkTime = 0.5f;

    private float nextWanderTime;
    private bool isFleeing;
    private bool isSpooked;

    void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        animator.updateMode = AnimatorUpdateMode.Normal;

        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = true;
        agent.updateRotation = true;
    }

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (trail != null) trail.emitting = false;
        if (spookedUI != null) spookedUI.SetActive(false);

        PlayIdleAnimation();
        StartCoroutine(WanderRoutine());
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (!isSpooked && distance <= detectionRange && !isFleeing)
        {
            StartCoroutine(SpookAndFlee());
        }
        else if (isFleeing && distance >= safeDistance)
        {
            // Stop fleeing, back to wandering
            isFleeing = false;
            if (trail != null) trail.emitting = false;
            agent.speed = walkSpeed;
            PlayIdleAnimation();
            StartCoroutine(WanderRoutine());
        }
    }

    System.Collections.IEnumerator WanderRoutine()
    {
        while (!isFleeing && !isSpooked)
        {
            PlayIdleAnimation();
            yield return new WaitForSeconds(wanderCooldown);

            // Rotate randomly before walking
            float randomY = Random.Range(0f, 360f);
            Quaternion targetRotation = Quaternion.Euler(0, randomY, 0);
            float t = 0f;
            Quaternion startRotation = transform.rotation;

            while (t < rotateBeforeWalkTime)
            {
                t += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t / rotateBeforeWalkTime);
                yield return null;
            }

            // Start walking
            Wander();
            PlayWalkAnimation();

            // Wait until destination is reached before idling again
            while (!agent.pathPending && agent.remainingDistance > 0.3f && !isFleeing && !isSpooked)
            {
                yield return null;
            }
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
    }

    System.Collections.IEnumerator SpookAndFlee()
    {
        isSpooked = true;

        // Stop walking
        agent.ResetPath();

        // Play spook animation and show UI at the same time
        PlaySpookAnimation();
        if (spookedUI != null) spookedUI.SetActive(true);

        // Wait for spook animation duration before fleeing
        yield return new WaitForSeconds(spookDuration);

        if (spookedUI != null) spookedUI.SetActive(false);

        isSpooked = false;
        Flee();
    }

    void Flee()
    {
        isFleeing = true;
        agent.speed = runSpeed;

        PlayRunAnimation();
        if (trail != null) trail.emitting = true;

        Vector3 fleeDirection = (transform.position - player.position).normalized * safeDistance;
        Vector3 newGoal = transform.position + fleeDirection;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newGoal, out hit, safeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void PlayIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.ResetTrigger("Spooked");
        }
    }

    void PlayWalkAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsRunning", false);
        }
    }

    void PlayRunAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", true);
        }
    }

    void PlaySpookAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetTrigger("Spooked");
        }
    }
}