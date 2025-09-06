using UnityEngine;

public class Pickup : MonoBehaviour
{
    public Item item; // Assign the ScriptableObject here

    private void OnTriggerEnter(Collider other)
    {
        Inventory inventory = other.GetComponent<Inventory>();
        if (inventory != null)
        {
            if (inventory.AddItem(item))
            {
                Destroy(gameObject); // Remove pickup from scene
            }
        }
    }
}
