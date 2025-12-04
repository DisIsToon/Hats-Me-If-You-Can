using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;


public class IntroCutscenePlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadSceneAsync("BiomeOptimized");
    }

    void Update()
    {
        // Skip with any key
        if (Input.anyKeyDown)
        {
            SceneManager.LoadSceneAsync("BiomeOptimized");
        }
    }
}
