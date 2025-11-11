using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class JumpHatAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Detection")]
    public float detectRange = 6f;       // starts fleeing when player is within this range

    [Header("Hop Forces")]
    public float hopForwardForce = 6.5f; // forward impulse along ground
    public float hopUpForce = 5.5f;      // upward impulse
    public float extraGravity = 15f;     // extra downward accel for snappy landings

    [Header("Cadence")]
    public float minHopInterval = 0.35f; // min time between hops (only when grounded)
    public float maxHopInterval = 0.55f; // max time between hops
    public float minFleeBurst = 2;       // min # consecutive hops when fleeing
    public float maxFleeBurst = 4;       // max # consecutive hops when fleeing

    [Header("Grounding")]
    public float groundCheckRadius = 0.2f;
    public float groundCheckOffset = 0.05f;
    public LayerMask groundMask = ~0;    // set to your ground layers

    [Header("Steering & Look")]
    public float turnSpeed = 12f;        // how fast the frog rotates to face direction
    public float obstacleProbeDistance = 1.0f; // probe ahead to avoid walls
    public float obstacleSideProbeAngle = 25f; // try angled directions if blocked
    public float obstacleProbeRadius = 0.2f;

    [Header("Idle Hops (optional)")]
    public bool idleMicroHops = false;   // tiny ambient hops when player is far
    public float idleHopForwardForce = 1.2f;
    public float idleHopUpForce = 0.9f;

    Rigidbody rb;
    float hopCooldown;
    bool isFleeing;
    int hopsRemainingInBurst;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // smoother motion
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool shouldFlee = dist <= detectRange;

        if (shouldFlee && !isFleeing)
        {
            // start a flee burst (2�4 hops by default)
            isFleeing = true;
            hopsRemainingInBurst = Random.Range((int)minFleeBurst, (int)maxFleeBurst + 1);
        }
        else if (!shouldFlee && isFleeing)
        {
            // end fleeing when player leaves range
            isFleeing = false;
            hopsRemainingInBurst = 0;
        }

        // Smoothly face current horizontal velocity, if any
        Vector3 v = rb.linearVelocity; v.y = 0f;
        if (v.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(v.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }
    }

    void FixedUpdate()
    {
        // Add extra gravity for crisp landings
        rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);

        // Reduce cooldown over time
        if (hopCooldown > 0f)
            hopCooldown -= Time.fixedDeltaTime;

        // Only initiate a hop when grounded and cooldown is done
        if (IsGrounded() && hopCooldown <= 0f)
        {
            if (isFleeing)
            {
                Vector3 dir = AwayFromPlayerOnPlane();

                // Avoid obstacles directly ahead
                dir = AdjustDirectionForObstacles(dir);

                DoHop(dir, hopForwardForce, hopUpForce);
                hopsRemainingInBurst = Mathf.Max(0, hopsRemainingInBurst - 1);

                // If burst done but still fleeing, start a new burst soon
                if (hopsRemainingInBurst == 0)
                {
                    hopCooldown = Random.Range(minHopInterval, maxHopInterval);
                    if (IsPlayerStillClose()) // recheck; if close, chain bursts
                        hopsRemainingInBurst = Random.Range((int)minFleeBurst, (int)maxFleeBurst + 1);
                }
                else
                {
                    // quick cadence during a burst
                    hopCooldown = Random.Range(minHopInterval, maxHopInterval) * 0.6f;
                }
            }
            else if (idleMicroHops)
            {
                // tiny ambient hop in a gently random direction
                Vector3 dir = Random.onUnitSphere; dir.y = 0f; dir.Normalize();
                DoHop(dir, idleHopForwardForce, idleHopUpForce);
                hopCooldown = Random.Range(0.9f, 1.4f);
            }
        }
    }

    void DoHop(Vector3 flatDir, float forwardForce, float upForce)
    {
        // Clear any downward velocity for consistent takeoff
        Vector3 vel = rb.linearVelocity;
        if (vel.y < 0f) vel.y = 0f;
        rb.linearVelocity = vel;

        // Compose impulse: forward + up
        Vector3 impulse = flatDir.normalized * forwardForce + Vector3.up * upForce;
        rb.AddForce(impulse, ForceMode.VelocityChange);
    }

    Vector3 AwayFromPlayerOnPlane()
    {
        Vector3 away = (transform.position - player.position);
        away.y = 0f;
        if (away.sqrMagnitude < 0.0001f) away = transform.forward; // fallback
        return away.normalized;
    }

    Vector3 AdjustDirectionForObstacles(Vector3 dir)
    {
        Vector3 origin = GroundProbeOrigin();

        // direct path
        if (!Physics.SphereCast(origin, obstacleProbeRadius, dir, out _, obstacleProbeDistance, groundMask, QueryTriggerInteraction.Ignore))
            return dir;

        // try slight left/right angles
        Quaternion left = Quaternion.AngleAxis(-obstacleSideProbeAngle, Vector3.up);
        Quaternion right = Quaternion.AngleAxis(obstacleSideProbeAngle, Vector3.up);
        Vector3 leftDir = left * dir;
        Vector3 rightDir = right * dir;

        bool leftClear = !Physics.SphereCast(origin, obstacleProbeRadius, leftDir, out _, obstacleProbeDistance, groundMask, QueryTriggerInteraction.Ignore);
        bool rightClear = !Physics.SphereCast(origin, obstacleProbeRadius, rightDir, out _, obstacleProbeDistance, groundMask, QueryTriggerInteraction.Ignore);

        if (leftClear && rightClear)
            return (Random.value < 0.5f) ? leftDir : rightDir;
        if (leftClear) return leftDir;
        if (rightClear) return rightDir;

        // fall back: 180� (straight opposite of blockage)
        return -dir;
    }

    bool IsGrounded()
    {
        Vector3 origin = GroundProbeOrigin();
        return Physics.CheckSphere(origin, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
    }

    Vector3 GroundProbeOrigin()
    {
        // slightly above bottom of collider
        float offset = groundCheckOffset;
        return transform.position + Vector3.down * ((GetComponent<Collider>().bounds.extents.y) - offset);
    }

    bool IsPlayerStillClose()
    {
        if (!player) return false;
        return Vector3.Distance(transform.position, player.position) <= detectRange;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GroundProbeOrigin(), groundCheckRadius);
    }
#endif
}
