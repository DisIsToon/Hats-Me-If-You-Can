using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameTracker : MonoBehaviour, IDataPersistence
{
    public static GameTracker Instance { get; set; }

    [Header("Player Reference")]
    public GameObject player;  // Assign your Player here

    [Header("Hats/Bools")]
    public bool shyHatCaptured = false;
    public bool fastHatCaptured = false;
    public bool jumpHatCaptured = false;
    public bool openedFrozenGate = false;
    public bool puzzleComplete = false;

    [Header("Quest Completion Bools")]
    public bool questCompleteLira = false;
    public bool questCompleteMallow = false;
    public bool questCompleteTulip = false;

    [Header("Current Biome")]
    public string currentBiome = "None";

    [Header("Area Triggers")]
    public GameObject triggerForest1;
    public GameObject triggerForest2;
    public GameObject triggerForest3;
    public GameObject triggerCastle;
    public GameObject triggerWinter;
    // Notifications (only once)
    private bool castleRuinNotified = false;
    private bool winterBiomeNotified = false;
    private bool springGardenNotified = false;

    [Header("Potions")]
    public bool cakePotionUnlocked = false;
    public bool cottonCrazePotionUnlocked = false;
    public bool mirrorPotionUnlocked = false;
    public bool starPotionUnlocked = false;
    public bool herbPotionUnlocked = false;

    [Header("FROM SAVE SYSTEM")]
    public bool shyHatAlreadyCaptured;
    public bool fastHatAlreadyCaptured;
    public bool jumpHatAlreadyCaptured;

    public bool questAlreadyCompleteLira;
    public bool questAlreadyCompleteMallow;
    public bool questAlreadyCompleteTulip;

    public bool castleRuinAlreadyDiscovered;
    public bool winterBiomeAlreadyDiscovered;
    public bool springGardenAlreadyDiscovered;

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

    public void LoadData(GameData data)
    {
        // ----- LOAD RAW BOOLS -----
        this.shyHatAlreadyCaptured = data.shyHatAlreadyCaptured;
        this.fastHatAlreadyCaptured = data.fastHatAlreadyCaptured;
        this.jumpHatAlreadyCaptured = data.jumpHatAlreadyCaptured;

        this.questAlreadyCompleteLira = data.questAlreadyCompleteLira;
        this.questAlreadyCompleteMallow = data.questAlreadyCompleteMallow;
        this.questAlreadyCompleteTulip = data.questAlreadyCompleteTulip;

        this.castleRuinAlreadyDiscovered = data.castleRuinAlreadyDiscovered;
        this.winterBiomeAlreadyDiscovered = data.winterBiomeAlreadyDiscovered;
        this.springGardenAlreadyDiscovered = data.springGardenAlreadyDiscovered;


        // ============================================
        // --- APPLY EFFECTS BASED ON LOADED DATA ------
        // ============================================


        // ----- HATS -----
        if (shyHatAlreadyCaptured)
        {
            shyHatCaptured = true;
            NewHatalougeManager.Instance.DiscoverShyHat();
            // ADD LINE TO REMOVE THE HAT FROM THE SCENE
        }

        if (fastHatAlreadyCaptured)
        {
            fastHatCaptured = true;
            NewHatalougeManager.Instance.DiscoverFastHat();
            // ADD LINE TO REMOVE THE HAT FROM THE SCENE
        }

        if (jumpHatAlreadyCaptured)
        {
            jumpHatCaptured = true;
            NewHatalougeManager.Instance.DiscoverLazyHat();
            // ADD LINE TO REMOVE THE HAT FROM THE SCENE
        }


        // ----- QUESTS -----
        if (questAlreadyCompleteLira)
        {
            questCompleteLira = true;
            NewHatalougeManager.Instance.quest1Found = true;
            NewHatalougeManager.Instance.UpdateQuestScreen();
        }

        if (questAlreadyCompleteMallow)
        {
            questCompleteMallow = true;
            NewHatalougeManager.Instance.quest2Found = true;
            NewHatalougeManager.Instance.UpdateQuestScreen();
        }

        if (questAlreadyCompleteTulip)
        {
            questCompleteTulip = true;
            NewHatalougeManager.Instance.quest3Found = true;
            NewHatalougeManager.Instance.UpdateQuestScreen();
        }


        // ----- BIOMES -----
        if (springGardenAlreadyDiscovered)
        {
            springGardenNotified = true;
            NewHatalougeManager.Instance.ReachForest();
        }

        if (castleRuinAlreadyDiscovered)
        {
            castleRuinNotified = true;
            NewHatalougeManager.Instance.ReachCastle();
        }

        if (winterBiomeAlreadyDiscovered)
        {
            winterBiomeNotified = true;
            NewHatalougeManager.Instance.ReachWinter();
        }
    }


    public void SaveData(GameData data)
    {
        data.shyHatAlreadyCaptured = this.shyHatAlreadyCaptured;
        data.fastHatAlreadyCaptured = this.fastHatAlreadyCaptured;
        data.jumpHatAlreadyCaptured = this.jumpHatAlreadyCaptured;

        data.questAlreadyCompleteLira = this.questAlreadyCompleteLira;
        data.questAlreadyCompleteMallow = this.questAlreadyCompleteMallow;
        data.questAlreadyCompleteTulip = this.questAlreadyCompleteTulip;

        data.castleRuinAlreadyDiscovered = this.castleRuinAlreadyDiscovered;
        data.winterBiomeAlreadyDiscovered = this.winterBiomeAlreadyDiscovered;
        data.springGardenAlreadyDiscovered = this.springGardenAlreadyDiscovered;
    }

    private void Start()
    {
        if (player == null)
            Debug.LogError("Player GameObject not assigned in GameManager!");
    }

    public void SetCurrentBiome(string biomeName)
    {
        currentBiome = biomeName;
        Debug.Log("Current biome set to: " + biomeName);
    }

    public void CheckPlayerInTrigger(GameObject trigger)
    {
        // ---------------- FOREST ----------------
        if (trigger == triggerForest1 || trigger == triggerForest2 || trigger == triggerForest3)
        {
            Debug.Log("Player entered Forest!");
            SetCurrentBiome("Forest");

            SoundManager.Instance.SwitchBiomeMusic("Forest");


            // forest can notify multiple times — but only first time plays discovery
            if (!springGardenNotified)
            {
                
                springGardenNotified = true;
                NotifUIManager.Instance.NotifyBiomeDiscovered("Spring Garden");
                NewHatalougeManager.Instance.ReachForest();
            }
            return;
        }

        // ---------------- CASTLE ----------------
        if (trigger == triggerCastle)
        {
            Debug.Log("Player entered Castle Ruin!");
            SetCurrentBiome("CastleRuin");

            SoundManager.Instance.SwitchBiomeMusic("CastleRuin");

            if (!castleRuinNotified)
            {
                
                castleRuinNotified = true;
                NotifUIManager.Instance.NotifyBiomeDiscovered("Castle Ruin");
                NewHatalougeManager.Instance.ReachCastle();
            }
            return;
        }

        // ---------------- WINTER ----------------
        if (trigger == triggerWinter)
        {
            Debug.Log("Player entered Winter Biome!");
            SetCurrentBiome("Winter");

            SoundManager.Instance.SwitchBiomeMusic("Winter");

            if (!winterBiomeNotified)
            {
                
                winterBiomeNotified = true;
                NotifUIManager.Instance.NotifyBiomeDiscovered("Winter Biome");
                NewHatalougeManager.Instance.ReachWinter();
            }
            return;
        }
    }

    public void SetPuzzleComplete(bool value)
    {
        puzzleComplete = value;
        Debug.Log("Puzzle Complete Status Updated: " + value);
    }

    public bool IsPuzzleComplete()
    {
        return puzzleComplete;
    }
    // --- Capture a hat ---
    public void CaptureHat(string hatName)
    {
        switch (hatName)
        {
            case "ShyHat":
                shyHatCaptured = true;
                NewHatalougeManager.Instance.DiscoverShyHat();
                NotifUIManager.Instance.NotifyHatCaptured("ShyHat");
                Debug.Log("ShyHat captured!");
                break;
            case "FastHat":
                fastHatCaptured = true;
                NewHatalougeManager.Instance.DiscoverFastHat();
                NotifUIManager.Instance.NotifyHatCaptured("FastHat");
                Debug.Log("FastHat captured!");
                break;
            case "JumpHat":
                jumpHatCaptured = true;
                NewHatalougeManager.Instance.DiscoverLazyHat();
                NotifUIManager.Instance.NotifyHatCaptured("JumpHat");
                Debug.Log("JumpHat captured!");
                break;
            default:
                Debug.LogWarning("Unknown hat: " + hatName);
                break;
        }
    }

    // --- Complete a quest ---
    public void CompleteQuest(string questGiver)
    {
        switch (questGiver)
        {
            case "Lira":
                questCompleteLira = true;
                NewHatalougeManager.Instance.quest1Found = true;   // unlock quest page 1
                NewHatalougeManager.Instance.UpdateQuestScreen();
                Debug.Log("Lira's quest completed!");
                break;

            case "Mallow":
                questCompleteMallow = true;
                NewHatalougeManager.Instance.quest2Found = true;   // unlock quest page 2
                NewHatalougeManager.Instance.UpdateQuestScreen();
                Debug.Log("Mallow's quest completed!");
                break;

            case "Tulip":
                questCompleteTulip = true;
                NewHatalougeManager.Instance.quest3Found = true;   // unlock quest page 3
                NewHatalougeManager.Instance.UpdateQuestScreen();
                Debug.Log("Tulip's quest completed!");
                break;

            default:
                Debug.LogWarning("Unknown quest giver: " + questGiver);
                break;
        }
    }

    public bool AllQuestsCompleted()
    {
        return questCompleteLira && questCompleteMallow && questCompleteTulip;
    }

    // --- Unlock a potion ---
    public void UnlockPotion(string potionName)
    {
        switch (potionName)
        {
            case "CakePotion":
                cakePotionUnlocked = true;
                Debug.Log("Cake Potion unlocked!");
                break;
            case "CottonCrazePotion":
                cottonCrazePotionUnlocked = true;
                Debug.Log("Cotton Craze Potion unlocked!");
                break;
            case "MirrorPotion":
                mirrorPotionUnlocked = true;
                Debug.Log("Mirror Potion unlocked!");
                break;
            case "StarPotion":
                starPotionUnlocked = true;
                Debug.Log("Star Potion unlocked!");
                break;
            case "HerbPotion":
                herbPotionUnlocked = true;
                Debug.Log("Herb Potion unlocked!");
                break;
            default:
                Debug.LogWarning("Unknown potion: " + potionName);
                break;
        }
    }

    // --- Check if all hats are captured ---
    public bool AllHatsCaptured()
    {
        return shyHatCaptured && fastHatCaptured && jumpHatCaptured;
    }

    // --- Check if all potions are unlocked ---
    public bool AllPotionsUnlocked()
    {
        return cakePotionUnlocked && cottonCrazePotionUnlocked &&
               mirrorPotionUnlocked && starPotionUnlocked && herbPotionUnlocked;
    }
}
