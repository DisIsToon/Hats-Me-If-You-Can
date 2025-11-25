using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    [Header("Quests")]
    public List<Quest> allActiveQuests;
    public List<Quest> allCompletedQuests;

    [Header("Quest Menu UI")]
    public GameObject questMenu;
    public bool isQuestMenuOpen;
    public GameObject questMenuContent;
    public GameObject activeQuestPrefab;
    public GameObject completedQuestPrefabs;

    [Header("Quest Tracker")]
    public GameObject questTrackerContent;
    public GameObject trackerRowPrefab;
    public List<Quest> allTrackedQuests;

    [Header("Special Objects / Rewards")]
    public GameObject puzzleObject;          // Appears when Lira quest is accepted
    public bool puzzleComplete = false;      // Set true when puzzle is solved
    public bool winterForestPass = false;    // Reward for completing Lira quest
    public Quest liraPuzzleQuest;            // Reference to Lira's quest

    private void Start()
    {
        // Initialize Lira's puzzle quest
        liraPuzzleQuest = new Quest();
        liraPuzzleQuest.questName = "Fix the Ancient Puzzle";
        liraPuzzleQuest.questGiver = "Lira";
        liraPuzzleQuest.questDescription = "Complete the old puzzle Lira found.";
        liraPuzzleQuest.accepted = false;
        liraPuzzleQuest.isCompleted = false;
    }

    #region Quest Menu Toggle
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isQuestMenuOpen)
            {
                questMenu.SetActive(true);
                isQuestMenuOpen = true;
            }
            else
            {
                questMenu.SetActive(false);
                isQuestMenuOpen = false;
            }
        }
    }
    #endregion

    #region Track / Untrack Quests
    public void TrackQuest(Quest quest)
    {
        if (!allTrackedQuests.Contains(quest))
        {
            allTrackedQuests.Add(quest);
            RefreshTrackerList();
        }
    }

    public void UnTrackQuest(Quest quest)
    {
        if (allTrackedQuests.Contains(quest))
        {
            allTrackedQuests.Remove(quest);
            RefreshTrackerList();
        }
    }

    public void RefreshTrackerList()
    {
        // Clear previous tracker UI
        foreach (Transform child in questTrackerContent.transform)
        {
            Destroy(child.gameObject);
        }

        // Create new tracker UI
        foreach (Quest trackedQuest in allTrackedQuests)
        {
            GameObject trackerPrefab = Instantiate(trackerRowPrefab, Vector3.zero, Quaternion.identity);
            trackerPrefab.transform.SetParent(questTrackerContent.transform, false);

            TrackerRow tRow = trackerPrefab.GetComponent<TrackerRow>();
            tRow.questName.text = trackedQuest.questName;
            tRow.description.text = trackedQuest.questDescription;

            var req1 = trackedQuest.info?.firstRequirmentItem ?? "";
            var req1Amount = trackedQuest.info?.firstRequirementAmount ?? 0;
            var req2 = trackedQuest.info?.secondRequirmentItem ?? "";
            var req2Amount = trackedQuest.info?.secondRequirementAmount ?? 0;

            if (!string.IsNullOrEmpty(req2))
            {
                tRow.requirements.text = $"{req1} " + InventorySystem.Instance.CheckItemAmount(req1) + "/" + req1Amount + "\n" +
                                         $"{req2} " + InventorySystem.Instance.CheckItemAmount(req2) + "/" + req2Amount;
            }
            else
            {
                tRow.requirements.text = $"{req1} " + InventorySystem.Instance.CheckItemAmount(req1) + "/" + req1Amount;
            }
        }
    }
    #endregion

    #region Add / Complete Quests
    public void AddActiveQuest(Quest quest)
    {
        if (!allActiveQuests.Contains(quest))
        {
            allActiveQuests.Add(quest);
            TrackQuest(quest);
            RefreshQuestList();

            // Special logic: Lira's puzzle quest activates puzzle object
            if (quest.questGiver == "Lira")
            {
                puzzleObject.SetActive(true);
            }
        }
    }

    public void MarkQuestCompleted(Quest quest)
    {
        if (allActiveQuests.Contains(quest))
            allActiveQuests.Remove(quest);

        if (!allCompletedQuests.Contains(quest))
            allCompletedQuests.Add(quest);

        UnTrackQuest(quest);
        RefreshQuestList();
    }

    public void RefreshQuestList()
    {
        // Clear previous UI
        foreach (Transform child in questMenuContent.transform)
        {
            Destroy(child.gameObject);
        }

        // Active Quests
        foreach (Quest activeQuest in allActiveQuests)
        {
            GameObject questPrefab = Instantiate(activeQuestPrefab, Vector3.zero, Quaternion.identity);
            questPrefab.transform.SetParent(questMenuContent.transform, false);

            QuestRow qRow = questPrefab.GetComponent<QuestRow>();
            qRow.thisQuest = activeQuest;
            qRow.questName.text = activeQuest.questName;
            qRow.questGiver.text = activeQuest.questGiver;
            qRow.isActive = true;
            qRow.isTracking = true;

            if (!string.IsNullOrEmpty(activeQuest.info?.rewardItem1))
            {
                qRow.firstReward.sprite = GetSpriteForItem(activeQuest.info.rewardItem1);
                qRow.firstRewardAmount.text = "";
            }
            else
            {
                qRow.firstReward.gameObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(activeQuest.info?.rewardItem2))
            {
                qRow.secondReward.sprite = GetSpriteForItem(activeQuest.info.rewardItem2);
                qRow.secondRewardAmount.text = "";
            }
            else
            {
                qRow.secondReward.gameObject.SetActive(false);
            }
        }

        // Completed Quests
        foreach (Quest completedQuest in allCompletedQuests)
        {
            GameObject questPrefab = Instantiate(completedQuestPrefabs, Vector3.zero, Quaternion.identity);
            questPrefab.transform.SetParent(questMenuContent.transform, false);

            QuestRow qRow = questPrefab.GetComponent<QuestRow>();
            qRow.questName.text = completedQuest.questName;
            qRow.questGiver.text = completedQuest.questGiver;
            qRow.isActive = false;
            qRow.isTracking = false;

            if (!string.IsNullOrEmpty(completedQuest.info?.rewardItem1))
            {
                qRow.firstReward.sprite = GetSpriteForItem(completedQuest.info.rewardItem1);
                qRow.firstRewardAmount.text = "";
            }
            else
            {
                qRow.firstReward.gameObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(completedQuest.info?.rewardItem2))
            {
                qRow.secondReward.sprite = GetSpriteForItem(completedQuest.info.rewardItem2);
                qRow.secondRewardAmount.text = "";
            }
            else
            {
                qRow.secondReward.gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region Helpers
    private Sprite GetSpriteForItem(string item)
    {
        var itemObj = Resources.Load<GameObject>(item);
        if (itemObj != null)
            return itemObj.GetComponent<Image>().sprite;
        return null;
    }
    #endregion
}
