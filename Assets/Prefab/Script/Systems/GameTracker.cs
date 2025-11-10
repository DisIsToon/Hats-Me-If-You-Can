using UnityEngine;

public class GameTracker : MonoBehaviour
{
    [Header("Player Reference")]
    public GameObject player;  // Assign your Player here

    // --- Hats ---
    public bool shyHatCaptured = false;
    public bool fastHatCaptured = false;
    public bool jumpHatCaptured = false;

    // --- Potions ---
    public bool cakePotionUnlocked = false;
    public bool cottonCrazePotionUnlocked = false;
    public bool mirrorPotionUnlocked = false;
    public bool starPotionUnlocked = false;
    public bool herbPotionUnlocked = false;

    private void Start()
    {
        if (player == null)
            Debug.LogError("Player GameObject not assigned in GameManager!");
    }

    // --- Capture a hat ---
    public void CaptureHat(string hatName)
    {
        switch (hatName)
        {
            case "ShyHat":
                shyHatCaptured = true;
                Debug.Log("ShyHat captured!");
                break;
            case "FastHat":
                fastHatCaptured = true;
                Debug.Log("FastHat captured!");
                break;
            case "JumpHat":
                jumpHatCaptured = true;
                Debug.Log("JumpHat captured!");
                break;
            default:
                Debug.LogWarning("Unknown hat: " + hatName);
                break;
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
