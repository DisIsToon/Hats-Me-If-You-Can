using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSystem : MonoBehaviour
{
    public GameObject craftingScreenUI;
    public GameObject inventoryScreenUI;

    public List<string>inventoryItemList = new List<string>();

    //Craft Tools Buttons
    Button craftRedPotionBTN;
    Button craftBluePotionBTN;
    Button craftYellowPotionBTN;

    //Requirement Tool Text
    TextMeshProUGUI redPotionReq1, redPotionReq2;
    TextMeshProUGUI bluePotionReq1, bluePotionReq2;
    TextMeshProUGUI yellowPotionReq1, yellowPotionReq2;

    public bool isOpen;

    //All Blueprint
    // Tools
    public ItemBlueprints redPotionBLP = new ItemBlueprints("RedPotion", 1, 1, "Star", 1, "Wood", 0);
    public ItemBlueprints bluePotionBLP = new ItemBlueprints("BluePotion", 1, 1, "Star", 0, "Wood", 1);
    public ItemBlueprints yellowPotionBLP = new ItemBlueprints("YellowPotion", 1, 2, "Star", 2, "Wood", 2);

    public static CraftingSystem Instance { get; set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);    
        }
        else
        {
            Instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        isOpen = false;

        // RedPotion
        redPotionReq1 = craftingScreenUI.transform.Find("RedPotion").transform.Find("req1").GetComponent<TextMeshProUGUI>();
        redPotionReq2 = craftingScreenUI.transform.Find("RedPotion").transform.Find("req2").GetComponent<TextMeshProUGUI>();

        craftRedPotionBTN = craftingScreenUI.transform.Find("RedPotion").transform.Find("Button").GetComponent<Button>();
        craftRedPotionBTN.onClick.AddListener(delegate { CraftAnyItem(redPotionBLP); });

        // BluePotion
        bluePotionReq1 = craftingScreenUI.transform.Find("BluePotion").transform.Find("req1").GetComponent<TextMeshProUGUI>();
        bluePotionReq2 = craftingScreenUI.transform.Find("BluePotion").transform.Find("req2").GetComponent<TextMeshProUGUI>();

        craftBluePotionBTN = craftingScreenUI.transform.Find("BluePotion").transform.Find("Button").GetComponent<Button>();
        craftBluePotionBTN.onClick.AddListener(delegate { CraftAnyItem(bluePotionBLP); });

        // YellowPotion
        yellowPotionReq1 = craftingScreenUI.transform.Find("YellowPotion").transform.Find("req1").GetComponent<TextMeshProUGUI>();
        yellowPotionReq2 = craftingScreenUI.transform.Find("YellowPotion").transform.Find("req2").GetComponent<TextMeshProUGUI>();

        craftYellowPotionBTN = craftingScreenUI.transform.Find("YellowPotion").transform.Find("Button").GetComponent<Button>();
        craftYellowPotionBTN.onClick.AddListener(delegate { CraftAnyItem(yellowPotionBLP); });
  
    }
    

    void CraftAnyItem(ItemBlueprints blueprintToCraft)
    {
        //SoundManager.Instance.PlaySound(SoundManager.Instance.craftingSound);

        StartCoroutine(craftingDelayForSound(blueprintToCraft));
        
        

        if(blueprintToCraft.numOfRequirements == 1)
        {
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req1, blueprintToCraft.Req1Amount);
        }
        else if(blueprintToCraft.numOfRequirements == 2)
        {
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req1, blueprintToCraft.Req1Amount);
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req2, blueprintToCraft.Req2Amount);
        }

        StartCoroutine(calculate());
        
    }
    public IEnumerator calculate()
    {
        yield return 0;

        InventorySystem.Instance.ReCalculateList();
        RefreshNeededItem();   
    }

    IEnumerator craftingDelayForSound(ItemBlueprints blueprintToCraft)
    {
        VideoPlayerManager.Instance.PlayCraftVideo();
        yield return new WaitForSeconds(1f);

        //SoundManager.Instance.craftingSound.Stop();

        // Produce the amount of items according to the blueprint
        for (var i = 0; i < blueprintToCraft.numOfItemsToProduce; i++)
        {
            InventorySystem.Instance.AddToInventory(blueprintToCraft.itemName);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.C) && !isOpen)
        {
            RefreshNeededItem();
            craftingScreenUI.SetActive(true);
            inventoryScreenUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            SelectionManager.Instance.DisableSelection();
            SelectionManager.Instance.GetComponent<SelectionManager>().enabled = false;

            isOpen = true;
 
        }
        else if (Input.GetKeyDown(KeyCode.C) && isOpen)
        {
            RefreshNeededItem();
            craftingScreenUI.SetActive(false);
            inventoryScreenUI.SetActive(false);

            if(!InventorySystem.Instance.isOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                SelectionManager.Instance.EnableSelection();
                SelectionManager.Instance.GetComponent<SelectionManager>().enabled = true;
            }

            isOpen = false;
        }
    }

    public void RefreshNeededItem()
    {
        int star_count = 0;
        int stone_count = 0;
        int wood_count = 0;
        int string_count = 0;
        int feather_count = 0;
        int iron_count = 0;
        int coal_count = 0;
        int leather_count = 0;
        int rawMeat_count = 0;

        inventoryItemList = InventorySystem.Instance.itemList;

        foreach(string itemName in inventoryItemList)
        {
            switch(itemName)
            {
                case "Star":
                    star_count += 1;
                    break;
                case "Stone":
                    stone_count += 1;
                    break;
                case "Wood":
                    wood_count += 1;
                    break;
                case "String":
                    string_count += 1;
                    break;
                case "Feather":
                    feather_count += 1;
                    break;
                case "Iron":
                    iron_count += 1;
                    break;
                case "Coal":
                    coal_count += 1;
                    break;
                case "Leather":
                    leather_count += 1;
                    break;
                case "RawMeat":
                    rawMeat_count += 1;
                    break;
            }
        }

        /*

         */


        //. ....Red Potion.... .//

        redPotionReq1.text = "1 Star [" + star_count + "]";
        redPotionReq2.text = "0 Wood [" + wood_count + "]";

        // CheckSlotsAvailable(2) if 2 item will be crafted instead of 1
        if (star_count >= 1 && wood_count >= 0 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftRedPotionBTN.gameObject.SetActive(true);
        }
        else
        {
            craftRedPotionBTN.gameObject.SetActive(false);
        }

        //. ....Blue Potion.... .//

        bluePotionReq1.text = "0 Star [" + star_count + "]";
        bluePotionReq2.text = "1 Wood [" + wood_count + "]";

        // CheckSlotsAvailable(2) if 2 item will be crafted instead of 1
        if (star_count >= 0 && wood_count >= 1 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftBluePotionBTN.gameObject.SetActive(true);
        }
        else
        {
            craftBluePotionBTN.gameObject.SetActive(false);
        }

        //. ....Yellow Potion.... .//

        yellowPotionReq1.text = "2 Star [" + star_count + "]";
        yellowPotionReq2.text = "2 Wood [" + wood_count + "]";

        // CheckSlotsAvailable(2) if 2 item will be crafted instead of 1
        if (star_count >= 2 && wood_count >= 2 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftYellowPotionBTN.gameObject.SetActive(true);
        }
        else
        {
            craftYellowPotionBTN.gameObject.SetActive(false);
        }

    }
}
