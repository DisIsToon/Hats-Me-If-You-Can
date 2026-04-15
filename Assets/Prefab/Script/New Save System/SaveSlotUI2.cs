using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

public class SaveSlotUI2 : MonoBehaviour
{
    public int slotIndex;

    public GameObject newGameUI;
    public GameObject loadGameUI;

    public GameObject deleteConfirmUI;

    public static SaveSlotUI2 selectedSlot;

    public GameObject resetButton;

    void Start()
    {
        
        bool hasSave = SaveSystem2.SlotHasSave(slotIndex);

        newGameUI.SetActive(!hasSave);
        loadGameUI.SetActive(hasSave);

        RefreshUI();
        deleteConfirmUI.SetActive(false);
    }

    public void OnClickSlot()
    {
        bool hasSave = SaveSystem2.SlotHasSave(slotIndex);
        

        if (hasSave)
        {
            GameDataManager2.Instance.LoadGame(slotIndex);

            
            GameDataManager2.Instance.LoadGame(slotIndex);
            SceneManager.LoadScene("BiomeOptimized");
        }
        else
        {
            
            GameDataManager2.Instance.StartNewGame(slotIndex);
            SceneManager.LoadScene("IntroCutsceneScene");
        }
    }

    public void OnClickDelete()
    {
        selectedSlot = this; // THIS SLOT is being deleted

        

        // Show confirmation popup
        if (deleteConfirmUI != null)
        {
            deleteConfirmUI.SetActive(true);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BiomeOptimized")
        {
            GameDataManager2.Instance.ApplyLoadedData();
            SceneManager.sceneLoaded -= OnSceneLoaded; // IMPORTANT
        }
    }

    public void ConfirmDelete()
    {
        if (selectedSlot == null) return;

        SaveSystem2.DeleteSave(selectedSlot.slotIndex);

        selectedSlot.RefreshUI();

        if (GameDataManager2.Instance != null &&
            GameDataManager2.Instance.currentSlot == selectedSlot.slotIndex)
        {
            GameDataManager2.Instance.currentData = null;
        }

        selectedSlot.CloseDeleteUI();

        selectedSlot = null;
    }

    public void CancelDelete()
    {
        if (selectedSlot == null) return;

        selectedSlot.CloseDeleteUI();
        selectedSlot = null;
    }

    private void CloseDeleteUI()
    {
        if (deleteConfirmUI != null)
            deleteConfirmUI.SetActive(false);
    }

    public void RefreshUI()
    {
        bool hasSave = SaveSystem2.SlotHasSave(slotIndex);

        newGameUI.SetActive(!hasSave);
        loadGameUI.SetActive(hasSave);

        //BUTTON LOGIC
        bool shouldShowButton = newGameUI.activeSelf || loadGameUI.activeSelf;

        if (resetButton != null)
        {
            resetButton.SetActive(shouldShowButton);
        }
    }
}