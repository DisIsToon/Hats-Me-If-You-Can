using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameDataManager2 : MonoBehaviour
{
    public static GameDataManager2 Instance;

    public GameData2 currentData;
    public int currentSlot;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // NEW GAME
    public void StartNewGame(int slot)
    {
        
        currentSlot = slot;
        currentData = new GameData2(); // default values
    }

    // LOAD GAME
    public void LoadGame(int slot)
    {
        currentSlot = slot;
        currentData = SaveSystem2.LoadGame(slot);

        if (currentData == null)
        {
            currentData = new GameData2();
        }

        StartCoroutine(DelayedLoad());
    }

    IEnumerator DelayedLoad()
    {
        yield return new WaitForSeconds(0.1f);
        ApplyLoadedData();
    }

    // SAVE GAME (ONLY IN BiomeOptimized)
    public void SaveGame()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "BiomeOptimized")
        {
            
            return;
        }

        if (currentData == null)
            currentData = new GameData2();

        // COLLECT DATA FIRST
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.SaveData(currentData);

        if (EquipSystem.Instance != null)
            EquipSystem.Instance.SaveData(currentData);

        if (GameTracker.Instance != null)
            GameTracker.Instance.SaveData(currentData);

        if (CardsController.Instance != null)
            CardsController.Instance.SaveData(currentData);

        if (WinterBarrierSystem.Instance != null)
            WinterBarrierSystem.Instance.SaveData(currentData);

        if (RealWinterBarrierSystem.Instance != null)
            RealWinterBarrierSystem.Instance.SaveData(currentData);

        if (NewHatalougeManager.Instance != null)
            NewHatalougeManager.Instance.SaveData(currentData);

        foreach (NPC npc in NPC.allNPCs)
        {
            npc.SaveData(currentData);
        }

        // THEN SAVE
        SaveSystem2.SaveGame(currentData, currentSlot);

        
    }

    public void ApplyLoadedData()
    {
        if (currentData == null)
        {
            
            return;
        }

        // 1. Inventory FIRST (base data)
        if (InventorySystem.Instance != null)
            InventorySystem.Instance.LoadData(currentData);

        // 2. Quickslots SECOND (depends on inventory sometimes)
        if (EquipSystem.Instance != null)
            EquipSystem.Instance.LoadData(currentData);

        // 3. Game progress (hats, quests, biomes)
        if (GameTracker.Instance != null)
            GameTracker.Instance.LoadData(currentData);

        // 4. Other systems
        if (CardsController.Instance != null)
            CardsController.Instance.LoadData(currentData);

        if (WinterBarrierSystem.Instance != null)
            WinterBarrierSystem.Instance.LoadData(currentData);

        if (RealWinterBarrierSystem.Instance != null)
            RealWinterBarrierSystem.Instance.LoadData(currentData);

        if (NewHatalougeManager.Instance != null)
            NewHatalougeManager.Instance.LoadData(currentData);

        foreach (NPC npc in NPC.allNPCs)
        {
            npc.LoadData(currentData);
        }

    }
}