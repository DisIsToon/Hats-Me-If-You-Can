using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PotionInteract : MonoBehaviour, IDataPersistence
{
    [SerializeField] private string id;
    [ContextMenu("Generate guid for id")]

    private void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

    public string potionName;        // e.g. "Red Potion"
    public Text potionText;          // UI element assigned in Inspector
    private bool collected = false;  // tracks if this potion is collected
    public int potionsCollected = 0;

    private void Start()
    {
        // Initialize UI to "No" at start
        if (potionText != null)
        {
            potionText.text = "No";
        }
    }

    public void LoadData(GameData data)
    {
        
        data.potionsCollected.TryGetValue(id, out collected);
        if (collected)
        {
            gameObject.SetActive(false);
        }
        foreach(KeyValuePair<string, bool> pair in data.potionsCollected)
        {
            if(pair.Value)
            {
                potionsCollected++;
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if(data.potionsCollected.ContainsKey(id))
        {
            data.potionsCollected.Remove(id);
        }
        data.potionsCollected.Add(id, collected);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            collected = true;

            // Update UI
            if (potionText != null)
            {
                potionText.text = "Yes";
            }

            // Hide or destroy potion object
            Destroy(gameObject);
        }
    }

    public bool IsCollected()
    {
        return collected;
    }
}
