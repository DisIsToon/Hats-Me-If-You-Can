using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewHatalougeManager : MonoBehaviour
{
    public static NewHatalougeManager Instance { get; private set; }

    [Header("Main Journal Panel")]
    public GameObject journalPanel;
    public bool isOpen = false;

    [Header("Section Parents")]
    public GameObject hatSection;
    public GameObject questSection;
    public GameObject characterSection;
    public GameObject infoSection;
    public GameObject mapSection;

    [Header("Pressed Images")]
    public GameObject hatPressed;
    public GameObject questPressed;
    public GameObject characterPressed;
    public GameObject infoPressed;
    public GameObject mapPressed;

    // ---------------------------------------------------------
    // HAT ENCYCLOPEDIA (3 profiles)
    // ---------------------------------------------------------
    [Header("Hat Encyclopedia Pages (Hidden / Found)")]
    public GameObject[] hatHiddenScreens;      // 0 = Shy, 1 = Lazy, 2 = Fast
    public GameObject[] hatFoundScreens;       // same indexing
    private int hatIndex = 0;

    [Header("Hat Found Bools")]
    public bool shyFound = false;
    public bool lazyFound = false;
    public bool fastFound = false;

    // ---------------------------------------------------------
    // QUEST ENCYCLOPEDIA (3 pages)
    // ---------------------------------------------------------
    [Header("Quest Encyclopedia Pages (Hidden / Found)")]
    public GameObject[] questHiddenScreens;    // size 3
    public GameObject[] questFoundScreens;     // size 3
    private int questIndex = 0;

    [Header("Quest Found Bools")]
    public bool quest1Found = false;
    public bool quest2Found = false;
    public bool quest3Found = false;

    // ---------------------------------------------------------
    // CHARACTER ENCYCLOPEDIA (8 pages)
    // ---------------------------------------------------------
    [Header("Character Encyclopedia Pages (Hidden / Found)")]
    public GameObject[] characterHiddenScreens;   // size 8
    public GameObject[] characterFoundScreens;    // size 8
    private int characterIndex = 0;

    [Header("Character Found Bools")]
    public bool headMasterFound = false;
    public bool clumsiFound = false;
    public bool ivyFound = false;
    public bool chaseFound = false;
    public bool louierFound = false;
    public bool liraFound = false;
    public bool mallowFound = false;
    public bool tulipFound = false;

    // ---------------------------------------------------------
    // INFO ENCYCLOPEDIA (5 pages)
    // ---------------------------------------------------------
    [Header("Info Encyclopedia Pages (Exactly 5)")]
    public GameObject[] infoScreens;      // set to 5 in inspector
    public GameObject infoLeftBtn;
    public GameObject infoRightBtn;
    private int infoIndex = 0;

    // ---------------------------------------------------------
    // MAP ENCYCLOPEDIA (Default, Forest, Winter, Castle, Complete)
    // ---------------------------------------------------------
    [Header("Map Encyclopedia Screens")]
    public GameObject mapDefault;
    public GameObject mapForest;
    public GameObject mapWinter;
    public GameObject mapCastle;
    public GameObject mapComplete;

    private bool reachedForest = false;
    private bool reachedWinter = false;
    private bool reachedCastle = false;

    // ---------------------------------------------------------
    // Unity callbacks
    // ---------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        // Ensure initial UI state is correct if journal is active or not
        if (journalPanel != null && journalPanel.activeSelf)
        {
            isOpen = true;
            OpenSection(0); // default to Hat
        }
        else
        {
            isOpen = false;
            // Ensure sections are hidden if journal closed
            if (hatSection != null) hatSection.SetActive(false);
            if (questSection != null) questSection.SetActive(false);
            if (characterSection != null) characterSection.SetActive(false);
            if (infoSection != null) infoSection.SetActive(false);
            if (mapSection != null) mapSection.SetActive(false);
        }

        // Initialize info buttons visibility
        UpdateInfoScreen();
        // Initialize map screen
        UpdateMapScreen();
    }

    private void Update()
    {
        // Toggle journal with H key
        if (Input.GetKeyDown(KeyCode.H) && !isOpen)
            OpenJournal();
        else if (Input.GetKeyDown(KeyCode.H) && isOpen)
            CloseJournal();
    }

    // ---------------------------------------------------------
    // Journal open / close
    // ---------------------------------------------------------
    public void OpenJournal()
    {
        if (journalPanel != null) journalPanel.SetActive(true);
        isOpen = true;
        OpenSection(0);
    }

    public void CloseJournal()
    {
        if (journalPanel != null) journalPanel.SetActive(false);
        isOpen = false;
    }

    public void ToggleJournal()
    {
        if (!isOpen) OpenJournal();
        else CloseJournal();
    }

    // ---------------------------------------------------------
    // SECTION SWITCHING
    // ---------------------------------------------------------
    public void OpenSection(int index)
    {
        // Hide all sections first
        if (hatSection != null) hatSection.SetActive(false);
        if (questSection != null) questSection.SetActive(false);
        if (characterSection != null) characterSection.SetActive(false);
        if (infoSection != null) infoSection.SetActive(false);
        if (mapSection != null) mapSection.SetActive(false);

        // Disable all pressed images
        if (hatPressed != null) hatPressed.SetActive(false);
        if (questPressed != null) questPressed.SetActive(false);
        if (characterPressed != null) characterPressed.SetActive(false);
        if (infoPressed != null) infoPressed.SetActive(false);
        if (mapPressed != null) mapPressed.SetActive(false);

        // Activate requested section + pressed image + update its screens
        switch (index)
        {
            case 0: // Hat
                if (hatSection != null) hatSection.SetActive(true);
                if (hatPressed != null) hatPressed.SetActive(true);
                UpdateHatScreen();
                break;

            case 1: // Quest
                if (questSection != null) questSection.SetActive(true);
                if (questPressed != null) questPressed.SetActive(true);
                UpdateQuestScreen();
                break;

            case 2: // Character
                if (characterSection != null) characterSection.SetActive(true);
                if (characterPressed != null) characterPressed.SetActive(true);
                UpdateCharacterScreen();
                break;

            case 3: // Info
                if (infoSection != null) infoSection.SetActive(true);
                if (infoPressed != null) infoPressed.SetActive(true);
                UpdateInfoScreen();
                break;

            case 4: // Map
                if (mapSection != null) mapSection.SetActive(true);
                if (mapPressed != null) mapPressed.SetActive(true);
                UpdateMapScreen();
                break;
        }
    }

    // ---------------------------------------------------------
    // HAT ENCYCLOPEDIA (3 profiles) - only one profile visible at a time
    // ---------------------------------------------------------
    public void NextHat()
    {
        if (hatHiddenScreens == null || hatHiddenScreens.Length == 0) return;
        hatIndex = (hatIndex + 1) % hatHiddenScreens.Length;
        UpdateHatScreen();
    }

    public void PrevHat()
    {
        if (hatHiddenScreens == null || hatHiddenScreens.Length == 0) return;
        hatIndex--;
        if (hatIndex < 0) hatIndex = hatHiddenScreens.Length - 1;
        UpdateHatScreen();
    }

    private void UpdateHatScreen()
    {
        // Defensive checks
        if (hatHiddenScreens == null || hatHiddenScreens.Length == 0) return;

        // Hide all hidden/found screens first so only one appears
        for (int i = 0; i < hatHiddenScreens.Length; i++)
            if (hatHiddenScreens[i] != null)
                hatHiddenScreens[i].SetActive(false);

        if (hatFoundScreens != null)
        {
            for (int i = 0; i < hatFoundScreens.Length; i++)
                if (hatFoundScreens[i] != null)
                    hatFoundScreens[i].SetActive(false);
        }

        // Determine if current index is found or hidden
        bool found = false;
        if (hatIndex == 0) found = shyFound;
        else if (hatIndex == 1) found = lazyFound;
        else if (hatIndex == 2) found = fastFound;

        // Activate only the current profile's hidden or found screen
        if (found && hatFoundScreens != null && hatIndex < hatFoundScreens.Length && hatFoundScreens[hatIndex] != null)
            hatFoundScreens[hatIndex].SetActive(true);
        else if (hatHiddenScreens[hatIndex] != null)
            hatHiddenScreens[hatIndex].SetActive(true);
    }

    // Public unlockers for hats
    public void DiscoverShyHat() { shyFound = true; UpdateHatScreen(); }
    public void DiscoverLazyHat() { lazyFound = true; UpdateHatScreen(); }
    public void DiscoverFastHat() { fastFound = true; UpdateHatScreen(); }

    // ---------------------------------------------------------
    // QUEST ENCYCLOPEDIA (3 pages) - same behavior as Hat
    // ---------------------------------------------------------
    public void NextQuest()
    {
        if (questHiddenScreens == null || questHiddenScreens.Length == 0) return;
        questIndex = (questIndex + 1) % questHiddenScreens.Length;
        UpdateQuestScreen();
    }

    public void PrevQuest()
    {
        if (questHiddenScreens == null || questHiddenScreens.Length == 0) return;
        questIndex--;
        if (questIndex < 0) questIndex = questHiddenScreens.Length - 1;
        UpdateQuestScreen();
    }

    private void UpdateQuestScreen()
    {
        if (questHiddenScreens == null || questHiddenScreens.Length == 0) return;

        // Hide all
        for (int i = 0; i < questHiddenScreens.Length; i++)
            if (questHiddenScreens[i] != null)
                questHiddenScreens[i].SetActive(false);

        if (questFoundScreens != null)
        {
            for (int i = 0; i < questFoundScreens.Length; i++)
                if (questFoundScreens[i] != null)
                    questFoundScreens[i].SetActive(false);
        }

        // Determine found state for current index
        bool found = false;
        if (questIndex == 0) found = quest1Found;
        else if (questIndex == 1) found = quest2Found;
        else if (questIndex == 2) found = quest3Found;

        // Activate appropriate screen
        if (found && questFoundScreens != null && questIndex < questFoundScreens.Length && questFoundScreens[questIndex] != null)
            questFoundScreens[questIndex].SetActive(true);
        else if (questHiddenScreens[questIndex] != null)
            questHiddenScreens[questIndex].SetActive(true);
    }

    public void DiscoverQuest1() { quest1Found = true; UpdateQuestScreen(); }
    public void DiscoverQuest2() { quest2Found = true; UpdateQuestScreen(); }
    public void DiscoverQuest3() { quest3Found = true; UpdateQuestScreen(); }

    // ---------------------------------------------------------
    // CHARACTER ENCYCLOPEDIA (8 pages) - same system
    // ---------------------------------------------------------
    public void NextCharacter()
    {
        if (characterHiddenScreens == null || characterHiddenScreens.Length == 0) return;
        characterIndex = (characterIndex + 1) % characterHiddenScreens.Length;
        UpdateCharacterScreen();
    }

    public void PrevCharacter()
    {
        if (characterHiddenScreens == null || characterHiddenScreens.Length == 0) return;
        characterIndex--;
        if (characterIndex < 0) characterIndex = characterHiddenScreens.Length - 1;
        UpdateCharacterScreen();
    }

    private void UpdateCharacterScreen()
    {
        if (characterHiddenScreens == null || characterHiddenScreens.Length == 0) return;

        // Hide all
        for (int i = 0; i < characterHiddenScreens.Length; i++)
            if (characterHiddenScreens[i] != null)
                characterHiddenScreens[i].SetActive(false);

        if (characterFoundScreens != null)
        {
            for (int i = 0; i < characterFoundScreens.Length; i++)
                if (characterFoundScreens[i] != null)
                    characterFoundScreens[i].SetActive(false);
        }

        // Check found state for index
        bool found = IsCharacterFound(characterIndex);

        // Activate appropriate screen
        if (found && characterFoundScreens != null && characterIndex < characterFoundScreens.Length && characterFoundScreens[characterIndex] != null)
            characterFoundScreens[characterIndex].SetActive(true);
        else if (characterHiddenScreens[characterIndex] != null)
            characterHiddenScreens[characterIndex].SetActive(true);
    }

    private bool IsCharacterFound(int idx)
    {
        switch (idx)
        {
            case 0: return headMasterFound;
            case 1: return clumsiFound;
            case 2: return ivyFound;
            case 3: return chaseFound;
            case 4: return louierFound;
            case 5: return liraFound;
            case 6: return mallowFound;
            case 7: return tulipFound;
            default: return false;
        }
    }

    public void DiscoverCharHeadMaster() { headMasterFound = true; UpdateCharacterScreen(); }
    public void DiscoverCharClumsi() { clumsiFound = true; UpdateCharacterScreen(); }
    public void DiscoverCharIvy() { ivyFound = true; UpdateCharacterScreen(); }
    public void DiscoverCharChase() { chaseFound = true; UpdateCharacterScreen(); }
    public void DiscoverCharLouire() { louierFound = true; UpdateCharacterScreen(); }
    public void DiscoverCharLira() { liraFound = true; UpdateCharacterScreen(); }
    public void DiscoverCharMallow() { mallowFound = true; UpdateCharacterScreen(); }
    public void DiscoverCharTulip() { tulipFound = true; UpdateCharacterScreen(); }

    // ---------------------------------------------------------
    // INFO ENCYCLOPEDIA (5 pages) - show only one, update left/right buttons
    // ---------------------------------------------------------
    public void NextInfo()
    {
        if (infoScreens == null || infoScreens.Length == 0) return;
        infoIndex++;
        if (infoIndex >= infoScreens.Length) infoIndex = infoScreens.Length - 1;
        UpdateInfoScreen();
    }

    public void PrevInfo()
    {
        if (infoScreens == null || infoScreens.Length == 0) return;
        infoIndex--;
        if (infoIndex < 0) infoIndex = 0;
        UpdateInfoScreen();
    }

    private void UpdateInfoScreen()
    {
        if (infoScreens == null || infoScreens.Length == 0)
        {
            if (infoLeftBtn != null) infoLeftBtn.SetActive(false);
            if (infoRightBtn != null) infoRightBtn.SetActive(false);
            return;
        }

        // Show only the current info screen
        for (int i = 0; i < infoScreens.Length; i++)
            if (infoScreens[i] != null)
                infoScreens[i].SetActive(i == infoIndex);

        // Update arrows
        if (infoLeftBtn != null) infoLeftBtn.SetActive(infoIndex > 0);
        if (infoRightBtn != null) infoRightBtn.SetActive(infoIndex < infoScreens.Length - 1);
    }

    // ---------------------------------------------------------
    // MAP ENCYCLOPEDIA
    // ---------------------------------------------------------
    public void ReachForest()
    {
        reachedForest = true;
        UpdateMapScreen();
    }

    public void ReachWinter()
    {
        reachedWinter = true;
        UpdateMapScreen();
    }

    public void ReachCastle()
    {
        reachedCastle = true;
        UpdateMapScreen();
    }

    private void UpdateMapScreen()
    {
        if (mapDefault != null) mapDefault.SetActive(false);
        if (mapForest != null) mapForest.SetActive(false);
        if (mapWinter != null) mapWinter.SetActive(false);
        if (mapCastle != null) mapCastle.SetActive(false);
        if (mapComplete != null) mapComplete.SetActive(false);

        // If all reached -> show complete
        if (reachedForest && reachedWinter && reachedCastle)
        {
            if (mapComplete != null) mapComplete.SetActive(true);
            return;
        }

        if (reachedCastle)
        {
            if (mapCastle != null) mapCastle.SetActive(true);
            return;
        }

        if (reachedWinter)
        {
            if (mapWinter != null) mapWinter.SetActive(true);
            return;
        }

        if (reachedForest)
        {
            if (mapForest != null) mapForest.SetActive(true);
            return;
        }

        if (mapDefault != null) mapDefault.SetActive(true);
    }
}
