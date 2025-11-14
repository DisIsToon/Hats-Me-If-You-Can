using UnityEngine;
using System.Collections.Generic;

public class ItemSpawnerManager : MonoBehaviour
{
    [Header("Spawner Settings")]
    public Transform[] spawnPoints;      // assign your 5 (or 9) spawn points
    public GameObject objectToSpawn;
    public float minSpawnTime = 20f;
    public float maxSpawnTime = 100f;

    [Header("Safety Check (optional)")]
    public bool usePhysicsCheck = true;
    public float checkRadius = 0.6f;
    public LayerMask blockingLayers = ~0; // default: everything
    [Tooltip("If set, only objects with this tag will be considered blocking. Leave blank to treat any collider as blocking.")]
    public string blockingTag = "";

    // runtime state
    private bool[] isOccupied;
    private float spawnTimer = 0f;       // accumulates fixedDeltaTime
    private float nextSpawnDelay = 0f;   // when timer >= nextSpawnDelay -> attempt spawn
    private readonly System.Object spawnLock = new System.Object();

    private void Awake()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            Debug.LogWarning("ItemSpawnerManager: spawnPoints is empty.");

        isOccupied = new bool[Mathf.Max(1, spawnPoints.Length)];
        ResetSpawnTimer();
    }

    private void OnEnable()
    {
        // ensure timer is set when enabled
        ResetSpawnTimer();
    }

    // Called by ItemSpawnTrigger (do NOT change)
    public void SetOccupied(int index, bool state)
    {
        if (index < 0 || index >= isOccupied.Length)
        {
            Debug.LogWarning($"SetOccupied: index {index} out of range.");
            return;
        }

        isOccupied[index] = state;
        Debug.Log($"SetOccupied: spawner[{index}] = {state}");
        // we DO NOT restart timer here; FixedUpdate loop handles continuous attempts
    }

    private void FixedUpdate()
    {
        // accumulate physics-step time
        spawnTimer += Time.fixedDeltaTime;

        // when timer reaches delay, try to spawn
        if (spawnTimer >= nextSpawnDelay)
        {
            // reset timer now (prevents repeated immediate attempts)
            spawnTimer = 0f;
            nextSpawnDelay = Random.Range(minSpawnTime, maxSpawnTime);

            TrySpawnOne();
        }
    }

    private void TrySpawnOne()
    {
        // lock in case other code reads/writes simultaneously (defensive)
        lock (spawnLock)
        {
            // collect empty indices
            List<int> emptyIndices = new List<int>();
            for (int i = 0; i < isOccupied.Length; i++)
            {
                if (!isOccupied[i])
                    emptyIndices.Add(i);
            }

            if (emptyIndices.Count == 0)
            {
                Debug.Log("TrySpawnOne: no empty spawners available. Will retry after next delay.");
                return;
            }

            // if using physics check, try candidates until we find a physically clear one
            int chosenIndex = -1;
            if (usePhysicsCheck)
            {
                // shuffle or randomly pick from list until one is clear
                List<int> candidates = new List<int>(emptyIndices);
                while (candidates.Count > 0)
                {
                    int pickIndex = Random.Range(0, candidates.Count);
                    int pick = candidates[pickIndex];

                    if (IsSpawnAreaClear(pick))
                    {
                        chosenIndex = pick;
                        break;
                    }
                    else
                    {
                        // remove this candidate and try others
                        candidates.RemoveAt(pickIndex);
                    }
                }

                if (chosenIndex == -1)
                {
                    Debug.Log("TrySpawnOne: empty spawners found but all physically blocked. Skipping this cycle.");
                    return;
                }
            }
            else
            {
                // no physics check — pick random empty
                chosenIndex = emptyIndices[Random.Range(0, emptyIndices.Count)];
            }

            // final double-check of occupancy before instantiate (protects against race where trigger set occupied)
            if (isOccupied[chosenIndex])
            {
                Debug.Log($"TrySpawnOne: chosen spawner {chosenIndex} is now occupied. Aborting this attempt.");
                return;
            }

            // Perform spawn
            Instantiate(objectToSpawn, spawnPoints[chosenIndex].position, spawnPoints[chosenIndex].rotation);
            isOccupied[chosenIndex] = true;
            Debug.Log($"TrySpawnOne: spawned at spawner {chosenIndex}");
        }
    }

    // Physics overlap check to ensure there's no collider blocking the spawn area
    private bool IsSpawnAreaClear(int index)
    {
        if (index < 0 || index >= spawnPoints.Length) return false;

        Collider[] hits = Physics.OverlapSphere(spawnPoints[index].position, checkRadius, blockingLayers, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0)
            return true;

        if (string.IsNullOrEmpty(blockingTag))
        {
            return false; // any collider blocks
        }
        else
        {
            foreach (var c in hits)
            {
                GameObject root = c.transform.root.gameObject;
                if (root.CompareTag(blockingTag))
                    return false;
            }
            return true; // colliders found but none with blockingTag
        }
    }

    // editor visualization
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null) continue;
            Gizmos.DrawWireSphere(spawnPoints[i].position, checkRadius);
        }
    }

    private void ResetSpawnTimer()
    {
        spawnTimer = 0f;
        nextSpawnDelay = Random.Range(minSpawnTime, maxSpawnTime);
    }
}
