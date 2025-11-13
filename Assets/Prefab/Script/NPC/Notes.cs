using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Notes : MonoBehaviour
{
    [Header("Settings")]
    public GameObject dialogScreen;  // Assign your dialog UI panel here
    public KeyCode interactKey = KeyCode.E;

    public bool isPlayerInRange = false;
    public bool isTalkingWithPlayer = false;
    public Transform player;

    public Button optionButton1;
    public TextMeshProUGUI optionButton1Text;

    public GameObject MainScreen;

    public bool activeNote;

    public static Notes Instance { get; set; }

    private void Awake()
    {
            Instance = this;
    }

    void Start()
    {
        dialogScreen.SetActive(false);
        activeNote = false;

        if (optionButton1 != null)
        {
            optionButton1.onClick.AddListener(CloseDialog);
            optionButton1.gameObject.SetActive(false);
        }

        if (optionButton1Text != null)
            optionButton1Text.text = "Close"; // Default button text
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            
            ToggleDialog();
        }
    }

    public void ToggleDialog()
    {
        SelectionManager.Instance.readableNoteDetected = false;
        dialogScreen.SetActive(true);
        activeNote = true;
        optionButton1.gameObject.SetActive(true);

        MainScreen.SetActive(false);
    }

    public void CloseDialog()
    {
        dialogScreen.SetActive(false);
        activeNote=false;
        optionButton1.gameObject.SetActive(false);
        MainScreen.SetActive(true);
    }

    /*public void LookAtPlayer()
    {
        var player = PlayerState.Instance.playerBody.transform;
        Vector3 direction = player.position - transform.position;
        transform.rotation = Quaternion.LookRotation(direction);

        var yRotation = transform.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0, yRotation, 0);

    }
    */

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            MainScreen.SetActive(true);
            activeNote = false;
            dialogScreen.SetActive(false);
            optionButton1.gameObject.SetActive(false);
        }
    }



}
