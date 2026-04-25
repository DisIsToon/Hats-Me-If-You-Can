using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("All Tutorial UI (Assign In Order 1–17)")]
    public GameObject[] tutorialSteps; // Size = 18 (index 1–17 used)

    [Header("Spots")]
    public GameObject movementSpot1;
    public GameObject movementSpot2;
    public GameObject jumpSpot;

    [Header("Materials Counter UI")]
    public GameObject materialsCounterUI;
    public TextMeshProUGUI starCounterText;
    public TextMeshProUGUI glitteroomCounterText;
    public TextMeshProUGUI mirrorshardCounterText;

    [Header("Material Targets")]
    public int targetStars = 3;
    public int targetGlitteroom = 1;
    public int targetMirrorshard = 1;


    [Header("Game Objects")]
    public GameObject inventoryicon;
    public GameObject quickSlotsIcon;
    public GameObject hatalougeIcon;
    public GameObject settingIcon;

    public GameObject Barrier1;
    public GameObject Barrier2;
    public GameObject Barrier3;
    public GameObject Barrier4;
    public GameObject Barrier5;

    // ------------------------
    // Internal State
    // ------------------------

    private int currentStep = 0;
    private bool[] stepCompleted;
    private bool tutorialActive = false;

    private int collectedStars = 0;
    private int collectedGlitteroom = 0;
    private int collectedMirrorshard = 0;

    private bool waitingForCraftOpen = false;
    private bool waitingForCraftFinish = false;
    private bool waitingForQuickslot = false;
    private bool waitingForPotionEquip = false;

    public bool IsAnyTutorialUIOpen
    {
        get
        {
            if (tutorialSteps == null) return false;

            foreach (GameObject ui in tutorialSteps)
            {
                if (ui != null && ui.activeSelf)
                    return true;
            }

            return false;
        }
    }

    void Awake()
    {
        Instance = this;
        stepCompleted = new bool[18]; // Steps 1–17
    }

    void Start()
    {
        HideAllUI();

        if (NewHatalougeManager.Instance != null &&
            NewHatalougeManager.Instance.notTutorial == false)
        {
            tutorialActive = true;
            inventoryicon.SetActive(false);
            quickSlotsIcon.SetActive(false);
            hatalougeIcon.SetActive(false); 
            settingIcon.SetActive(false);
            Barrier1.SetActive(true); 
            Barrier2.SetActive(true);
            Barrier3.SetActive(true);
            Barrier4.SetActive(true);
            Barrier5.SetActive(true);
            StartCoroutine(StartTutorial());
        }
        else
        {
            DisableAllSpots();
        }
    }

    // =========================================================
    // CORE STEP SYSTEM
    // =========================================================

    IEnumerator StartTutorial()
    {
        yield return new WaitForSeconds(1f);
        movementSpot1.SetActive(true);
        ShowStep(1);
    }

    void ShowStep(int stepNumber)
    {
        if (!tutorialActive) return;

        // Prevent skipping order
        if (stepNumber != currentStep + 1) return;

        // Prevent repeating completed step
        if (stepCompleted[stepNumber]) return;

        StartCoroutine(ShowStepWithDelay(stepNumber));
    }

    IEnumerator ShowStepWithDelay(int stepNumber)
    {
        HideCurrentStep();

        yield return new WaitForSeconds(1f); // 1 second delay

        currentStep = stepNumber;

        SoundManager.Instance.PlaySFX(SoundManager.Instance.openMessageSfx.clip);

        if (tutorialSteps[stepNumber] != null)
        {
            tutorialSteps[stepNumber].SetActive(true);
        }

    }


    void CompleteStep(int stepNumber)
    {
        if (stepNumber < stepCompleted.Length)
            stepCompleted[stepNumber] = true;
    }

    public void CloseCurrentTutorialUI()
    {
        if (currentStep > 0 && tutorialSteps[currentStep] != null)
        {
            tutorialSteps[currentStep].SetActive(false);
        }
    }


    void HideCurrentStep()
    {
        if (currentStep > 0 && tutorialSteps[currentStep] != null)
        {
            tutorialSteps[currentStep].SetActive(false);
        }
    }


    void HideAllUI()
    {
        foreach (GameObject ui in tutorialSteps)
        {
            if (ui != null)
                ui.SetActive(false);
        }
    }

    void DisableAllSpots()
    {
        movementSpot1.SetActive(false);
        movementSpot2.SetActive(false);
        jumpSpot.SetActive(false);
    }

    // =========================================================
    // HELP BUTTON
    // =========================================================

    public void ShowCurrentTutorialHelp()
    {
        if (!tutorialActive) return;

        if (currentStep > 0 && tutorialSteps[currentStep] != null)
            tutorialSteps[currentStep].SetActive(true);
    }

    // =========================================================
    // MOVEMENT SECTION
    // =========================================================

    public void MovementSpot1Reached()
    {
        if (currentStep != 1) return;

        CompleteStep(1);
        movementSpot1.SetActive(false);
        movementSpot2.SetActive(true);

        ShowStep(2);
        Barrier1.SetActive(false);
    }

    public void MovementSpot2Reached()
    {
        if (currentStep != 2) return;

        CompleteStep(2);
        movementSpot2.SetActive(false);
        jumpSpot.SetActive(true);

        ShowStep(3);
        Barrier2.SetActive(false);
        Barrier1.SetActive(true);
    }

    public void JumpSpotReached()
    {
        if (currentStep != 3) return;

        CompleteStep(3);
        jumpSpot.SetActive(false);

        ShowStep(4);
        Barrier2.SetActive(true);
        inventoryicon.SetActive(true);
    }

    // =========================================================
    // SYSTEM INTERACTIONS
    // =========================================================

    public void OnInventoryOpened()
    {
        if (currentStep != 4) return;

        CompleteStep(4);
        ShowStep(5);
    }

    public void OnInventoryClosed()
    {
        if (currentStep != 5) return;

        CompleteStep(5);
        ShowStep(6);
        hatalougeIcon.SetActive(true);
    }

    public void OnHatalogueOpened()
    {
        if (currentStep != 6) return;

        CompleteStep(6);
        ShowStep(7);
    }

    public void OnHatalogueClosed()
    {
        if (currentStep != 7) return;

        CompleteStep(7);
        ShowStep(8);
        settingIcon.SetActive(true);
    }

    public void OnSettingsOpened()
    {
        if (currentStep != 8) return;

        Time.timeScale = 1f;

        CompleteStep(8);
        ShowStep(9);
    }

    public void OnSettingsClosedForTutorial()
    {
        if (currentStep != 9) return;

        CompleteStep(9);
        ShowStep(10);

        Barrier3.SetActive(false);

        materialsCounterUI.SetActive(true);
        UpdateMaterialCounters();
    }

    // =========================================================
    // MATERIAL COLLECTION
    // =========================================================

    public void OnMaterialCollected(string itemName)
    {
        if (currentStep != 10) return;

        if (itemName == "Star")
            collectedStars = Mathf.Min(collectedStars + 1, targetStars);
        else if (itemName == "Glitteroom")
            collectedGlitteroom = Mathf.Min(collectedGlitteroom + 1, targetGlitteroom);
        else if (itemName == "Mirror Shard")
            collectedMirrorshard = Mathf.Min(collectedMirrorshard + 1, targetMirrorshard);
        else
            return;

        UpdateMaterialCounters();

        if (collectedStars >= targetStars &&
            collectedGlitteroom >= targetGlitteroom &&
            collectedMirrorshard >= targetMirrorshard)
        {
            CompleteStep(10);
            materialsCounterUI.SetActive(false);
            ShowStep(11);
            waitingForCraftOpen = true;
            Barrier3.SetActive(true);
            Barrier4.SetActive(false);
        }
    }

    void UpdateMaterialCounters()
    {
        starCounterText.text = $"[{collectedStars}/{targetStars} Star]";
        glitteroomCounterText.text = $"[{collectedGlitteroom}/{targetGlitteroom} Glitteroom]";
        mirrorshardCounterText.text = $"[{collectedMirrorshard}/{targetMirrorshard} Mirror Shard]";
    }

    // =========================================================
    // CRAFTING + QUICK SLOT
    // =========================================================

    public void OnCraftingScreenOpened()
    {
        if (!waitingForCraftOpen || currentStep != 11) return;

        waitingForCraftOpen = false;
        waitingForCraftFinish = true;

        CompleteStep(11);
        ShowStep(12);
    }

    public void CloseStep12UI()
    {
        if (tutorialSteps[12] != null)
        {
            tutorialSteps[12].SetActive(false);
        }
    }

    public void OnPotionCrafted()
    {
        if (!waitingForCraftFinish || currentStep != 12) return;

        waitingForCraftFinish = false;
        waitingForQuickslot = true;

        CompleteStep(12);
        ShowStep(13);
        quickSlotsIcon.SetActive(true);
    }

    public void OnPotionPlacedInQuickslot()
    {
        if (!waitingForQuickslot || currentStep != 13) return;

        if (CraftingSystem.Instance != null &&
            CraftingSystem.Instance.isOpen)
            return;

        waitingForQuickslot = false;
        waitingForPotionEquip = true;

        CompleteStep(13);
        ShowStep(14);
    }

    public void OnPotionEquippedFromQuickslot()
    {
        if (!waitingForPotionEquip || currentStep != 14) return;

        waitingForPotionEquip = false;

        CompleteStep(14);
        ShowStep(15);
        Barrier5.SetActive(false);
    }

    // =========================================================
    // MINIGAME TUTORIAL STEPS
    // =========================================================

    public void ShowTutorialUI16AndPause()
    {
        if (currentStep != 15) return;

        Time.timeScale = 1f;

        CompleteStep(15);
        ShowStep(16);
        
    }

    public void CloseTutorialUI16()
    {
        Time.timeScale = 1f;
    }

    public void ShowTutorialUI17()
    {
        if (!tutorialActive) return;

        StartCoroutine(Show17AfterDelay());
    }

    public bool IsStep17Showing()
    {
        return tutorialActive &&
               currentStep == 17 &&
               tutorialSteps[17] != null &&
               tutorialSteps[17].activeSelf;
    }

    IEnumerator Show17AfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);

        // Force progression safely
        if (currentStep < 16)
        {
            CompleteStep(15);
            currentStep = 16;
        }

        CompleteStep(16);
        ShowStep(17);
    }

    public void ExitTutorialSaveGameAndLoadScene()
    {
        // DataPersistenceManager.instance.SaveGame();

        SceneManager.LoadSceneAsync("BiomeOptimized");
    }
}
