using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    public int maxItems = 20;

    public GameObject inventoryPanel;
    public InventorySlot[] slots;
    public Canvas uiCanvas; // Assign your Canvas here in Inspector

    public bool AddItem(Item newItem)
    {
        if (items.Count >= maxItems)
        {
            Debug.Log("Inventory Full!");
            return false;
        }

        items.Add(newItem);
        UpdateUI();
        return true;
    }

    public void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                slots[i].AddItem(items[i], uiCanvas);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
    }
}
