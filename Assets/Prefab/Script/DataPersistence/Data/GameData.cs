using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameData
{
    public bool shyHatAlreadyCaptured;
    public bool fastHatAlreadyCaptured;
    public bool jumpHatAlreadyCaptured;

    public bool CRBarrierAlreadyOpened;
    public bool WBBarrierAlreadyOpened;
    public bool mirrorClaimed;

    public bool questAlreadyCompleteLira;
    public bool questAlreadyCompleteMallow;
    public bool questAlreadyCompleteTulip;

    public bool castleRuinAlreadyDiscovered;
    public bool winterBiomeAlreadyDiscovered;
    public bool springGardenAlreadyDiscovered;

    public long lastUpdated;
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

        shyHatAlreadyCaptured = false;
        fastHatAlreadyCaptured = false;
        jumpHatAlreadyCaptured = false;

        CRBarrierAlreadyOpened = false;
        WBBarrierAlreadyOpened = false;
        mirrorClaimed = false;

        questAlreadyCompleteLira = false;
        questAlreadyCompleteMallow = false;
        questAlreadyCompleteTulip = false;

        castleRuinAlreadyDiscovered = false;
        winterBiomeAlreadyDiscovered = false;
        springGardenAlreadyDiscovered = false;
}

    // Public function to get playtime
    public float GetFinalGamePlayTime()
    {
        return playTime;
    }
}
