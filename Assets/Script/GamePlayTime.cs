using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayTime : MonoBehaviour, IDataPersistence
{
    private float playTime;      // holds the time in seconds
    public Text timeText;        // reference to UI Text (drag from Canvas)

    public void LoadData(GameData data)
    {
        this.playTime = data.playTime;
    }

    public void SaveData(ref GameData data)
    {
        data.playTime = this.playTime;
    }

    void Update()
    {
        // add the time since last frame
        playTime += Time.deltaTime;

        // update the UI
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
