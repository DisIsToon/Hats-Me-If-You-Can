using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance { get; set; }

    [Header("Video Player Reference")]
    public VideoPlayer craftingVideoPlayer;
    public GameObject videoCanvas;

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

    public void PlayCraftVideo()
    {
        videoCanvas.SetActive(true);
        craftingVideoPlayer.Stop();
        craftingVideoPlayer.Play();

        StartCoroutine(HideAfterVideo());
    }

    private IEnumerator HideAfterVideo()
    {
        while (craftingVideoPlayer.isPlaying)
            yield return null;

        videoCanvas.SetActive(false);
    }
}
