using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }

    [Header("Clicks")]
    public AudioSource buttonClickedSound;
    public AudioSource clickedSound;

    [Header("Hats Interaction")]
    public AudioSource catchAttemptSound;
    public AudioSource failedcatchSound;
    public AudioSource shyHatCapturedSound;
    public AudioSource jumpHatCapturedSound;
    public AudioSource fastHatCaptureSound;
    public AudioSource shyHatMovementSound;
    public AudioSource jumpHatMovementSound;
    public AudioSource fastHatMovementSound;

    [Header("Puzzles Sounds")]
    public AudioSource puzzleInteractSound;
    public AudioSource rotatingPuzzleButtonClickedSound;
    public AudioSource rotatingPuzzlePartCompleteSound;
    public AudioSource pzzleCompleteSound;
    public AudioSource puzzleGameOverSound;
    public AudioSource matchingPuzzleCardClicked;
    public AudioSource matchingPuzzleCardMaatch;

    [Header("Hatalogue")]
    public AudioSource hatalogueOpenCloseSound;
    public AudioSource hatalogueNextPageSound;

    [Header("Inventory")]
    public AudioSource openInventory;
    public AudioSource dragUp;
    public AudioSource dragDrop;
    public AudioSource equipPotion;
    public AudioSource brewBtnClicked;
    public AudioSource brewPotion;

    [Header("Notif Sounds")]
    public AudioSource itemCollectSound;
    public AudioSource questCompleteSound;

    [Header("Misc")]
    public AudioSource pauseSound;
    public AudioSource leavesSound;
    public AudioSource walkOnForest;
    public AudioSource walkOnSnow;
    public AudioSource walkOnCastleRuin;

    [Header("BG Music")]
    public AudioSource mainMenuBGMusic;
    public AudioSource forestZoneBGMusic;
    public AudioSource castleZoneBGMusic;
    public AudioSource winterZoneBGMusic;
    public AudioSource hatalougeViewingMusic;
    public AudioSource brewingMusic;
    public AudioSource puzzlePlayingMusic;

    [Header("Music Settings")]
    public float fadeDuration = 1.5f;
    private AudioSource currentBGMusic;
    private string lastBiome = "Forest";   // default, or set on player spawn

    private void Awake()
    {
        Instance = this;

        // Stop ALL background tracks at the beginning
        forestZoneBGMusic.Stop();
        castleZoneBGMusic.Stop();
        winterZoneBGMusic.Stop();
        brewingMusic.Stop();

        // Optional but recommended:
        forestZoneBGMusic.volume = 0;
        castleZoneBGMusic.volume = 0;
        winterZoneBGMusic.volume = 0;
        brewingMusic.volume = 0;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "MainMenu":
                PlayBGMusic(mainMenuBGMusic);
                break;

            case "BiomeOptimized":
                PlayBGMusic(forestZoneBGMusic); // default biome
                break;
        }
    }

    public void PlayBrewingMusic()
    {
        PlayBGMusic(brewingMusic);   // already uses fading
    }

    public void PlayPuzzleMusic()
    {
        PlayBGMusic(puzzlePlayingMusic);   // already uses fading
    }

    public void PlayHatalogueMusic()
    {
        PlayBGMusic(hatalougeViewingMusic);   // already uses fading
    }

    public void ReturnToBiomeMusic()
    {
        // restore biome music based on lastBiome value
        SwitchBiomeMusic(lastBiome);
    }

    public void PlaySound(AudioSource soundToPlay)
    {
        if (!soundToPlay.isPlaying)
            soundToPlay.Play();
    }

    public void PlayBGMusic(AudioSource newMusic)
    {
        if (currentBGMusic == newMusic)
            return;

        // Fade out previous
        if (currentBGMusic != null)
            StartCoroutine(FadeOut(currentBGMusic));

        // Fade in new
        currentBGMusic = newMusic;
        StartCoroutine(FadeIn(newMusic));
    }


    private IEnumerator FadeIn(AudioSource audio)
    {
        audio.volume = 0f;

        if (!audio.isPlaying)
            audio.Play();

        while (audio.volume < 1f)
        {
            audio.volume += Time.deltaTime * 0.5f;
            yield return null;
        }

        audio.volume = 1f;
    }

    private IEnumerator FadeOut(AudioSource audio)
    {
        while (audio.volume > 0f)
        {
            audio.volume -= Time.deltaTime * 0.5f;
            yield return null;
        }

        audio.volume = 0f;
        audio.Stop();
    }


    // -------- UI + BIOME HANDLERS --------

    public void SwitchBiomeMusic(string biome)
    {
        lastBiome = biome;

        switch (biome)
        {
            case "Forest":
                PlayBGMusic(forestZoneBGMusic);
                break;

            case "CastleRuin":
                PlayBGMusic(castleZoneBGMusic);
                break;

            case "Winter":
                PlayBGMusic(winterZoneBGMusic);
                break;
        }
    }

    /*
    public void EnterCraftingScreen() => PlayBGMusic(brewingMusic);
    public void EnterPuzzle() => PlayBGMusic(puzzlePlayingMusic);
    public void EnterHatalogue() => PlayBGMusic(hatalougeViewingMusic);
    public void OpenInventoryScreen() => PlayBGMusic(brewingMusic);
    */

}
