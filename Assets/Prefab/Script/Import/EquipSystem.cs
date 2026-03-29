using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipSystem : MonoBehaviour, IDataPersistence
{
    public static EquipSystem Instance { get; set; }
 
    // -- UI -- //
    public GameObject quickSlotsPanel;
 
    public List<GameObject> quickSlotsList = new List<GameObject>();

    public GameObject numberHolder;
    public int selectedNumber = -1;
    public GameObject selectedItem;

    public GameObject toolHolder;
    public GameObject selectedItemModel;
 
   
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
 
    
    private void Start()
    {
        PopulateSlotList();
    }

    public void SaveData(GameData data)
    {
        data.equippedItems.Clear();
        data.equippedStacks.Clear();

        foreach (GameObject slot in quickSlotsList)
        {
            if (slot.transform.childCount > 0)
            {
                GameObject item = slot.transform.GetChild(0).gameObject;
                string itemName = item.name.Replace("(Clone)", "");

                TextMeshProUGUI stackText = item.transform.Find("StackText")?.GetComponent<TextMeshProUGUI>();

                int stack = 1;
                if (stackText != null)
                    int.TryParse(stackText.text, out stack);

                data.equippedItems.Add(itemName);
                data.equippedStacks.Add(stack);
            }
        }
    }

    public void LoadData(GameData data)
    {
        // CLEAR
        foreach (GameObject slot in quickSlotsList)
        {
            foreach (Transform child in slot.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // REBUILD
        for (int i = 0; i < data.equippedItems.Count; i++)
        {
            string itemName = data.equippedItems[i];
            int stack = data.equippedStacks[i];

            GameObject slot = FindNextEmptySlot();

            GameObject item = Instantiate(Resources.Load<GameObject>(itemName),
                slot.transform);

            TextMeshProUGUI stackText = item.transform.Find("StackText")?.GetComponent<TextMeshProUGUI>();
            if (stackText != null)
                stackText.text = stack.ToString();
        }
    }


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectQuickSlot(1);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectQuickSlot(2);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectQuickSlot(3);
        }
    }

    public GameObject FindSlotWithItem(string itemName)
    {
        foreach (GameObject slot in quickSlotsList)
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

    public void IncreaseStack(GameObject slot)
    {
        Transform item = slot.transform.GetChild(0);

        TextMeshProUGUI stackText = item.Find("StackText")?.GetComponent<TextMeshProUGUI>();

        int currentStack = 1;

        if (stackText != null && !string.IsNullOrEmpty(stackText.text))
            int.TryParse(stackText.text, out currentStack);

        currentStack++;

        if (stackText != null)
            stackText.text = currentStack.ToString();
    }

    void SelectQuickSlot(int number)
    {
        if(checkIfSlotIsFull(number) == true)
        {
            if(selectedNumber != number)
            {
                selectedNumber = number; 

                // Unselect Previously selected item
                if(selectedItem != null)
                {
                    selectedItem.gameObject.GetComponent<InventoryItem>().isSelected = false;
                }
                selectedItem = GetSelectedItem(number);
                selectedItem.GetComponent<InventoryItem>().isSelected = true;

                SetEquippedModel(selectedItem);

                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.OnPotionEquippedFromQuickslot();
                }

                // ChanGe Color
                foreach (Transform child in numberHolder.transform)
                {
                    child.transform.Find("Text").GetComponent<TMP_Text>().color = Color.gray;
                }

                TMP_Text toBeChanged = numberHolder.transform.Find("number" + number).transform.Find("Text").GetComponent<TMP_Text>();

                toBeChanged.color = Color.white;
            }
            else // We are trying to select the same slot
            {
                selectedNumber = -1; //null

                // Unselect Previously selected item
                if(selectedItem != null)
                {
                    selectedItem.gameObject.GetComponent<InventoryItem>().isSelected = false;
                    selectedItem = null;
                }

                if(selectedItemModel != null)
                {
                    DestroyImmediate(selectedItemModel.gameObject);
                    selectedItemModel = null;
                }

                // ChanGe Color
                foreach (Transform child in numberHolder.transform)
                {
                    child.transform.Find("Text").GetComponent<TMP_Text>().color = Color.gray;
                }

            }
        }
    }


    private void SetEquippedModel(GameObject selectedItem)
    {
        if(selectedItemModel != null)
        {
            DestroyImmediate(selectedItemModel.gameObject);
            selectedItemModel = null;
        }

        string selectedItemName = selectedItem.name.Replace("(Clone)", "");
        selectedItemModel = Instantiate(Resources.Load<GameObject>(selectedItemName +"_Model"),
            new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
        selectedItemModel.transform.SetParent(toolHolder.transform, false); 

    }

    GameObject GetSelectedItem(int slotNumber)
    {
        return quickSlotsList[slotNumber -1].transform.GetChild(0).gameObject;
    }   

    bool checkIfSlotIsFull(int slotNumber)
    {
        if(quickSlotsList[slotNumber - 1].transform.childCount > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
 
    private void PopulateSlotList()
    {
        foreach (Transform child in quickSlotsPanel.transform)
        {
            if (child.CompareTag("QuickSlot"))
            {
                quickSlotsList.Add(child.gameObject);
            }
        }
    }
 
    public void AddToQuickSlots(GameObject itemToEquip)
    {
        // Find next free slot
        GameObject availableSlot = FindNextEmptySlot();
        // Set transform of our object
        itemToEquip.transform.SetParent(availableSlot.transform, false);

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnPotionPlacedInQuickslot();

        InventorySystem.Instance.ReCalculateList();
 
    }
 
 
    private GameObject FindNextEmptySlot()
    {
        foreach (GameObject slot in quickSlotsList)
        {
            if (slot.transform.childCount == 0)
            {
                return slot;
            }
        }
        return new GameObject();
    }
 
    public bool CheckIfFull()
    {
 
        int counter = 0;
 
        foreach (GameObject slot in quickSlotsList)
        {
            if (slot.transform.childCount > 0)
            {
                counter += 1;
            }
        }
 
        if (counter == 7)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsHoldingGatePass()
    {
        if (selectedItem != null)
        {
            return selectedItem.CompareTag("Gatepass");
        }
        return false;
    }

    public bool IsHoldingHumanVillageMap()
    {
        if (selectedItem != null)
        {
            return selectedItem.CompareTag("HumanMap");
        }
        return false;
    }


    public bool IsHoldingSpear()
    {
        if (selectedItem != null)
        {
            
            return selectedItem.CompareTag("Spear");
        }
        return false;
    }
    public int GetEquippedSpearCount()
    {
        int spearCount = 0;
        foreach (GameObject slot in quickSlotsList)
        {
            if (slot.transform.childCount > 0)
            {
                GameObject item = slot.transform.GetChild(0).gameObject;
                if (item.CompareTag("Spear"))
                {
                    Debug.Log("spearCount++");
                    spearCount++;
                }
            }
        }
        return spearCount;
    }

    public void RemoveSelectedItem()
    {
        if (selectedItem != null)
        {
            Destroy(selectedItem);
            selectedItem = null;
            selectedNumber = -1;
        }
    }

}
