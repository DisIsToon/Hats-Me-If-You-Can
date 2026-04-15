using UnityEngine;

public class MainMenuUI2 : MonoBehaviour
{
    public GameObject slotPanel;

    public void OnClickPlay()
    {
        Debug.LogError("MainMenuUI2 Play CLicked");
        slotPanel.SetActive(true);
    }
}