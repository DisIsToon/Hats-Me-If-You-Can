using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPC : MonoBehaviour
{
    [Header("References")]
    public GameObject pressFUI;   // UI prompt ("Press F")
    public GameObject dialogUI;   // Dialog panel UI

    private bool playerInRange = false;
    public bool isTalkingWithPlayer = false; 
    private Transform player;

    void Start()
    {
        if (pressFUI != null) pressFUI.SetActive(false);
        if (dialogUI != null) dialogUI.SetActive(false);

        // Find player automatically by tag
        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null) player = found.transform;
        else Debug.LogWarning("NPCInteraction: No GameObject with tag 'Player' found.");
    }

    void Update()
    {
        if (!playerInRange) return;

        // Show "Press F" only if dialog is not active
        if (pressFUI != null && (dialogUI == null || !dialogUI.activeSelf))
            pressFUI.SetActive(true);

        if (Input.GetKeyDown(KeyCode.F))
        {
            //LookAtPlayer();
            StartConversation();
            //if (dialogUI != null) dialogUI.SetActive(true);
            if (pressFUI != null) pressFUI.SetActive(false);
        }

        
    }

    // Rotate the NPC to face the player's horizontal direction only (no tilt).
    public void LookAtPlayer()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;

        // ignore vertical difference so NPC doesn't tilt up/down
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f) return; // avoid zero-length look
        transform.rotation = Quaternion.LookRotation(direction);
    }
    
    public void StartConversation()
    {
        isTalkingWithPlayer = true;

        print("Conversation Started");

        DialogSystem.Instance.OpenDialogUI();
        DialogSystem.Instance.dialogText.text = "Hello There";
        DialogSystem.Instance.option1BTN.transform.Find("Text(TMP)").GetComponent<TextMeshProUGUI>().text = "Bye";
        DialogSystem.Instance.option1BTN.onClick.AddListener(() =>
        {
            DialogSystem.Instance.CloseDialogUI();
            isTalkingWithPlayer = false;
        });

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (pressFUI != null) pressFUI.SetActive(false);

            // Optional: close dialog when leaving range
            if (dialogUI != null && dialogUI.activeSelf)
                dialogUI.SetActive(false);
        }
    }

    

}
