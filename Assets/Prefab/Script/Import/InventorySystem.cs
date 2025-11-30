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

    public List<GameObject>slotList = new List<GameObject>();

    public List<string> itemList = new List<string>();
    
    private GameObject itemToAdd;

    private GameObject whatSlotToEquip;
    
    //public bool isFull;

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

    }

    private void PopulateSlotList()
    {
        foreach(Transform child in inventoryScreenUI.transform)
        {
            if(child.CompareTag("Slot"))
            {
                slotList.Add(child.gameObject);
            }
        }
    }
 
 
    void Update()
    {
 
        if (Input.GetKeyDown(KeyCode.I) && !isOpen)
        {
 
			Debug.Log("i is pressed");
            inventoryScreenUI.SetActive(true);
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;

            //SelectionManager.Instance.DisableSelection();
            //SelectionManager.Instance.GetComponent<SelectionManager>().enabled = false;

            isOpen = true;
 
        }
        else if (Input.GetKeyDown(KeyCode.I) && isOpen)
        {
            inventoryScreenUI.SetActive(false);

            if(!CraftingSystem.Instance.isOpen)
            {
                //Cursor.lockState = CursorLockMode.Locked;
                //Cursor.visible = false;

                //SelectionManager.Instance.EnableSelection();
                //SelectionManager.Instance.GetComponent<SelectionManager>().enabled = true;
            }

            isOpen = false;
        }
    }

    public void AddToInventory(string itemName)
    {
        whatSlotToEquip = FindNextEmptySlot();

        itemToAdd = Instantiate(Resources.Load<GameObject>(itemName),whatSlotToEquip.transform.position,whatSlotToEquip.transform.rotation);
        itemToAdd.transform.SetParent(whatSlotToEquip.transform);

        itemList.Add(itemName);

        TriggerPickupPopUp(itemName, itemToAdd.GetComponent<Image>().sprite);

        ReCalculateList();
        CraftingSystem.Instance.RefreshNeededItem();

        QuestManager.Instance.RefreshTrackerList();
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
        int counter = amountToRemove;

        for(var i = slotList.Count - 1; i >=0; i--)
        {
            if(slotList[i].transform.childCount > 0)
            {
                if(slotList[i].transform.GetChild(0).name == nameToRemove + "(Clone)" && counter !=0)
                {
                    Destroy(slotList[i].transform.GetChild(0).gameObject);

                    counter -= 1;
                }
            }
        }

        ReCalculateList();
        CraftingSystem.Instance.RefreshNeededItem();
        QuestManager.Instance.RefreshTrackerList();
    }

    public void ReCalculateList()
    {
        itemList.Clear();

        foreach(GameObject slot in slotList)
        {
            if(slot.transform.childCount > 0)
            {
                string name = slot.transform.GetChild(0).name; // Stone(Clone)
                string str2 = "(Clone)";
                string result = name.Replace(str2, "");

                itemList.Add(result);
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
