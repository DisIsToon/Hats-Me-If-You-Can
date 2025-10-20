using Unity.Cinemachine;
using UnityEngine;

public class CauldronInteraction : MonoBehaviour
{
    [Header("References")]
    public CinemachineCamera brewCam;
    public GameObject brewingUI;
    public GameObject inventoryUI;
    public GameObject player; // The whole player GameObject

    private bool inRange = false;
    private bool brewingActive = false;

    private void Update()
    {
        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!brewingActive)
                EnterBrewingMode();
            else
                ExitBrewingMode();
        }
    }

    private void EnterBrewingMode()
    {
        brewingActive = true;

        // Enable cauldron camera
        brewCam.gameObject.SetActive(true);

        // Show brewing UI
        brewingUI.SetActive(true);

        // Hide inventory UI
        if (inventoryUI != null)
            inventoryUI.SetActive(false);

        // Disable/hide player
        if (player != null)
            player.SetActive(false);

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ExitBrewingMode()
    {
        brewingActive = false;

        // Disable cauldron camera
        brewCam.gameObject.SetActive(false);

        // Hide brewing UI
        brewingUI.SetActive(false);

        // Show inventory UI
        if (inventoryUI != null)
            inventoryUI.SetActive(true);

        // Enable/show player
        if (player != null)
            player.SetActive(true);

        // Hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            inRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            inRange = false;
    }
}
