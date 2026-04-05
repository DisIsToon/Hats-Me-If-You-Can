    using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySystem : MonoBehaviour, IDataPersistence
{
    public GameObject ItemInfoUI;

    public GameObject CraftingItemInfoUI;
    public static InventorySystem Instance { get; set; }

    public GameObject inventoryScreenUI;

    public List<GameObject>slotList = new List<GameObject>();

    public List<string> itemList = new List<string>();
    
    private GameObject itemToAdd;

    private GameObject whatSlotToEquip;
    public GameObject inventoryKeybindHint;

    //public bool isFull;

    public bool isOpen;
    private bool tabPressedWhileOpen = false;


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

        foreach (GameObject slot in slotList)
        {
            foreach (Transform child in slot.transform)
            {
                Destroy(child.gameObject);
            }
        }

    }

    public void SaveData(GameData data)
    {
        if (NewHatalougeManager.Instance != null &&
            NewHatalougeManager.Instance.notTutorial == false)
            return;

        data.inventoryItems.Clear();
        data.inventoryStacks.Clear();

        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
            {
                GameObject item = slot.transform.GetChild(0).gameObject;
                string itemName = item.name.Replace("(Clone)", "");

                TextMeshProUGUI stackText = item.transform.Find("StackText").GetComponent<TextMeshProUGUI>();

                int stack = 1;
                int.TryParse(stackText.text, out stack);

                data.inventoryItems.Add(itemName);
                data.inventoryStacks.Add(stack);
            }
        }
    }

    public void LoadData(GameData data)
    {
        if (NewHatalougeManager.Instance != null &&
            NewHatalougeManager.Instance.notTutorial == false)
            return;

        // CLEAR EXISTING
        foreach (GameObject slot in slotList)
        {
            foreach (Transform child in slot.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // REBUILD
        for (int i = 0; i < data.inventoryItems.Count; i++)
        {
            string itemName = data.inventoryItems[i];
            int stack = data.inventoryStacks[i];

            GameObject slot = FindNextEmptySlot();

            GameObject item = Instantiate(Resources.Load<GameObject>(itemName),
                slot.transform);

            TextMeshProUGUI stackText = item.transform.Find("StackText").GetComponent<TextMeshProUGUI>();
            stackText.text = stack.ToString();
        }

        ReCalculateList();
    }

    private void PopulateSlotList()
    {
        slotList.Clear(); // ✅ IMPORTANT

        foreach (Transform child in inventoryScreenUI.transform)
        {
            if (child.CompareTag("Slot"))
            {
                slotList.Add(child.gameObject);
            }
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //  BLOCK if crafting is open
            if (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen)
                return;

            if (!isOpen)
            {
                OpenInventory();
            }
            else
            {
                tabPressedWhileOpen = true;
                CloseInventory();
            }
        }
    }

    public void OnInventoryButtonClicked()
    {
        // If inventory is open AND Tab was pressed → force close
        if (isOpen && tabPressedWhileOpen)
        {
            CloseInventory();
            tabPressedWhileOpen = false;
            return;
        }

        // Normal behavior
        if (!isOpen)
        {
            OpenInventory();
        }
        else
        {
            CloseInventory();
        }

        tabPressedWhileOpen = false;
    }



    public void OpenInventory()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);
        inventoryScreenUI.SetActive(true);
        isOpen = true;

        // Only show hint if crafting is NOT open
        if (CraftingSystem.Instance == null || !CraftingSystem.Instance.isOpen)
        {
            inventoryKeybindHint.SetActive(true);
        }
        else
        {
            inventoryKeybindHint.SetActive(false);
        }

        if (NewHatalougeManager.Instance != null &&
            NewHatalougeManager.Instance.notTutorial == false)
        {
            TutorialManager.Instance.OnInventoryOpened();
        }
    }

    public void CloseInventory()
    {
        inventoryScreenUI.SetActive(false);
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);

        isOpen = false;

        // Always hide hint when closing
        inventoryKeybindHint.SetActive(false);

        // Also close crafting if open
        if (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen)
        {
            CraftingSystem.Instance.CraftingScreenOff();
        }

        if (NewHatalougeManager.Instance != null &&
            NewHatalougeManager.Instance.notTutorial == false)
        {
            TutorialManager.Instance.OnInventoryClosed();
        }
    }

    public void AddToInventory(string itemName)
    {
        // Tutorial trigger
        if (NewHatalougeManager.Instance != null &&
            NewHatalougeManager.Instance.notTutorial == false)
        {
            TutorialManager.Instance.OnMaterialCollected(itemName);
        }

        // ✅ 0) Check QUICK SLOTS FIRST
        GameObject equippedSlot = EquipSystem.Instance.FindSlotWithItem(itemName);

        if (equippedSlot != null)
        {
            EquipSystem.Instance.IncreaseStack(equippedSlot);

            Sprite icon = equippedSlot.transform.GetChild(0).GetComponent<Image>().sprite;
            TriggerPickupPopUp(itemName, icon);

            ReCalculateList();
            CraftingSystem.Instance.RefreshNeededItem();
            QuestManager.Instance.RefreshTrackerList();
            return;
        }

        // ✅ 1) Check INVENTORY
        GameObject existingSlot = FindSlotWithItem(itemName);

        if (existingSlot != null)
        {
            IncreaseStack(existingSlot);

            Sprite icon = existingSlot.transform.GetChild(0).GetComponent<Image>().sprite;
            TriggerPickupPopUp(itemName, icon);

            ReCalculateList();
            CraftingSystem.Instance.RefreshNeededItem();
            QuestManager.Instance.RefreshTrackerList();
            return;
        }

        // ❗ 2) Create NEW
        whatSlotToEquip = FindNextEmptySlot();

        itemToAdd = Instantiate(Resources.Load<GameObject>(itemName),
            whatSlotToEquip.transform.position,
            whatSlotToEquip.transform.rotation);

        itemToAdd.transform.SetParent(whatSlotToEquip.transform);
        itemList.Add(itemName);

        TriggerPickupPopUp(itemName, itemToAdd.GetComponent<Image>().sprite);

        ReCalculateList();
        CraftingSystem.Instance.RefreshNeededItem();
        QuestManager.Instance.RefreshTrackerList();
    }

    private GameObject FindSlotWithItem(string itemName)
    {
        foreach (GameObject slot in slotList)
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
        foreach(GameObject slot in slotList)
        {
            if(slot.transform.childCount == 0)
            {
                return slot;
            }
        }
        return new GameObject();
    }

    public bool CheckSlotsAvailable(int emptyNeeded)
    {
        int emptySlot = 0;
        foreach(GameObject slot in slotList)
        {
            if(slot.transform.childCount <= 0)
            {
                emptySlot += 1;
            }
        }
        if(emptySlot >= emptyNeeded)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RemoveItem(string nameToRemove, int amountToRemove)
    {
        bool removedAny = false;

        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount == 0) continue;

            GameObject child = slot.transform.GetChild(0).gameObject;
            string childName = child.name.Replace("(Clone)", "");

            if (childName == nameToRemove)
            {
                TextMeshProUGUI stackText = child.transform.Find("StackText")?.GetComponent<TextMeshProUGUI>();

                int currentStack = 1;
                if (stackText != null && !string.IsNullOrEmpty(stackText.text))
                    int.TryParse(stackText.text, out currentStack);

                if (currentStack > amountToRemove)
                {
                    currentStack -= amountToRemove;
                    if (stackText != null) stackText.text = currentStack.ToString();
                }
                else
                {
                    Destroy(child);
                }

                removedAny = true;
                break; // remove only from one slot per call
            }
        }

        if (!removedAny)
            Debug.LogWarning($"Could not remove {nameToRemove}: not found in inventory!");

        ReCalculateList();
        CraftingSystem.Instance.RefreshNeededItem();
        QuestManager.Instance.RefreshTrackerList();
    }




    public void ReCalculateList()
    {
        itemList.Clear();

        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
            {
                GameObject item = slot.transform.GetChild(0).gameObject;
                string cleanName = item.name.Replace("(Clone)", "");

                TextMeshProUGUI stackText = item.transform.Find("StackText").GetComponent<TextMeshProUGUI>();

                int stack = 1;

                if (!int.TryParse(stackText.text, out stack) || stack < 1)
                {
                    stack = 1;
                }

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
            if(item == name)
            {
                itemCounter++;
            }    
        }
        return itemCounter;
    }
}
