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

    public bool puzzleComplete; // Mirror
    public bool rotatingPuzzleComplete;

    // CHARACTER DISCOVERY
    public bool headMasterFound;
    public bool clumsiFound;
    public bool ivyFound;
    public bool chaseFound;
    public bool louierFound;
    public bool liraFound;
    public bool mallowFound;
    public bool tulipFound;

    //public Vector3 playerPosition;

    public SerializableDictionary<string, bool> potionsCollected;

    public SerializableDictionary<string, NPCSaveData> npcData;

    //  MOVE HERE (CLASS LEVEL)
    public List<string> inventoryItems;
    public List<int> inventoryStacks;

    public List<string> equippedItems;
    public List<int> equippedStacks;

    [Serializable]
    public class NPCSaveData
    {
        public bool firstTimeInteraction;
        public bool questDone;
        public int activeQuestIndex;
        public bool accepted;
        public bool completed;
    }

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

        puzzleComplete = false;
        rotatingPuzzleComplete = false;

        headMasterFound = false;
        clumsiFound = false;
        ivyFound = false;
        chaseFound = false;
        louierFound = false;
        liraFound = false;
        mallowFound = false;
        tulipFound = false;

        npcData = new SerializableDictionary<string, NPCSaveData>();

    }
}

