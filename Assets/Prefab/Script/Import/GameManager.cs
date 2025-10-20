using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Transform PlayerTransform { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);  // Optionally, to keep it across scenes
    }

    public void SetPlayerTransform(Transform playerTransform)
    {
        Debug.Log("Player Transform Set From GM");
        PlayerTransform = playerTransform;
    }
}
