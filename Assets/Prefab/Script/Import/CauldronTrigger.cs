using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CauldronTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (CraftingSystem.Instance != null)
                CraftingSystem.Instance.SetCanCraft(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (CraftingSystem.Instance != null)
                CraftingSystem.Instance.CraftingScreenOff();
                CraftingSystem.Instance.SetCanCraft(false);
            
        }
    }
}
