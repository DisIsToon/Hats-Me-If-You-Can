using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    // --- Is this item trashable --- //
    public bool isTrashable;

    // --- Item Info UI --- //
    public GameObject itemInfoUI;
    public GameObject craftingItemInfoUI;

    public TextMeshProUGUI itemInfoUI_itemName;
    public TextMeshProUGUI itemInfoUI_itemDescription;
    public TextMeshProUGUI itemInfoUI_itemFunctionality;

    public TextMeshProUGUI craftingItemInfoUI_itemName;
    public TextMeshProUGUI craftingItemInfoUI_itemDescription;
    public TextMeshProUGUI craftingItemInfoUI_itemFunctionality;

    // --- New Icon Fields --- //
    public Image itemInfoUI_itemIcon;
    public Image craftingItemInfoUI_itemIcon;

    // icon for this item
    public Sprite iconForThisItem;

    public string thisName, thisDescription, thisFunctionality;

    // --- Consumption --- //
    private GameObject itemPendingConsumption;
    public bool isConsumable;

    public float healthEffect;
    public float hungerEffect;

    public bool isEquippable;
    private GameObject itemPendingEquipping;
    public bool isNowInsideQuickSlot;

    public bool isSelected;

    public bool isUseable;

    public List<ItemIconData> itemIcons = new List<ItemIconData>();

    [System.Serializable]
    public class ItemIconData
    {
        public string itemName;
        public Sprite icon;
    }

    private void Start()
    {
        itemInfoUI = InventorySystem.Instance.ItemInfoUI;
        craftingItemInfoUI = InventorySystem.Instance.CraftingItemInfoUI;

        itemInfoUI_itemName = itemInfoUI.transform.Find("itemName").GetComponent<TextMeshProUGUI>();
        itemInfoUI_itemDescription = itemInfoUI.transform.Find("itemDescription").GetComponent<TextMeshProUGUI>();
        itemInfoUI_itemFunctionality = itemInfoUI.transform.Find("itemFunctionality").GetComponent<TextMeshProUGUI>();

        craftingItemInfoUI_itemName = craftingItemInfoUI.transform.Find("itemName").GetComponent<TextMeshProUGUI>();
        craftingItemInfoUI_itemDescription = craftingItemInfoUI.transform.Find("itemDescription").GetComponent<TextMeshProUGUI>();
        craftingItemInfoUI_itemFunctionality = craftingItemInfoUI.transform.Find("itemFunctionality").GetComponent<TextMeshProUGUI>();

        // --- Find the icon components --- //
        itemInfoUI_itemIcon = itemInfoUI.transform.Find("itemIcon").GetComponent<Image>();
        craftingItemInfoUI_itemIcon = craftingItemInfoUI.transform.Find("itemIcon").GetComponent<Image>();
    }

    void Update()
    {
        if (isSelected)
            gameObject.GetComponent<DragDrop>().enabled = false;
        else
            gameObject.GetComponent<DragDrop>().enabled = true;
    }

    private Sprite LoadIcon(string itemName)
    {
        return Resources.Load<Sprite>("ItemIcons/" + itemName);
    }


    private Sprite GetIconByName(string name)
    {
        foreach (var data in itemIcons)
        {
            if (data.itemName == name)
                return data.icon;
        }

        return null; // if no match
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CraftingSystem.Instance.isOpen)
        {
            craftingItemInfoUI.SetActive(true);

            craftingItemInfoUI_itemName.text = thisName;
            craftingItemInfoUI_itemDescription.text = thisDescription;
            craftingItemInfoUI_itemFunctionality.text = thisFunctionality;

            craftingItemInfoUI_itemIcon.sprite = LoadIcon(thisName);

        }
        else
        {
            itemInfoUI.SetActive(true);

            itemInfoUI_itemName.text = thisName;
            itemInfoUI_itemDescription.text = thisDescription;
            itemInfoUI_itemFunctionality.text = thisFunctionality;

            itemInfoUI_itemIcon.sprite = LoadIcon(thisName);
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        itemInfoUI.SetActive(false);
        craftingItemInfoUI.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isConsumable)
            {
                itemPendingConsumption = gameObject;
            }

            if (isEquippable && isNowInsideQuickSlot == false && EquipSystem.Instance.CheckIfFull() == false)
            {
                EquipSystem.Instance.AddToQuickSlots(gameObject);
                isNowInsideQuickSlot = true;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isConsumable && itemPendingConsumption == gameObject)
            {
                DestroyImmediate(gameObject);
                InventorySystem.Instance.ReCalculateList();
            }
        }
    }

    private void UseItem()
    {
        itemInfoUI.SetActive(false);
        craftingItemInfoUI.SetActive(false);

        InventorySystem.Instance.isOpen = false;
        InventorySystem.Instance.inventoryScreenUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SelectionManager.Instance.EnableSelection();
        SelectionManager.Instance.enabled = true;
    }
}
