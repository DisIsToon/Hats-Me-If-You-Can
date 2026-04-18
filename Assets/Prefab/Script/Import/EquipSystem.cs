using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipSystem : MonoBehaviour
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

    public GameObject equipmentHintUI;

    bool IsAnyMinigameMainScreenOpen()
    {
        if (fastHatMinigame != null && fastHatMinigame.mainScreen.activeSelf)
            return true;

        if (jumpHatMinigame != null && jumpHatMinigame.mainScreen.activeSelf)
            return true;

        if (shyHatMinigame != null && shyHatMinigame.mainScreen.activeSelf)
            return true;

        return false;
    }

    [Header("Minigame References")]
    public FastHatMinigame fastHatMinigame;
    public JumpHatMinigame jumpHatMinigame;
    public ShyHatMinigame shyHatMinigame;

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

    public void SaveData(GameData2 data)
    {
        data.equippedItems.Clear();
        data.equippedStacks.Clear();

        foreach (GameObject slot in quickSlotsList)
        {
            if (slot.transform.childCount > 0)
            {
                GameObject item = slot.transform.GetChild(0).gameObject;
                string itemName = item.name.Replace("(Clone)", "");

                TextMeshProUGUI stackText =
                    item.transform.Find("StackText")?.GetComponent<TextMeshProUGUI>();

                int stack = 1;
                if (stackText != null)
                    int.TryParse(stackText.text, out stack);

                data.equippedItems.Add(itemName);
                data.equippedStacks.Add(stack);
            }
            else
            {
                // IMPORTANT: preserve empty slot
                data.equippedItems.Add("");
                data.equippedStacks.Add(0);
            }
        }
    }

    public void LoadData(GameData2 data)
    {
        // CLEAR
        foreach (GameObject slot in quickSlotsList)
        {
            foreach (Transform child in slot.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // REBUILD EXACT SLOT INDEX
        for (int i = 0; i < data.equippedItems.Count; i++)
        {
            if (i >= quickSlotsList.Count) break;

            string itemName = data.equippedItems[i];
            int stack = data.equippedStacks[i];

            if (string.IsNullOrEmpty(itemName)) continue;

            GameObject slot = quickSlotsList[i];

            GameObject item = Instantiate(Resources.Load<GameObject>(itemName));
            item.transform.SetParent(slot.transform, false); // 🔥 IMPORTANT

            RectTransform rt = item.GetComponent<RectTransform>();

            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.sizeDelta = new Vector2(32, 32);

            TextMeshProUGUI stackText =
                item.transform.Find("StackText")?.GetComponent<TextMeshProUGUI>();

            if (stackText != null)
                stackText.text = stack.ToString();
        }
    }

    public void ConsumeSelectedItem()
    {
        if (selectedItem == null) return;

        string itemName = selectedItem.name.Replace("(Clone)", "");

        TextMeshProUGUI stackText = selectedItem.transform.Find("StackText")?.GetComponent<TextMeshProUGUI>();

        int currentStack = 1;

        if (stackText != null && !string.IsNullOrEmpty(stackText.text))
            int.TryParse(stackText.text, out currentStack);

        currentStack--;

        if (currentStack > 0)
        {
            // Update quickslot stack
            if (stackText != null)
                stackText.text = currentStack.ToString();

            //  RE-EQUIP AGAIN
            StartCoroutine(ReEquipDelay());
        }
        else
        {
            // Destroy from quickslot
            Destroy(selectedItem);
            selectedItem = null;
            selectedNumber = -1;
            equipmentHintUI.SetActive(false);

            if (selectedItemModel != null)
            {
                equipmentHintUI.SetActive(false);
                Destroy(selectedItemModel);
                selectedItemModel = null;
            }
        }

        // ALSO remove from inventory
        InventorySystem.Instance.RemoveItem(itemName, 1);
    }

    void ReEquipCurrentItem()
    {
        if (selectedItem == null) return;

        // Recreate the model in hand
        SetEquippedModel(selectedItem);
    }

    IEnumerator ReEquipDelay()
    {
        yield return new WaitForSeconds(0.05f);
        ReEquipCurrentItem();
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

        HandleEquipmentHintVisibility();
    }

    void HandleEquipmentHintVisibility()
    {
        // If any minigame main screen is open → ALWAYS hide hint
        if (IsAnyMinigameMainScreenOpen())
        {
            if (equipmentHintUI.activeSelf)
                equipmentHintUI.SetActive(false);

            return;
        }

        // If no minigame UI is open
        // Show hint ONLY if player is selecting an item
        if (selectedItem != null && selectedNumber != -1)
        {
            if (!equipmentHintUI.activeSelf)
                equipmentHintUI.SetActive(true);
        }
        else
        {
            if (equipmentHintUI.activeSelf)
                equipmentHintUI.SetActive(false);
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
