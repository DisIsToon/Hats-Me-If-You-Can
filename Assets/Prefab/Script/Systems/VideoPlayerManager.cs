using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerManager : MonoBehaviour
{
    public static VideoPlayerManager Instance { get; set; }

    [Header("UI References")]
    [SerializeField] private GameObject videoCanvas;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Video Settings")]
    [SerializeField] private string videoFileName = "craft.mp4";
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

        videoCanvas.SetActive(false);

        string url = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName); 
        videoPlayer.source = VideoSource.Url; 
        videoPlayer.url = url;

        videoPlayer.playOnAwake = false; 
        videoPlayer.isLooping = false;

        videoPlayer.loopPointReached += OnVideoEnd;
    }

    public void PlayCraftVideo() 
    { 
        StartCoroutine(PrepareAndPlay()); 
    }

    private IEnumerator PrepareAndPlay()
    {
        videoCanvas.SetActive(true); // Prepare video before playing
        videoPlayer.Prepare(); 
        while (!videoPlayer.isPrepared) 
            yield return null; 
        videoPlayer.Play(); 
    }

    private void OnVideoEnd(VideoPlayer vp) 
    { 
        videoCanvas.SetActive(false); 
    }
}

