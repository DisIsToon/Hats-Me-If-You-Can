using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class GameTracker : MonoBehaviour
{
    public static GameTracker Instance { get; set; }

    [Header("Player Reference")]
    public GameObject player;  // Assign your Player here

    [Header("Hats/Bools")]
    public bool shyHatCaptured = false;
    public bool fastHatCaptured = false;
    public bool jumpHatCaptured = false;
    public bool openedFrozenGate = false;
    public bool puzzleComplete = false;

    [Header("Hat GameObjects in Scene")]
    public GameObject shyHatObject;
    public GameObject fastHatObject;
    public GameObject jumpHatObject;

    [Header("Quest Completion Bools")]
    public bool questCompleteLira = false;
    public bool questCompleteMallow = false;
    public bool questCompleteTulip = false;

    [Header("Video UI")]
    public RawImage videoRawImage;          // assign the RawImage in inspector
    public VideoPlayer FastHatVideoPlayer; // assign VideoPlayer in inspector
    public VideoPlayer JumpHatVideoPlayer; // assign VideoPlayer in inspector
    public VideoPlayer ShyHatVideoPlayer; // assign VideoPlayer in inspector

    private bool hasPlayedShyHatVideo = false;
    private bool hasPlayedFastHatVideo = false;
    private bool hasPlayedJumpHatVideo = false;

    [Header("Current Biome")]
    public string currentBiome = "None";

    [Header("Area Triggers")]
    public GameObject triggerForest1;
    public GameObject triggerForest2;
    public GameObject triggerForest3;
    public GameObject triggerCastle;
    public GameObject triggerWinter;
    // Notifications (only once)
    private bool castleRuinNotified = false;
    private bool winterBiomeNotified = false;
    private bool springGardenNotified = false;
    public bool visitedWinterForest = false;
    public bool visitedCastleRuin = false;

    [Header("Potions")]
    public bool cakePotionUnlocked = false;
    public bool cottonCrazePotionUnlocked = false;
    public bool mirrorPotionUnlocked = false;
    public bool starPotionUnlocked = false;
    public bool herbPotionUnlocked = false;

    [Header("FROM SAVE SYSTEM")]
    public bool shyHatAlreadyCaptured;
    public bool fastHatAlreadyCaptured;
    public bool jumpHatAlreadyCaptured;

    public bool questAlreadyCompleteLira;
    public bool questAlreadyCompleteMallow;
    public bool questAlreadyCompleteTulip;

    public bool castleRuinAlreadyDiscovered;
    public bool winterBiomeAlreadyDiscovered;
    public bool springGardenAlreadyDiscovered;

    public bool castleRuinDiscovered;
    public bool winterBiomeDiscovered;
    public bool springGardenDiscovered;

    public GameObject allHatsCapturedObject; // assign in inspector
    public GameObject endingHintScreen;

    [Header("All Hats Captured UI")]
    public float hatsPopupDuration = 3f;
    public float fadeOutDuration = 1f;

    private bool allHatsPopupShown = false;

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

    public bool IsTutorial()
    {
        return NewHatalougeManager.Instance != null &&
               NewHatalougeManager.Instance.notTutorial == false;
    }

    public void LoadData(GameData2 data)
    {
        if (IsTutorial()) return;

        Debug.Log("GameTracker: Loading Data...");

        // LOAD FROM SAVE - INTO GAME
        shyHatCaptured = data.shyHatAlreadyCaptured;
        fastHatCaptured = data.fastHatAlreadyCaptured;
        jumpHatCaptured = data.jumpHatAlreadyCaptured;

        questCompleteLira = data.questAlreadyCompleteLira;
        questCompleteMallow = data.questAlreadyCompleteMallow;
        questCompleteTulip = data.questAlreadyCompleteTulip;

        castleRuinDiscovered = data.castleRuinAlreadyDiscovered;
        winterBiomeDiscovered = data.winterBiomeAlreadyDiscovered;
        springGardenDiscovered = data.springGardenAlreadyDiscovered;

        ApplyLoadedData();
    }

    private void ApplyLoadedData()
    {
        // HATS
        if (shyHatCaptured)
        {
            NewHatalougeManager.Instance.DiscoverShyHat();

            if (shyHatObject != null)
                shyHatObject.SetActive(false); // 🔥 hide in scene
        }

        if (fastHatCaptured)
        {
            NewHatalougeManager.Instance.DiscoverFastHat();

            if (fastHatObject != null)
                fastHatObject.SetActive(false);
        }

        if (jumpHatCaptured)
        {
            NewHatalougeManager.Instance.DiscoverLazyHat();

            if (jumpHatObject != null)
                jumpHatObject.SetActive(false);
        }

        // QUESTS
        if (questCompleteLira)
            NewHatalougeManager.Instance.quest1Found = true;

        if (questCompleteMallow)
            NewHatalougeManager.Instance.quest2Found = true;

        if (questCompleteTulip)
            NewHatalougeManager.Instance.quest3Found = true;

        NewHatalougeManager.Instance.UpdateQuestScreen();

        // BIOMES
        if (springGardenDiscovered)
            NewHatalougeManager.Instance.ReachForest();

        if (castleRuinDiscovered)
            NewHatalougeManager.Instance.ReachCastle();

        if (winterBiomeDiscovered)
            NewHatalougeManager.Instance.ReachWinter();
    }


    public void SaveData(GameData2 data)
    {
        if (IsTutorial()) return;

        Debug.Log("GameTracker: Saving Data...");

        // SAVE FROM GAME → INTO DATA
        data.shyHatAlreadyCaptured = shyHatCaptured;
        data.fastHatAlreadyCaptured = fastHatCaptured;
        data.jumpHatAlreadyCaptured = jumpHatCaptured;

        data.questAlreadyCompleteLira = questCompleteLira;
        data.questAlreadyCompleteMallow = questCompleteMallow;
        data.questAlreadyCompleteTulip = questCompleteTulip;

        data.castleRuinAlreadyDiscovered = castleRuinDiscovered;
        data.winterBiomeAlreadyDiscovered = winterBiomeDiscovered;
        data.springGardenAlreadyDiscovered = springGardenDiscovered;
    }

    private IEnumerator Start()
    {
        // Hide the RawImage at start
        if (videoRawImage != null)
            videoRawImage.gameObject.SetActive(false);

        // wait 1 frame to ensure GameDataManager2 is ready
        yield return null;

        if (GameDataManager2.Instance != null &&
            GameDataManager2.Instance.currentData != null)
        {
            LoadData(GameDataManager2.Instance.currentData);
        }
        else
        {
            Debug.LogWarning("GameTracker: No data found on start");
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            //PlayShyHatVideoAfterCapture();
        }
    }
    public void PlayShyHatVideoAfterCapture()
    {
        if (IsTutorial()) return; // no video in tutorial

        if (hasPlayedShyHatVideo) return;

        if (ShyHatVideoPlayer == null || videoRawImage == null)
        {
            Debug.LogWarning("Hat VideoPlayer or RawImage not assigned!");
            return;
        }

        // Pause game time
        Time.timeScale = 0f;

        // Ensure video ignores timescale
        ShyHatVideoPlayer.playbackSpeed = 1f;
        ShyHatVideoPlayer.timeReference = VideoTimeReference.InternalTime;

        // Pause all audio
        if (SoundManager.Instance != null)
        {
            foreach (AudioSource sfx in SoundManager.Instance.allSFX)
                sfx.Pause();

            foreach (AudioSource bgm in SoundManager.Instance.allBGMs)
                bgm.Pause();
        }

        // Show video UI
        videoRawImage.gameObject.SetActive(true);
        ShyHatVideoPlayer.gameObject.SetActive(true);

        // Subscribe safely
        ShyHatVideoPlayer.loopPointReached -= OnVideoFinished;
        ShyHatVideoPlayer.loopPointReached += OnVideoFinished;

        ShyHatVideoPlayer.Play();
        hasPlayedShyHatVideo = true;
    }

    public void PlayFastHatVideoAfterCapture()
    {
        if (IsTutorial()) return; // no video in tutorial

        if (hasPlayedFastHatVideo) return;

        if (FastHatVideoPlayer == null || videoRawImage == null)
        {
            Debug.LogWarning("Hat VideoPlayer or RawImage not assigned!");
            return;
        }

        // Pause game time
        Time.timeScale = 0f;

        // Ensure video ignores timescale
        FastHatVideoPlayer.playbackSpeed = 1f;
        FastHatVideoPlayer.timeReference = VideoTimeReference.InternalTime;

        // Pause all audio
        if (SoundManager.Instance != null)
        {
            foreach (AudioSource sfx in SoundManager.Instance.allSFX)
                sfx.Pause();

            foreach (AudioSource bgm in SoundManager.Instance.allBGMs)
                bgm.Pause();
        }

        // Show video UI
        videoRawImage.gameObject.SetActive(true);
        FastHatVideoPlayer.gameObject.SetActive(true);

        // Subscribe safely
        FastHatVideoPlayer.loopPointReached -= OnVideoFinished;
        FastHatVideoPlayer.loopPointReached += OnVideoFinished;

        FastHatVideoPlayer.Play();
        hasPlayedFastHatVideo = true;
    }

    public void PlayJumpHatVideoAfterCapture()
    {
        if (IsTutorial()) return; // no video in tutorial

        if (hasPlayedJumpHatVideo) return;

        if (JumpHatVideoPlayer == null || videoRawImage == null)
        {
            Debug.LogWarning("Hat VideoPlayer or RawImage not assigned!");
            return;
        }

        // Pause game time
        Time.timeScale = 0f;

        // Ensure video ignores timescale
        JumpHatVideoPlayer.playbackSpeed = 1f;
        JumpHatVideoPlayer.timeReference = VideoTimeReference.InternalTime;

        // Pause all audio
        if (SoundManager.Instance != null)
        {
            foreach (AudioSource sfx in SoundManager.Instance.allSFX)
                sfx.Pause();

            foreach (AudioSource bgm in SoundManager.Instance.allBGMs)
                bgm.Pause();
        }

        // Show video UI
        videoRawImage.gameObject.SetActive(true);
        JumpHatVideoPlayer.gameObject.SetActive(true);

        // Subscribe safely
        JumpHatVideoPlayer.loopPointReached -= OnVideoFinished;
        JumpHatVideoPlayer.loopPointReached += OnVideoFinished;

        JumpHatVideoPlayer.Play();
        hasPlayedJumpHatVideo = true;
    }

    public void OnVideoFinished(VideoPlayer vp)
    {
        // Prevent duplicate calls
        vp.loopPointReached -= OnVideoFinished;

        // Hide video
        if (videoRawImage != null)
            videoRawImage.gameObject.SetActive(false);

        if (vp != null)
            vp.gameObject.SetActive(false);

        // Resume game time
        Time.timeScale = 1f;

        // Resume audio
        if (SoundManager.Instance != null)
        {
            foreach (AudioSource sfx in SoundManager.Instance.allSFX)
                sfx.UnPause();

            foreach (AudioSource bgm in SoundManager.Instance.allBGMs)
                bgm.UnPause();
        }
    }

    public void SetCurrentBiome(string biomeName)
    {
        currentBiome = biomeName;
        Debug.Log("Current biome set to: " + biomeName);
    }

    public void CheckPlayerInTrigger(GameObject trigger)
    {
        // ---------------- FOREST ----------------
        if (trigger == triggerForest1 || trigger == triggerForest2 || trigger == triggerForest3)
        {
            Debug.Log("Player entered Forest!");
            SetCurrentBiome("Forest");

            SoundManager.Instance.SwitchBiomeMusic("Forest");


            // forest can notify multiple times — but only first time plays discovery
            if (!springGardenNotified)
            {
                springGardenNotified = true;

                // ✅ ADD THIS
                springGardenDiscovered = true;

                NotifUIManager.Instance.NotifyBiomeDiscovered("Spring Garden");
                NewHatalougeManager.Instance.ReachForest();

                GameDataManager2.Instance.SaveGame();
            }
            return;
        }

        // ---------------- CASTLE ----------------
        if (trigger == triggerCastle)
        {
            Debug.Log("Player entered Castle Ruin!");
            SetCurrentBiome("CastleRuin");

            SoundManager.Instance.SwitchBiomeMusic("CastleRuin");

            if (!castleRuinNotified)
            {
                castleRuinNotified = true;

                // ✅ ADD THIS
                castleRuinDiscovered = true;

                NotifUIManager.Instance.NotifyBiomeDiscovered("Castle Ruins");
                visitedCastleRuin = true;
                NewHatalougeManager.Instance.ReachCastle();

                GameDataManager2.Instance.SaveGame();
            }
            return;
        }

        // ---------------- WINTER ----------------
        if (trigger == triggerWinter)
        {
            Debug.Log("Player entered Winter Biome!");
            SetCurrentBiome("Winter");

            SoundManager.Instance.SwitchBiomeMusic("Winter");

            if (!winterBiomeNotified)
            {
                winterBiomeNotified = true;

                // ✅ ADD THIS
                winterBiomeDiscovered = true;

                NotifUIManager.Instance.NotifyBiomeDiscovered("Winter Forest");
                visitedWinterForest = true;
                NewHatalougeManager.Instance.ReachWinter();

                GameDataManager2.Instance.SaveGame();
            }
            return;
        }

        // AUTO SAVE
        GameDataManager2.Instance.SaveGame();
    }

    public void SetPuzzleComplete(bool value)
    {
        puzzleComplete = value;
        Debug.Log("Puzzle Complete Status Updated: " + value);
        // AUTO SAVE
        GameDataManager2.Instance.SaveGame();
    }

    public bool IsPuzzleComplete()
    {
        return puzzleComplete;
    }

    // --- Capture a hat ---
    public void CaptureHat(string hatName)
    {
        bool isTutorial = IsTutorial();

        switch (hatName)
        {
            case "ShyHat":
                if (!isTutorial) //  don't record in tutorial
                {
                    shyHatCaptured = true;
                    NewHatalougeManager.Instance.DiscoverShyHat();
                }

                NotifUIManager.Instance.NotifyHatCaptured("ShyHat");

                if (!isTutorial)
                    PlayShyHatVideoAfterCapture();

                Debug.Log("ShyHat captured!");
                break;

            case "FastHat":
                if (!isTutorial)
                {
                    fastHatCaptured = true;
                    NewHatalougeManager.Instance.DiscoverFastHat();
                }

                NotifUIManager.Instance.NotifyHatCaptured("FastHat");

                if (!isTutorial)
                    PlayFastHatVideoAfterCapture();

                Debug.Log("FastHat captured!");
                break;

            case "JumpHat":
                if (!isTutorial)
                {
                    jumpHatCaptured = true;
                    NewHatalougeManager.Instance.DiscoverLazyHat();
                }

                NotifUIManager.Instance.NotifyHatCaptured("JumpHat");

                if (!isTutorial)
                    PlayJumpHatVideoAfterCapture();

                Debug.Log("JumpHat captured!");
                break;
        }

        //  also prevent "all hats" popup during tutorial
        if (!isTutorial && AllHatsCaptured() && allHatsCapturedObject != null && !allHatsPopupShown)
        {
            allHatsCapturedObject.SetActive(true);
            StartCoroutine(ShowAllHatsCapturedPopup());
        }

        // AUTO SAVE
        GameDataManager2.Instance.SaveGame();
    }

    // --- Complete a quest ---
    public void CompleteQuest(string questGiver)
    {
        switch (questGiver)
        {
            case "Lira":
                questCompleteLira = true;
                NewHatalougeManager.Instance.quest1Found = true;   // unlock quest page 1
                NewHatalougeManager.Instance.UpdateQuestScreen();
                GameDataManager2.Instance.SaveGame();
                Debug.Log("Lira's quest completed!");
                break;

            case "Mallow":
                questCompleteMallow = true;
                NewHatalougeManager.Instance.quest2Found = true;   // unlock quest page 2
                NewHatalougeManager.Instance.UpdateQuestScreen();
                GameDataManager2.Instance.SaveGame();
                Debug.Log("Mallow's quest completed!");
                break;

            case "Tulip":
                questCompleteTulip = true;
                NewHatalougeManager.Instance.quest3Found = true;   // unlock quest page 3
                NewHatalougeManager.Instance.UpdateQuestScreen();
                GameDataManager2.Instance.SaveGame();
                Debug.Log("Tulip's quest completed!");
                break;

            default:
                Debug.LogWarning("Unknown quest giver: " + questGiver);
                break;
        }
    }

    public bool AllQuestsCompleted()
    {
        return questCompleteLira && questCompleteMallow && questCompleteTulip;
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
    private IEnumerator ShowAllHatsCapturedPopup()
    {
        yield return new WaitForSeconds(2f);
        SoundManager.Instance.PlaySFX(SoundManager.Instance.rotatingPuzzlePartCompleteSound.clip);

        allHatsPopupShown = true;

        endingHintScreen.SetActive(true);

        CanvasGroup cg = endingHintScreen.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            Debug.LogError("CanvasGroup missing on allHatsCapturedObject!");
            yield break;
        }

        // Fade in instantly
        cg.alpha = 1f;

        // Wait before fading out
        yield return new WaitForSeconds(hatsPopupDuration);

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        cg.alpha = 0f;
        endingHintScreen.SetActive(false);
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
