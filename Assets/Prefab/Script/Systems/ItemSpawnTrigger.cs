using UnityEngine;

public class ItemSpawnTrigger : MonoBehaviour
{
    public int spawnerIndex;
    private ItemSpawnerManager manager;
    private int objectsInside = 0;

    private void Start()
    {
        manager = FindObjectOfType<ItemSpawnerManager>();
    }

    public void NotifyItemRemoved()
    {
        objectsInside--;

        if (objectsInside <= 0)
        {
            objectsInside = 0;
            Debug.Log("SpawnIsNotOccupied (destroyed/picked up)");
            manager.SetOccupied(spawnerIndex, false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        objectsInside++;
        Debug.Log("SpawnIsOccupied");

        // register the item so it can notify when destroyed
        SpawnedItem item = other.GetComponent<SpawnedItem>();
        if (item != null)
            item.SetTrigger(this);

        manager.SetOccupied(spawnerIndex, true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;

        objectsInside--;
        Debug.Log("SpawnIsNotOccupiedddddddddd");

        if (objectsInside <= 0)
        {
            objectsInside = 0;
            Debug.Log("SpawnIsNotOccupied (exited)");
            manager.SetOccupied(spawnerIndex, false);
        }
    }
}

