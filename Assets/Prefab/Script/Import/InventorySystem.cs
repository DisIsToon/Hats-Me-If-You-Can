using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySystem : MonoBehaviour
{
    public GameObject ItemInfoUI;
    public GameObject CraftingItemInfoUI;
    public static InventorySystem Instance { get; set; }

    public GameObject inventoryScreenUI;

    // Main inventory slots (tag "Slot")
    public List<GameObject> slotList = new List<GameObject>();

    // Quick slots (tag "QuickSlot")
    public List<GameObject> quickSlotList = new List<GameObject>();

    public List<string> itemList = new List<string>();

    private GameObject itemToAdd;
    private GameObject whatSlotToEquip;

    public bool isOpen;

    //Pickup Popup
    public GameObject pickupAlert;
    public TextMeshProUGUI pickupName;
    public Image pickupImage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (pickupAlert == null)
        {
            Debug.LogError("PickupAlert is not assigned in the Inspector!");
            return;
        }

        Transform textTransform = pickupAlert.transform.Find("Text");
        if (textTransform == null)
        {
            Debug.LogError("Could not find child named 'Text' under PickupAlert!");
            return;
        }

        pickupName = pickupAlert.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        isOpen = false;

        PopulateSlotList();
        PopulateQuickSlotList();
    }

    private void PopulateSlotList()
    {
        slotList.Clear();
        foreach (Transform child in inventoryScreenUI.transform)
        {
            if (child.CompareTag("Slot"))
            {
                slotList.Add(child.gameObject);
            }
        }
    }

    private void PopulateQuickSlotList()
    {
        quickSlotList.Clear();
        GameObject[] qs = GameObject.FindGameObjectsWithTag("QuickSlot");
        quickSlotList.AddRange(qs);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isOpen)
        {
            Debug.Log("i is pressed");
            inventoryScreenUI.SetActive(true);
            isOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.I) && isOpen)
        {
            inventoryScreenUI.SetActive(false);
            isOpen = false;
        }
    }

    public void OpenInventory()
    {
        if (!isOpen)
        {
            inventoryScreenUI.SetActive(true);
            isOpen = true;
        }
    }

    public void AddToInventory(string itemName)
    {
        // 1) Check if item already exists in inventory (main slots + quickslots)
        GameObject existingSlot = FindSlotWithItem(itemName);

        if (existingSlot != null)
        {
            // Increase stack amount
            IncreaseStack(existingSlot);

            // Show popup
            Sprite icon = existingSlot.transform.GetChild(0).GetComponent<Image>().sprite;
            TriggerPickupPopUp(itemName, icon);

            ReCalculateList();
            CraftingSystem.Instance.RefreshNeededItem();
            QuestManager.Instance.RefreshTrackerList();
            return;
        }

        // 2) Item does NOT exist → add a new slot in main inventory
        whatSlotToEquip = FindNextEmptySlot();

        itemToAdd = Instantiate(Resources.Load<GameObject>(itemName),
            whatSlotToEquip.transform.position,
            whatSlotToEquip.transform.rotation);

        itemToAdd.transform.SetParent(whatSlotToEquip.transform);
        itemToAdd.transform.localPosition = Vector3.zero;

        itemList.Add(itemName);

        TriggerPickupPopUp(itemName, itemToAdd.GetComponent<Image>().sprite);

        ReCalculateList();
        CraftingSystem.Instance.RefreshNeededItem();
        QuestManager.Instance.RefreshTrackerList();
    }

    private GameObject FindSlotWithItem(string itemName)
    {
        // Check main inventory slots
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
            {
                string childName = slot.transform.GetChild(0).name.Replace("(Clone)", "");
                if (childName == itemName)
                    return slot;
            }
        }

        // Check quickslots
        foreach (GameObject slot in quickSlotList)
        {
            if (slot.transform.childCount > 0)
            {
                string childName = slot.transform.GetChild(0).name.Replace("(Clone)", "");
                if (childName == itemName)
                    return slot;
            }
        }

        return null;
    }

    private void IncreaseStack(GameObject slot)
    {
        Transform item = slot.transform.GetChild(0);
        TextMeshProUGUI stackText = item.Find("StackText").GetComponent<TextMeshProUGUI>();

        int currentStack = 1;
        if (!string.IsNullOrEmpty(stackText.text))
            int.TryParse(stackText.text, out currentStack);

        currentStack++;
        stackText.text = currentStack.ToString();
    }

    void TriggerPickupPopUp(string itemName, Sprite itemSprite)
    {
        pickupAlert.SetActive(true);

        pickupName.text = itemName;
        pickupImage.sprite = itemSprite;

        StartCoroutine(DeactivatePickupAlert());
    }

    IEnumerator DeactivatePickupAlert()
    {
        yield return new WaitForSeconds(2f);
        pickupAlert.SetActive(false);
    }

    private GameObject FindNextEmptySlot()
    {
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount == 0)
            {
                return slot;
            }
        }
        return new GameObject();
    }

    public bool CheckSlotsAvailable(int emptyNeeded)
    {
        int emptySlot = 0;
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount <= 0)
            {
                emptySlot += 1;
            }
        }
        return emptySlot >= emptyNeeded;
    }

    // 🔹 Core remover: now checks main slots, then quickslots
    public void RemoveItem(string nameToRemove, int amountToRemove)
    {
        // Try main inventory slots first
        if (TryRemoveFromSlots(slotList, nameToRemove, amountToRemove))
        {
            ReCalculateList();
            CraftingSystem.Instance.RefreshNeededItem();
            QuestManager.Instance.RefreshTrackerList();
            return;
        }

        // Then quickslots (if item lives only in quickslot)
        if (TryRemoveFromSlots(quickSlotList, nameToRemove, amountToRemove))
        {
            ReCalculateList();
            CraftingSystem.Instance.RefreshNeededItem();
            QuestManager.Instance.RefreshTrackerList();
            return;
        }
    }

    private bool TryRemoveFromSlots(List<GameObject> slots, string nameToRemove, int amountToRemove)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) continue;
            if (slots[i].transform.childCount == 0) continue;

            GameObject child = slots[i].transform.GetChild(0).gameObject;
            string childName = child.name.Replace("(Clone)", "");

            if (childName == nameToRemove)
            {
                TextMeshProUGUI stackText = child.transform.Find("StackText").GetComponent<TextMeshProUGUI>();

                int currentStack = 1;
                int.TryParse(stackText.text, out currentStack);

                if (currentStack > amountToRemove)
                {
                    currentStack -= amountToRemove;
                    stackText.text = currentStack.ToString();
                }
                else
                {
                    // delete item completely from this slot
                    Destroy(child);
                }

                return true;
            }
        }
        return false;
    }

    public void ReCalculateList()
    {
        itemList.Clear();

        // Rebuild from main slots
        RecalcFromSlotCollection(slotList);

        // And from quickslots if you want them to count as owned items
        RecalcFromSlotCollection(quickSlotList);
    }

    private void RecalcFromSlotCollection(List<GameObject> slots)
    {
        foreach (GameObject slot in slots)
        {
            if (slot == null) continue;
            if (slot.transform.childCount > 0)
            {
                GameObject item = slot.transform.GetChild(0).gameObject;
                string cleanName = item.name.Replace("(Clone)", "");

                TextMeshProUGUI stackText = item.transform.Find("StackText").GetComponent<TextMeshProUGUI>();

                int stack = 1;
                int.TryParse(stackText.text, out stack);

                for (int i = 0; i < stack; i++)
                    itemList.Add(cleanName);
            }
        }
    }

    public int CheckItemAmount(string name)
    {
        int itemCounter = 0;
        foreach (string item in itemList)
        {
            if (item == name)
            {
                itemCounter++;
            }
        }
        return itemCounter;
    }
}
