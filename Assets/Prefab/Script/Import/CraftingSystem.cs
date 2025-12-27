using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSystem : MonoBehaviour
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject inventoryBTN;
    public GameObject quickSlots;

    [Header("UI Screens")]
    public GameObject craftingScreenUI;
    public GameObject inventoryScreenUI;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera craftingCamera;

    [Header("Fade Transition")]
    public Image fadeImage; // UI Image covering screen for fade (black or any color)
    public float fadeDuration = 0.5f;

    public bool canCraft = false;

    public List<string>inventoryItemList = new List<string>();

    //Craft Tools Buttons
    Button craftCakePotionBTN;
    Button craftCottonCrazeBTN;
    Button craftMirrorPotionBTN;

    //Requirement Tool Text
    TextMeshProUGUI cakePotionReq1, cakePotionReq2, cakePotionReq3;
    TextMeshProUGUI cottonCrazeReq1, cottonCrazeReq2, cottonCrazeReq3;
    TextMeshProUGUI mirrorPotionReq1, mirrorPotionReq2, mirrorPotionReq3;

    public bool isOpen;

    //All Blueprint
    // Tools
    public ItemBlueprints cakePotionBLP = new ItemBlueprints("CakePotion", 3, 2, "Star", 1, "Glitteroom", 2, "", 0);
    public ItemBlueprints cottonCrazeBLP = new ItemBlueprints("CottonCraze", 3, 2, "Star", 2, "Glitteroom", 3, "Cottonflower", 1);
    public ItemBlueprints mirrorPotionBLP = new ItemBlueprints("MirrorPotion", 3, 2, "Star", 3, "Glitteroom", 1, "Mirrorshard", 1);

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

        // CakePotion
        cakePotionReq1 = craftingScreenUI.transform.Find("CakePotion").transform.Find("req1").GetComponent<TextMeshProUGUI>();
        cakePotionReq2 = craftingScreenUI.transform.Find("CakePotion").transform.Find("req2").GetComponent<TextMeshProUGUI>();

        craftCakePotionBTN = craftingScreenUI.transform.Find("CakePotion").transform.Find("Button").GetComponent<Button>();
        craftCakePotionBTN.onClick.AddListener(delegate { CraftAnyItem(cakePotionBLP); });

        // CottonCraze
        cottonCrazeReq1 = craftingScreenUI.transform.Find("CottonCraze").transform.Find("req1").GetComponent<TextMeshProUGUI>();
        cottonCrazeReq2 = craftingScreenUI.transform.Find("CottonCraze").transform.Find("req2").GetComponent<TextMeshProUGUI>();
        cottonCrazeReq3 = craftingScreenUI.transform.Find("CottonCraze").transform.Find("req3").GetComponent<TextMeshProUGUI>();

        craftCottonCrazeBTN = craftingScreenUI.transform.Find("CottonCraze").transform.Find("Button").GetComponent<Button>();
        craftCottonCrazeBTN.onClick.AddListener(delegate { CraftAnyItem(cottonCrazeBLP); });

        // MirrorPotion
        mirrorPotionReq1 = craftingScreenUI.transform.Find("MirrorPotion").transform.Find("req1").GetComponent<TextMeshProUGUI>();
        mirrorPotionReq2 = craftingScreenUI.transform.Find("MirrorPotion").transform.Find("req2").GetComponent<TextMeshProUGUI>();
        mirrorPotionReq3 = craftingScreenUI.transform.Find("MirrorPotion").transform.Find("req3").GetComponent<TextMeshProUGUI>();

        craftMirrorPotionBTN = craftingScreenUI.transform.Find("MirrorPotion").transform.Find("Button").GetComponent<Button>();
        craftMirrorPotionBTN.onClick.AddListener(delegate { CraftAnyItem(mirrorPotionBLP); });
  
    }
    

    void CraftAnyItem(ItemBlueprints blueprintToCraft)
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);
        SoundManager.Instance.PlaySound(SoundManager.Instance.brewPotion);

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
        else if (blueprintToCraft.numOfRequirements == 3)
        {
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req1, blueprintToCraft.Req1Amount);
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req2, blueprintToCraft.Req2Amount);
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req3, blueprintToCraft.Req3Amount);
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
      
        yield return new WaitForSeconds(1f);
        SoundManager.Instance.brewPotion.Stop();
        // Produce the amount of items according to the blueprint
        for (var i = 0; i < blueprintToCraft.numOfItemsToProduce; i++)
        {
            InventorySystem.Instance.AddToInventory(blueprintToCraft.itemName);
            NotifUIManager.Instance.NotifyPotionBrewed(blueprintToCraft.itemName);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.E) && canCraft && !isOpen)
        {
            StartCoroutine(OpenCraftingScreen());
        }
        else if (Input.GetKeyDown(KeyCode.E) && isOpen)
        {
            StartCoroutine(CloseCraftingScreen());
        }

        if(canCraft ==  false && isOpen)
        {
            StartCoroutine(CloseCraftingScreen());
        }
    }
    public void SetCanCraft(bool value)
    {
        canCraft = value; 
    }

    IEnumerator OpenCraftingScreen()
    {
        mainScreen.SetActive(false);
        quickSlots.SetActive(false);
        inventoryBTN.SetActive(false);

        SoundManager.Instance.PlayBrewingMusic();
        isOpen = true;

        // Fade out
        yield return StartCoroutine(Fade(1f));

        // Switch camera
        mainCamera.gameObject.SetActive(false);
        craftingCamera.gameObject.SetActive(true);

        // Show UI
        craftingScreenUI.SetActive(true);
        inventoryScreenUI.SetActive(true);

        RefreshNeededItem();

        // Fade in
        yield return StartCoroutine(Fade(0f));

        SoundManager.Instance.PlayBrewingMusic();
    }

    IEnumerator CloseCraftingScreen()
    {
        mainScreen.SetActive(true);
        quickSlots.SetActive(true);
        inventoryBTN.SetActive(true);

        // Fade out
        yield return StartCoroutine(Fade(1f));

        // Hide UI
        craftingScreenUI.SetActive(false);
        inventoryScreenUI.SetActive(false);

        // Switch back camera
        craftingCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);

        isOpen = false;

        // Fade in
        yield return StartCoroutine(Fade(0f));

        SoundManager.Instance.ReturnToBiomeMusic();
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
    }

    public void CraftingScreenOff()
    {
        CloseCraftingScreen();
    }

    public void RefreshNeededItem()
    {
        int star_count = 0;
        int glitteroom_count = 0;
        int cottonflower_count = 0;
        int mirrorshard_count = 0;

        int stone_count = 0;
        int wood_count = 0;


        inventoryItemList = InventorySystem.Instance.itemList;

        foreach(string itemName in inventoryItemList)
        {
            switch(itemName)
            {
                case "Star":
                    star_count += 1;
                    break;
                case "Glitteroom":
                    glitteroom_count += 1;
                    break;
                case "Cottonflower":
                    cottonflower_count += 1;
                    break;
                case "Mirrorshard":
                    mirrorshard_count += 1;
                    break;
                case "Stone":
                    stone_count += 1;
                    break;
                case "Wood":
                    wood_count += 1;
                    break;

            }
        }

        //. ....Red CakePotion.... .//

        cakePotionReq1.text = "1 Star [" + star_count + "]";
        cakePotionReq2.text = "2 Glitteroom [" + glitteroom_count + "]";

        if (star_count >= 1 && glitteroom_count >= 2 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftCakePotionBTN.gameObject.SetActive(true);
        }
        else
        {
            craftCakePotionBTN.gameObject.SetActive(false);
        }

        //. ....CottonCraze.... .//

        cottonCrazeReq1.text = "2 Star [" + star_count + "]";
        cottonCrazeReq2.text = "3 Glitteroom [" + glitteroom_count + "]";
        cottonCrazeReq3.text = "1 Cottonflower [" + cottonflower_count + "]";

        if (star_count >= 2 && glitteroom_count >= 3 && cottonflower_count >= 1 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftCottonCrazeBTN.gameObject.SetActive(true);
        }
        else
        {
            craftCottonCrazeBTN.gameObject.SetActive(false);
        }

        //. ....MirrorPotion.... .//

        mirrorPotionReq1.text = "3 Star [" + star_count + "]";
        mirrorPotionReq2.text = "1 Glitteroom [" + glitteroom_count + "]";
        mirrorPotionReq3.text = "1 Mirrorshard [" + mirrorshard_count + "]";

        if (star_count >= 3 && glitteroom_count >= 1 && mirrorshard_count >= 1 && InventorySystem.Instance.CheckSlotsAvailable(1))
        {
            craftMirrorPotionBTN.gameObject.SetActive(true);
        }
        else
        {
            craftMirrorPotionBTN.gameObject.SetActive(false);
        }

    }
}
