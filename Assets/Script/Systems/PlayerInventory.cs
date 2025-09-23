using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class PlayerInventory : MonoBehaviour
{
    public Inventory Inventory { get; private set; }
    void Awake() => Inventory = GetComponent<Inventory>();
}
