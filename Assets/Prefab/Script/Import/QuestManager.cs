using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
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

    [Header("Quest State Bools")]
    public bool liraQuestAccepted = false;
    public bool mallowQuestAccepted = false;
    public bool tulipQuestAccepted = false;
    public bool HeadMasterQuestAccepted = false;

    [Header("Quests")]
    public List<Quest> allActiveQuests;
    public List<Quest> allCompletedQuests;

    [Header("SpawningObject")]
    public GameObject cotton;
    public GameObject cottonPrefab;
    public GameObject position;
    public GameObject lumiShroom;

    public GameObject jumpHat;
    public GameObject fastHat;

    [Header("Video UI")]
    public RawImage videoRawImage;          // assign the RawImage in inspector
    public VideoPlayer headmasterVideoPlayer; // assign VideoPlayer in inspector

    private bool hasPlayedHeadmasterVideo = false;

    [Header("Active Quest")]
    public QuestInfo activeQuest;
    public bool questAccepted;
    public bool questCompleted;

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
    public GameObject puzzleObject;
    public GameObject lostHat;
    public bool puzzleComplete = false;
    public bool winterForestPass = false;
    public bool questCompleteLira = false;
    public bool questCompleteMallow = false;
    public bool questCompleteTulip = false;
    public bool headmasterNotifShown = false;
    public Quest liraPuzzleQuest;

    private void Start()
    {
        // Initialize Lira puzzle quest
        liraPuzzleQuest = new Quest();
        liraPuzzleQuest.questName = "Fix the Ancient Puzzle";
        liraPuzzleQuest.questGiver = "Lira";
        liraPuzzleQuest.questDescription = "Complete the old puzzle Lira found.";
        liraPuzzleQuest.accepted = false;
        liraPuzzleQuest.isCompleted = false;
        lumiShroom.SetActive(false);

        // Hide the RawImage at start
        if (videoRawImage != null)
            videoRawImage.gameObject.SetActive(false);

        // Optional: subscribe to video end event
        if (headmasterVideoPlayer != null)
            headmasterVideoPlayer.loopPointReached += OnVideoFinished;
    }

    public void SetLiraPuzzleComplete()
    {
        puzzleComplete = true;
    }

    // ======================================================
    //   REQUIREMENTS
    // ======================================================

    public bool CanStartQuest(QuestInfo quest)
    {
        // --- BOOL requirement ---
        if (quest.requireBoolToStart && quest.boolRequirementName != "")
        {
            if (!CheckBoolRequirement(quest.boolRequirementName))
                return false;
        }

        // --- ITEM requirements ---
        if (!CheckItemRequirement(quest.firstRequirmentItem, quest.firstRequirementAmount))
            return false;

        if (!CheckItemRequirement(quest.secondRequirmentItem, quest.secondRequirementAmount))
            return false;

        return true;
    }

    private bool CheckItemRequirement(string item, int amount)
    {
        if (string.IsNullOrEmpty(item) || amount <= 0)
            return true;

        return InventorySystem.Instance.CheckItemAmount(item) >= amount;
    }

    private bool CheckBoolRequirement(string flagName)
    {
        GameTracker t = GameTracker.Instance;

        switch (flagName)
        {
            case "OpenedFrozenGate": return t.openedFrozenGate;
        }

        Debug.LogWarning("Bool requirement flag not found: " + flagName);
        return false;
    }

    // ======================================================
    //   START QUEST
    // ======================================================

    public void AcceptQuest(QuestInfo quest)
    {
        activeQuest = quest;
        questAccepted = true;
        questCompleted = false;

        RefreshTrackerList();
    }


    // ======================================================
    //   COMPLETE QUEST + REWARDS
    // ======================================================

    public void CompleteQuest()
    {
        if (activeQuest == null)
            return;

        // Remove items required (optional)
        if (activeQuest.firstRequirementAmount > 0)
            InventorySystem.Instance.RemoveItem(activeQuest.firstRequirmentItem, activeQuest.firstRequirementAmount);

        if (activeQuest.secondRequirementAmount > 0)
            InventorySystem.Instance.RemoveItem(activeQuest.secondRequirmentItem, activeQuest.secondRequirementAmount);
 

        // Coin reward
        if (activeQuest.coinReward > 0)
            Debug.Log("Give coin reward here");
        // Add your currency system if you have one.


        // Item rewards
        if (!string.IsNullOrEmpty(activeQuest.rewardItem1))
            InventorySystem.Instance.AddToInventory(activeQuest.rewardItem1);

        if (!string.IsNullOrEmpty(activeQuest.rewardItem2))
            InventorySystem.Instance.AddToInventory(activeQuest.rewardItem2);


        // Bool reward
        if (activeQuest.giveBoolReward && activeQuest.boolRewardName != "")
            ApplyBoolReward(activeQuest.boolRewardName);


        questCompleted = true;

        RefreshTrackerList();
    }

    private void ApplyBoolReward(string flagName)
    {
        GameTracker t = GameTracker.Instance;

        switch (flagName)
        {
            case "OpenedFrozenGate": t.openedFrozenGate = true; break;

            default:
                Debug.LogWarning("Bool reward flag not found: " + flagName);
                break;
        }
    }

    #region Quest Menu Toggle
    public void Update()
    {
        /*
        // Play HeadMaster video if accepted and not yet played
        if (HeadMasterQuestAccepted && !hasPlayedHeadmasterVideo)
        {
            //PlayHeadMasterVideoAfterDialog();
        }
        */
    }

    public void PlayHeadMasterVideoAfterDialog()
    {
        if (HeadMasterQuestAccepted && !hasPlayedHeadmasterVideo)
        {
            if (headmasterVideoPlayer != null && videoRawImage != null)
            {
                // Stop all game sounds
                if (SoundManager.Instance != null)
                {
                    foreach (AudioSource sfx in SoundManager.Instance.allSFX)
                        sfx.Pause();

                    foreach (AudioSource bgm in SoundManager.Instance.allBGMs)
                        bgm.Pause();
                }

                videoRawImage.gameObject.SetActive(true);
                headmasterVideoPlayer.gameObject.SetActive(true);
                headmasterVideoPlayer.Play();
                hasPlayedHeadmasterVideo = true;

                // Subscribe safely
                headmasterVideoPlayer.loopPointReached -= OnVideoFinished; // remove any previous
                headmasterVideoPlayer.loopPointReached += OnVideoFinished;
            }
            else
            {
                Debug.LogWarning("HeadMaster VideoPlayer or RawImage not assigned!");
            }
        }
    }

    public void OnVideoFinished(VideoPlayer vp)
    {
        // Unsubscribe immediately to avoid multiple calls
        vp.loopPointReached -= OnVideoFinished;

        if (videoRawImage != null)
            videoRawImage.gameObject.SetActive(false);

        if (vp != null)
            vp.gameObject.SetActive(false);

        // Resume all sounds
        if (SoundManager.Instance != null)
        {
            foreach (AudioSource sfx in SoundManager.Instance.allSFX)
                sfx.UnPause();

            foreach (AudioSource bgm in SoundManager.Instance.allBGMs)
                bgm.UnPause();
        }
    }




    public void AcceptHeadMasterQuest()
    {
        HeadMasterQuestAccepted = true;
        // Any additional logic for HeadMaster quest
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
        foreach (Transform child in questTrackerContent.transform)
            Destroy(child.gameObject);

        foreach (Quest trackedQuest in allTrackedQuests)
        {
            GameObject trackerPrefab = Instantiate(trackerRowPrefab);
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
                tRow.requirements.text =
                    $"{req1} {InventorySystem.Instance.CheckItemAmount(req1)}/{req1Amount}\n" +
                    $"{req2} {InventorySystem.Instance.CheckItemAmount(req2)}/{req2Amount}";
            }
            else
            {
                tRow.requirements.text =
                    $"{req1} {InventorySystem.Instance.CheckItemAmount(req1)}/{req1Amount}";
            }
        }
    }
    #endregion

    #region Add / Complete Quests
    public void AddActiveQuest(Quest quest)
    {
        if (allActiveQuests.Contains(quest))
            return;

        allActiveQuests.Add(quest);
        TrackQuest(quest);
        RefreshQuestList();

        string npcName = DialogSystem.Instance.GetSpeakerName();

        // --------------------
        // Assign correct quest bool
        // --------------------
        if (npcName == "Lira")
        {
            liraQuestAccepted = true;
            puzzleObject.SetActive(true);     // activate puzzle
            NotifUIManager.Instance.NotifyQuestAccepted("Forest Sprite");
            NewHatalougeManager.Instance.quest1Found = true;   // unlock quest page 1
            NewHatalougeManager.Instance.DiscoverCharLira();
            NewHatalougeManager.Instance.UpdateQuestScreen();
            NewHatalougeManager.Instance.UpdateCharacterScreen();
        }
        else if (npcName == "Mallow")
        {
            mallowQuestAccepted = true;
            NotifUIManager.Instance.NotifyQuestAccepted("Sickly Stranger");
            NewHatalougeManager.Instance.quest1Found = true;   // unlock quest page 1
            NewHatalougeManager.Instance.DiscoverCharMallow();
            NewHatalougeManager.Instance.UpdateQuestScreen();
            NewHatalougeManager.Instance.UpdateCharacterScreen();
            lumiShroom.SetActive(true);
        }
        else if (npcName == "Tulip")
        {
            tulipQuestAccepted = true;
            lostHat.SetActive(true);    
            NotifUIManager.Instance.NotifyQuestAccepted("Lost Hat");
            NewHatalougeManager.Instance.quest1Found = true;   // unlock quest page 1
            NewHatalougeManager.Instance.DiscoverCharTulip();
            NewHatalougeManager.Instance.UpdateQuestScreen();
            NewHatalougeManager.Instance.UpdateCharacterScreen();
        }
        else if (npcName == "Headmaster")
        {
            HeadMasterQuestAccepted = true;
        }
    }

    public IEnumerator MarkQuestCompleted(Quest quest)
    {
        if (allActiveQuests.Contains(quest))
            allActiveQuests.Remove(quest);

        if (!allCompletedQuests.Contains(quest))
            allCompletedQuests.Add(quest);

        UnTrackQuest(quest);
        RefreshQuestList(); 

        string npcName = DialogSystem.Instance.GetSpeakerName();

        // Reward only for Lira
        if (npcName == "Lira" && puzzleComplete)
        {
            winterForestPass = true;
            questCompleteLira = true;
            GameTracker.Instance.CompleteQuest("Lira");
            QuestCompleteNotif.Instance.ShowLiraComplete();

            yield return new WaitForSeconds(1f);

            // Spawn cotton reward
            if (cottonPrefab != null && cotton != null)
            {
                Instantiate(cottonPrefab, position.transform.position, position.transform.rotation);
            }
        }

        // Reward only for Mallow
        if (npcName == "Mallow")
        {
            questCompleteMallow = true;
            GameTracker.Instance.CompleteQuest("Mallow");
            QuestCompleteNotif.Instance.ShowMallowComplete();

            yield return new WaitForSeconds(2.5f);

            if (jumpHat != null)
            {
                jumpHat.gameObject.SetActive(true);
            }
        }

        // Reward only for Tulip
        if (npcName == "Tulip")
        {
            questCompleteTulip = true;
            GameTracker.Instance.CompleteQuest("Tulip");
            QuestCompleteNotif.Instance.ShowTulipComplete();

            yield return new WaitForSeconds(2.5f);

            if (fastHat != null)
            {
                fastHat.gameObject.SetActive(true);
            }
        }

        /*
        if (!headmasterNotifShown &&
        (questCompleteLira && questCompleteMallow && questCompleteTulip))
        {
            headmasterNotifShown = true;
            NotifUIManager.Instance.NotifyMeetHeadMaster();
        }
        */
    }

    public void RefreshQuestList()
    {
        foreach (Transform child in questMenuContent.transform)
            Destroy(child.gameObject);

        foreach (Quest activeQuest in allActiveQuests)
        {
            GameObject questPrefab = Instantiate(activeQuestPrefab);
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
            else qRow.firstReward.gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(activeQuest.info?.rewardItem2))
            {
                qRow.secondReward.sprite = GetSpriteForItem(activeQuest.info.rewardItem2);
                qRow.secondRewardAmount.text = "";
            }
            else qRow.secondReward.gameObject.SetActive(false);
        }

        foreach (Quest completedQuest in allCompletedQuests)
        {
            GameObject questPrefab = Instantiate(completedQuestPrefabs);
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
            else qRow.firstReward.gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(completedQuest.info?.rewardItem2))
            {
                qRow.secondReward.sprite = GetSpriteForItem(completedQuest.info.rewardItem2);
                qRow.secondRewardAmount.text = "";
            }
            else qRow.secondReward.gameObject.SetActive(false);
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
