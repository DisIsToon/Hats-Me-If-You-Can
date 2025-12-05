using UnityEngine;
using UnityEngine.Playables;

public class TimelineOnce : MonoBehaviour
{
    public PlayableDirector director;
    private static bool timelinePlayed = false;

    void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();
    }

    void Start()
    {
        if (!timelinePlayed)
        {
            director.time = 0;
            director.Play();
            timelinePlayed = true;

            // Stop director after finishing
            director.stopped += DisableDirector;
        }
        else
        {
            // If already played, disable the director completely
            DisableDirector(director);
        }
    }

    void DisableDirector(PlayableDirector d)
    {
        d.Stop();
        d.enabled = false;       // prevents replay
        this.enabled = false;    // stop this script too
    }
}
