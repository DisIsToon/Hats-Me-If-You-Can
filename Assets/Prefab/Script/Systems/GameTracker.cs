using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameTracker : MonoBehaviour
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

    [Header("Area Triggers")]
    public GameObject triggerForest;
    public GameObject triggerCastle;
    public GameObject triggerWinter;
    private bool springGardenNotified = false;
    private bool castleRuinNotified = false;
    private bool winterBiomeNotified = false;

    [Header("Potions")]
    public bool cakePotionUnlocked = false;
    public bool cottonCrazePotionUnlocked = false;
    public bool mirrorPotionUnlocked = false;
    public bool starPotionUnlocked = false;
    public bool herbPotionUnlocked = false;

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
        if (player == null)
            Debug.LogError("Player GameObject not assigned in GameManager!");
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

    public void CheckPlayerInTrigger(GameObject trigger)
    {
        if (trigger == triggerForest)
        {
            Debug.Log("Player entered Forest!");
            NewHatalougeManager.Instance.ReachForest();

            if (!springGardenNotified)   // <-- only once
            {
                springGardenNotified = true;
                NotifUIManager.Instance.NotifyBiomeDiscovered("Spring Garden");
            }
        }
        else if (trigger == triggerCastle)
        {
            Debug.Log("Player entered Castle!");
            NewHatalougeManager.Instance.ReachCastle();

            if (!castleRuinNotified)     // <-- only once
            {
                castleRuinNotified = true;
                NotifUIManager.Instance.NotifyBiomeDiscovered("Castle Ruin");
            }
        }
        else if (trigger == triggerWinter)
        {
            Debug.Log("Player entered Winter!");
            NewHatalougeManager.Instance.ReachWinter();

            if (!winterBiomeNotified)    // <-- only once
            {
                winterBiomeNotified = true;
                NotifUIManager.Instance.NotifyBiomeDiscovered("Winter Biome");
            }
        }
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
