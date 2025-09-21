using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NPC1 : MonoBehaviour
{
    [Header("References")]
    public Transform player;          // Assign player in inspector (optional if Player has tag "Player")
    public GameObject pressEUI;       // UI prompt ("Press E")
    public GameObject dialogUI;       // Dialog panel UI

    [Header("Settings")]
    public float interactionRange = 3f; // Distance for interaction

    private bool inRange = false;

    void Start()
    {
        if (pressEUI != null) pressEUI.SetActive(false);
        if (dialogUI != null) dialogUI.SetActive(false);

        // Fallback: find player by tag if not assigned in inspector
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
            else Debug.LogWarning("NPCInteraction: player Transform not assigned and no GameObject with tag 'Player' found.");
        }
    }

    void Update()
    {
        if (player == null) return; // nothing to do without player

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= interactionRange)
        {
            inRange = true;

            // Show "Press E" only if dialog UI is not active (or dialogUI is null)
            if (pressEUI != null && (dialogUI == null || !dialogUI.activeSelf))
                pressEUI.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                LookAtPlayer();
                if (dialogUI != null) dialogUI.SetActive(true);
                if (pressEUI != null) pressEUI.SetActive(false);
            }
        }
        else
        {
            inRange = false;

            if (pressEUI != null) pressEUI.SetActive(false);

            // Optional: auto-close dialog when walking away
            if (dialogUI != null && dialogUI.activeSelf)
                dialogUI.SetActive(false);
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
}
