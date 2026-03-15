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
