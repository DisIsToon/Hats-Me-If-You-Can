using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HatalougeManager : MonoBehaviour
{
    public static HatalougeManager Instance { get; set; }

    public bool isOpen;

    [Header("Main Buttons")]
    public Button HatalougeBTN;
    public Button ExitBTN;

    [Header("Screens")]
    public GameObject HatalougeScreen;
    public GameObject HatEncyclopediaScreen;
    public GameObject PotionEncyclopediaScreen;
    public GameObject PlaceEncyclopediaScreen;

    [Header("Hat Buttons")]
    public Button ShyHatButton;
    public Button FastHatButton;
    public Button JumpHatButton;

    [Header("Hat Profile Screens")]
    public GameObject ShyHatProfileScreen;
    public GameObject FastHatProfileScreen;
    public GameObject JumpHatProfileScreen;

    [Header("Potion Buttons")]
    public Button RedPotionButton;
    public Button BluePotionButton;
    public Button YellowPotionButton;

    [Header("Potion Profile Screens")]
    public GameObject RedPotionProfileScreen;
    public GameObject BluePotionProfileScreen;
    public GameObject YellowPotionProfileScreen;

    [Header("Page Buttons")]
    public Button BTNHatsEncyclopediaPage;
    public Button BTNPotionEncyclopediaPage;
    public Button BTNEPlaceEncyclopediaPage;

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
        // Assign button listeners
        HatalougeBTN.onClick.AddListener(OpenHatalouge);

        ExitBTN.onClick.AddListener(CloseHatalouge);

        BTNHatsEncyclopediaPage.onClick.AddListener(OpenHatEncyclopedia);
        BTNPotionEncyclopediaPage.onClick.AddListener(OpenPotionEncyclopedia);
        BTNEPlaceEncyclopediaPage.onClick.AddListener(OpenPlaceEncyclopedia);

        ShyHatButton.onClick.AddListener(OpenShyHatProfile);
        FastHatButton.onClick.AddListener(OpenFastHatProfile);
        JumpHatButton.onClick.AddListener(OpenJumpHatProfile);

        RedPotionButton.onClick.AddListener(OpenRedPotionProfile);
        BluePotionButton.onClick.AddListener(OpenBluePotionProfile);
        YellowPotionButton.onClick.AddListener(OpenYellowPotionProfile);

        // Ensure everything is closed on start
        HatalougeScreen.SetActive(false);
        HatEncyclopediaScreen.SetActive(false);
        PotionEncyclopediaScreen.SetActive(false);
        PlaceEncyclopediaScreen.SetActive(false);
        ShyHatProfileScreen.SetActive(false);
        FastHatProfileScreen.SetActive(false);
        JumpHatProfileScreen.SetActive(false);
    }

    public void Update()
    {
        // H key opens the Hatalouge if not active
        if (Input.GetKeyDown(KeyCode.H) && !isOpen)
        {
            isOpen = true;
            OpenHatalouge();
        }

        // ESC key closes the whole Hatalouge
        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            isOpen = false;
            CloseHatalouge();
        }
        
    }

    public void OpenHatalouge()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SelectionManager.Instance.DisableSelection();
        SelectionManager.Instance.GetComponent<SelectionManager>().enabled = false;

        

        HatalougeScreen.SetActive(true);
        CloseAllEncyclopediaPages();
        CloseAllProfiles();
        ShyHatProfileScreen.SetActive(true);
        HatEncyclopediaScreen.SetActive(true);
    }

    public void CloseHatalouge()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SelectionManager.Instance.EnableSelection();
        SelectionManager.Instance.GetComponent<SelectionManager>().enabled = true;

        

        HatalougeScreen.SetActive(false);
        CloseAllEncyclopediaPages();
        CloseAllProfiles();
    }

    private void OpenHatEncyclopedia()
    {
        CloseAllEncyclopediaPages();
        CloseAllProfiles();
        HatEncyclopediaScreen.SetActive(true);
        ShyHatProfileScreen.SetActive(true);
    }

    private void OpenPotionEncyclopedia()
    {
        CloseAllEncyclopediaPages();
        CloseAllProfiles();
        PotionEncyclopediaScreen.SetActive(true);
        RedPotionProfileScreen.SetActive(true);
    }

    private void OpenPlaceEncyclopedia()
    {
        CloseAllEncyclopediaPages();
        PlaceEncyclopediaScreen.SetActive(true);
    }

    // Hat Profiles
    private void OpenShyHatProfile()
    {
        CloseAllProfiles();
        ShyHatProfileScreen.SetActive(true);
    }

    private void OpenFastHatProfile()
    {
        CloseAllProfiles();
        FastHatProfileScreen.SetActive(true);
    }

    private void OpenJumpHatProfile()
    {
        CloseAllProfiles();
        JumpHatProfileScreen.SetActive(true);
    }


    // Potions Profile
    private void OpenRedPotionProfile()
    {
        CloseAllProfiles();
        RedPotionProfileScreen.SetActive(true);
    }

    private void OpenBluePotionProfile()
    {
        CloseAllProfiles();
        BluePotionProfileScreen.SetActive(true);
    }

    private void OpenYellowPotionProfile()
    {
        CloseAllProfiles();
        YellowPotionProfileScreen.SetActive(true);
    }

    private void CloseAllEncyclopediaPages()
    {
        HatEncyclopediaScreen.SetActive(false);
        PotionEncyclopediaScreen.SetActive(false);
        PlaceEncyclopediaScreen.SetActive(false);
    }

    private void CloseAllProfiles()
    {
        ShyHatProfileScreen.SetActive(false);
        FastHatProfileScreen.SetActive(false);
        JumpHatProfileScreen.SetActive(false);

        RedPotionProfileScreen.SetActive(false);
        BluePotionProfileScreen.SetActive(false);
        YellowPotionProfileScreen.SetActive(false);
    }


}