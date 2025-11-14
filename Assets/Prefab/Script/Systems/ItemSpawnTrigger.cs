using UnityEngine;

public class ItemSpawnTrigger : MonoBehaviour
{
    public int spawnerIndex;
    private ItemSpawnerManager manager;
    private int objectsInside = 0; // how many objects are inside (important!)

    private void Start()
    {
        manager = FindObjectOfType<ItemSpawnerManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore triggers from the spawner itself
        if (other.isTrigger) return;

        objectsInside++;
        Debug.Log("SpawnIsOccupied");
        manager.SetOccupied(spawnerIndex, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;

        objectsInside--;

        // Only mark empty if fully clear
        if (objectsInside <= 0)
        {
            objectsInside = 0;
            Debug.Log("SpawnIsNotOccupied");
            manager.SetOccupied(spawnerIndex, false);
        }
    }
}

