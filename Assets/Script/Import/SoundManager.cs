using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; set; }

    //SFX 
    public AudioSource dropItemSound;
    public AudioSource craftingSound;
    public AudioSource pickUpSound;
    public AudioSource walkGrassSound;
    public AudioSource jumpSound;
    public AudioSource axeSwingSound;
    public AudioSource woodHitSound;
    public AudioSource swordSwingSound;
    public AudioSource eatingSound;
    public AudioSource woodSplitSound;
    public AudioSource stoneHitSound;
    public AudioSource stoneSplitSound;

    //Music 
    public AudioSource startingZoneBGMusic;



    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);    
        }
        else
        {
            Instance = this;
        }
    }

    public void PlaySound(AudioSource soundToPlay)
    {
        if(!soundToPlay.isPlaying)
        {
            soundToPlay.Play();
        }
    }
}
