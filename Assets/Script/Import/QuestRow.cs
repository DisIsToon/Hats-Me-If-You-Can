using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestRow : MonoBehaviour
{
    public TextMeshProUGUI questName;
    public TextMeshProUGUI questGiver;

    public Button trackingButton;

    public bool isActive;
    public bool isTracking;

    public TextMeshProUGUI coinAmount;
    public TextMeshProUGUI humanReputation;
    public TextMeshProUGUI goblinReputation;

    public Image firstReward;
    public TextMeshProUGUI firstRewardAmount;

    public Image secondReward;
    public TextMeshProUGUI secondRewardAmount;

    public Quest thisQuest;

    private void Start()
    {
        trackingButton.onClick.AddListener(() =>
        {
            if (isActive)
            {
                if (isTracking)
                {
                    isTracking = false;
                    trackingButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Not Tracking";
                    QuestManager.Instance.UnTrackQuest(thisQuest);
                }
                else
                {
                    isTracking = true;
                    trackingButton.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Tracking";
                    QuestManager.Instance.TrackQuest(thisQuest);
                }
            }
        }); 
    }
}
