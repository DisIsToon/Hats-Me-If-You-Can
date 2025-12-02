using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; set; }

    public GameObject pauseScreenUI;
    public bool isOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isOpen = !isOpen;
            pauseScreenUI.SetActive(isOpen);
        }
    }
}
