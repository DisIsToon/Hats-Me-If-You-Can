using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private string profileId = "";

    [Header("Content")]
    [SerializeField] private GameObject noDataContent;
    [SerializeField] private GameObject hasDataContent;
    [SerializeField] private TextMeshProUGUI gamePlayTimeText;

    [Header("Visual")]
    [SerializeField] private Image slotImage;

    public bool hasData { get; private set; } = false;
    private Button saveSlotButton;

    private void Awake()
    {
        saveSlotButton = this.GetComponent<Button>();
    }

    public void SetData(GameData data)
    {
        if (data == null)
        {
            hasData = false;
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
            Debug.Log("SetData and data == null");
        }
        else
        {
            Debug.Log("SetData and data is not null");
            hasData = true;
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);

            // Call the function from GameData
            gamePlayTimeText.text = data.GetFinalGamePlayTime().ToString("F2");
            // "F2" formats to 2 decimal places (optional)
            SetOpacity(255);
        }
    }

    private void SetOpacity(int alpha) {
        Color c = slotImage.color;
        c.a = alpha / 255f;
        slotImage.color = c;
    }

    public string GetProfileId()
    {
        return this.profileId;
    }

    public void SetInteractable(bool interactable)
    {
        saveSlotButton.interactable = interactable;
    }
}
