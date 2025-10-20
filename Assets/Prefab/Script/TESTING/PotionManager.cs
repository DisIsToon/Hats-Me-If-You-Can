using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum PotionType { Red, Blue, Green }

public class PotionManager : MonoBehaviour
{
    public static PotionManager Instance;

    // Track potion possession
    private Dictionary<PotionType, bool> potions = new Dictionary<PotionType, bool>();

    [Header("UI References")]
    public Text redPotionText;
    public Text bluePotionText;
    public Text greenPotionText;

    private void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initialize all potions as false
        potions[PotionType.Red] = false;
        potions[PotionType.Blue] = false;
        potions[PotionType.Green] = false;
    }

    private void Update()
    {
        if (redPotionText != null) redPotionText.text = potions[PotionType.Red] ? "Yes" : "No";
        if (bluePotionText != null) bluePotionText.text = potions[PotionType.Blue] ? "Yes" : "No";
        if (greenPotionText != null) greenPotionText.text = potions[PotionType.Green] ? "Yes" : "No";
    }

    public void PickUpPotion(PotionType type)
    {
        potions[type] = true;
    }

    public bool HasPotion(PotionType type)
    {
        return potions[type];
    }
}
