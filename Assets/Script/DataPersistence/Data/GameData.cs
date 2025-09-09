using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameData
{
    public float playTime;

    public Vector3 playerPosition;

    public SerializableDictionary<string, bool> potionsCollected;

    // the values defined in this constructor will be the default values
    // the game starts with when there's no data to load
    public GameData() 
    {
        this.playTime = 0.0f;
        playerPosition = new Vector3(16f, -0.8f, -2f);
        potionsCollected = new SerializableDictionary<string, bool>();
    }

    // Public function to get playtime
    public float GetFinalGamePlayTime()
    {
        return playTime;
    }
}
