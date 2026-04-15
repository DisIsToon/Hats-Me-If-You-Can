using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[Serializable]
public class GameData2
{
    public bool hasStarted;
    public bool tutorialCompleted;
    public int playerLevel;

    public bool shyHatAlreadyCaptured;
    public bool fastHatAlreadyCaptured;
    public bool jumpHatAlreadyCaptured;

    public bool CRBarrierAlreadyOpened;
    public bool WBBarrierAlreadyOpened;
    public bool mirrorClaimed;

    public bool questAlreadyCompleteLira;
    public bool questAlreadyCompleteMallow;
    public bool questAlreadyCompleteTulip;

    public bool castleRuinAlreadyDiscovered;
    public bool winterBiomeAlreadyDiscovered;
    public bool springGardenAlreadyDiscovered;

    public long lastUpdated;
    public float playTime;

    //public Vector3 playerPosition;

    public SerializableDictionary<string, bool> potionsCollected;

    //  MOVE HERE (CLASS LEVEL)
    public List<string> inventoryItems;
    public List<int> inventoryStacks;

    public List<string> equippedItems;
    public List<int> equippedStacks;

    public GameData2()
    {
        // DEFAULT VALUES (New Game)
        hasStarted = false;
        tutorialCompleted = false;
        playerLevel = 1;

        //this.playTime = 0.0f;
        //playerPosition = new Vector3(16f, -0.8f, -2f);
        potionsCollected = new SerializableDictionary<string, bool>();

        // Initialize lists HERE (no "public")
        inventoryItems = new List<string>();
        inventoryStacks = new List<int>();

        equippedItems = new List<string>();
        equippedStacks = new List<int>();

        shyHatAlreadyCaptured = false;
        fastHatAlreadyCaptured = false;
        jumpHatAlreadyCaptured = false;

        CRBarrierAlreadyOpened = false;
        WBBarrierAlreadyOpened = false;
        mirrorClaimed = false;

        questAlreadyCompleteLira = false;
        questAlreadyCompleteMallow = false;
        questAlreadyCompleteTulip = false;

        castleRuinAlreadyDiscovered = false;
        winterBiomeAlreadyDiscovered = false;
        springGardenAlreadyDiscovered = false;
    }
}