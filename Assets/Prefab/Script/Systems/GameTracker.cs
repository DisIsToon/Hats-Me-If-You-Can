using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class GameTracker : MonoBehaviour, IDataPersistence
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

    public void LoadData(GameData data)
    {
        if (IsTutorial()) return; // skip loading in tutorial

        // ----- LOAD RAW BOOLS -----
        this.shyHatAlreadyCaptured = data.shyHatAlreadyCaptured;
        this.fastHatAlreadyCaptured = data.fastHatAlreadyCaptured;
        this.jumpHatAlreadyCaptured = data.jumpHatAlreadyCaptured;

        this.questAlreadyCompleteLira = data.questAlreadyCompleteLira;
        this.questAlreadyCompleteMallow = data.questAlreadyCompleteMallow;
        this.questAlreadyCompleteTulip = data.questAlreadyCompleteTulip;

        this.castleRuinAlreadyDiscovered = data.castleRuinAlreadyDiscovered;
        this.winterBiomeAlreadyDiscovered = data.winterBiomeAlreadyDiscovered;
        this.springGardenAlreadyDiscovered = data.springGardenAlreadyDiscovered;


        // ============================================
        // --- APPLY EFFECTS BASED ON LOADED DATA ------
        // ============================================


        // ----- HATS -----
        if (shyHatAlreadyCaptured)
        {
            shyHatCaptured = true;
            NewHatalougeManager.Instance.DiscoverShyHat();
            // ADD LINE TO REMOVE THE HAT FROM THE SCENE
        }

        if (fastHatAlreadyCaptured)
        {
            fastHatCaptured = true;
            NewHatalougeManager.Instance.DiscoverFastHat();
            // ADD LINE TO REMOVE THE HAT FROM THE SCENE
        }

        if (jumpHatAlreadyCaptured)
        {
            jumpHatCaptured = true;
            NewHatalougeManager.Instance.DiscoverLazyHat();
            // ADD LINE TO REMOVE THE HAT FROM THE SCENE
        }


        // ----- QUESTS -----
        if (questAlreadyCompleteLira)
        {
            questCompleteLira = true;
            NewHatalougeManager.Instance.quest1Found = true;
            NewHatalougeManager.Instance.UpdateQuestScreen();
        }

        if (questAlreadyCompleteMallow)
        {
            questCompleteMallow = true;
            NewHatalougeManager.Instance.quest2Found = true;
            NewHatalougeManager.Instance.UpdateQuestScreen();
        }

        if (questAlreadyCompleteTulip)
        {
            questCompleteTulip = true;
            NewHatalougeManager.Instance.quest3Found = true;
            NewHatalougeManager.Instance.UpdateQuestScreen();
        }


        // ----- BIOMES -----
        if (springGardenAlreadyDiscovered)
        {
            springGardenNotified = true;
            NewHatalougeManager.Instance.ReachForest();
        }

        if (castleRuinAlreadyDiscovered)
        {
            castleRuinNotified = true;
            NewHatalougeManager.Instance.ReachCastle();
        }

        if (winterBiomeAlreadyDiscovered)
        {
            winterBiomeNotified = true;
            NewHatalougeManager.Instance.ReachWinter();
        }
    }


    public void SaveData(GameData data)
    {
        if (IsTutorial()) return; // skip saving in tutorial

        data.shyHatAlreadyCaptured = this.shyHatAlreadyCaptured;
        data.fastHatAlreadyCaptured = this.fastHatAlreadyCaptured;
        data.jumpHatAlreadyCaptured = this.jumpHatAlreadyCaptured;

        data.questAlreadyCompleteLira = this.questAlreadyCompleteLira;
        data.questAlreadyCompleteMallow = this.questAlreadyCompleteMallow;
        data.questAlreadyCompleteTulip = this.questAlreadyCompleteTulip;

        data.castleRuinAlreadyDiscovered = this.castleRuinAlreadyDiscovered;
        data.winterBiomeAlreadyDiscovered = this.winterBiomeAlreadyDiscovered;
        data.springGardenAlreadyDiscovered = this.springGardenAlreadyDiscovered;
    }

    private void Start()
    {
        if (player == null)
            Debug.LogError("Player GameObject not assigned in GameManager!");

        // Hide the RawImage at start
        if (videoRawImage != null)
            videoRawImage.gameObject.SetActive(false);

    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            PlayShyHatVideoAfterCapture();
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
                NotifUIManager.Instance.NotifyBiomeDiscovered("Spring Garden");
                NewHatalougeManager.Instance.ReachForest();
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
                NotifUIManager.Instance.NotifyBiomeDiscovered("Castle Ruins");
                NewHatalougeManager.Instance.ReachCastle();
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
                NotifUIManager.Instance.NotifyBiomeDiscovered("Winter Forest");
                NewHatalougeManager.Instance.ReachWinter();
            }
            return;
        }
    }

    public void SetPuzzleComplete(bool value)
    {
        puzzleComplete = value;
        Debug.Log("Puzzle Complete Status Updated: " + value);
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
                Debug.Log("Lira's quest completed!");
                break;

            case "Mallow":
                questCompleteMallow = true;
                NewHatalougeManager.Instance.quest2Found = true;   // unlock quest page 2
                NewHatalougeManager.Instance.UpdateQuestScreen();
                Debug.Log("Mallow's quest completed!");
                break;

            case "Tulip":
                questCompleteTulip = true;
                NewHatalougeManager.Instance.quest3Found = true;   // unlock quest page 3
                NewHatalougeManager.Instance.UpdateQuestScreen();
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
