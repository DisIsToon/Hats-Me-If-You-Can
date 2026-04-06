using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;


public class IntroCutscenePlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public MenuManager menuManager;

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    public void StopCutscene()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop(); // completely stops video + audio
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (menuManager != null) {
            menuManager.ShowTutorialPopupAfterDelay();
        }
    }

    void Update()
    {
        // Skip with any key
        if (Input.anyKeyDown)
        {
            if (menuManager != null) {
                menuManager.ShowTutorialPopupAfterDelay();
            }
        }
    }
}
