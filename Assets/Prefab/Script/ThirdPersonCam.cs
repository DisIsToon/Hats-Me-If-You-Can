using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {

        // Disable camera controls when UI is open
        if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen ||
            CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen ||
            DialogSystem.Instance != null && DialogSystem.Instance.dialogUIActive ||
            QuestManager.Instance != null && QuestManager.Instance.isQuestMenuOpen ||
            CardsController.Instance != null && CardsController.Instance.isOpen ||
            PuzzleManagerUI.Instance != null && PuzzleManagerUI.Instance.isOpen ||
            Notes.Instance != null && Notes.Instance.activeNote ||
            NewHatalougeManager.Instance != null && NewHatalougeManager.Instance.isOpen)
        {
            // Unlock cursor always while UI is open
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;   // 🔥 stop camera movement
        }

        //Rotate orientation
        //if(DialogSystem.Instance.dialogUIActive == false)
        //{
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
            orientation.forward = viewDir.normalized;
        //}
           
        

        //rotate player object
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (inputDir != Vector3.zero)
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }


    }
}
