using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    public static ThirdPersonCameraController Instance { get; private set; }

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float zoomLerpSpeed = 10f;
    public float minDistance = 3f;
    public float maxDistance = 15f;

    [Header("Input System")]
    public InputSystem_Actions controls;

    [Header("Cinemachine References")]
    public CinemachineCamera cam;
    public CinemachineOrbitalFollow orbital;
    public CinemachineInputAxisController axisController;

    [Header("Zoom State")]
    public Vector2 scrollDelta;
    public float targetZoom;
    public float currentZoom;

    private bool rotationEnabled = true;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        cam = GetComponent<CinemachineCamera>();
        orbital = cam.GetComponent<CinemachineOrbitalFollow>();
        axisController = cam.GetComponent<CinemachineInputAxisController>();

        if (orbital != null)
            targetZoom = currentZoom = orbital.Radius;

        controls = new InputSystem_Actions();
        controls.Enable();
        controls.CameraControls.MouseZoom.performed += HandleMouseScroll;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    public void HandleMouseScroll(InputAction.CallbackContext context)
    {
        scrollDelta = context.ReadValue<Vector2>();
    }

    void Update()
    {
        bool tutorialOpen = TutorialManager.Instance != null &&
                    TutorialManager.Instance.IsAnyTutorialUIOpen;



        bool uiOpen = tutorialOpen || 
                      (InventorySystem.Instance != null && InventorySystem.Instance.isOpen) ||
                      (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen) ||
                      (DialogSystem.Instance != null && DialogSystem.Instance.dialogUIActive) ||
                      (QuestManager.Instance != null && QuestManager.Instance.isQuestMenuOpen) ||
                      (CardsController.Instance != null && CardsController.Instance.isOpen) ||
                      (PuzzleManagerUI.Instance != null && PuzzleManagerUI.Instance.isOpen) ||
                      (Notes.Instance != null && Notes.Instance.activeNote) ||
                      (PauseManager.Instance != null && PauseManager.Instance.isOpen) ||
                      (NewHatalougeManager.Instance != null && NewHatalougeManager.Instance.isOpen);

        if (uiOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            rotationEnabled = false; // stop rotation
        }
        else
        {
            rotationEnabled = true; // allow rotation
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Zoom
        if (scrollDelta.y != 0f && orbital != null)
        {
            targetZoom = Mathf.Clamp(orbital.Radius - scrollDelta.y * zoomSpeed, minDistance, maxDistance);
            scrollDelta = Vector2.zero;
        }

        float bumperData = controls.CameraControls.GamepadZoom.ReadValue<float>();
        if (bumperData != 0f && orbital != null)
        {
            targetZoom = Mathf.Clamp(orbital.Radius - bumperData * zoomSpeed, minDistance, maxDistance);
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
        if (orbital != null) orbital.Radius = currentZoom;

        // Disable rotation in FixedUpdate or LateUpdate of the orbital follow
        if (axisController != null)
            axisController.enabled = rotationEnabled;
    }

    public void SetCameraActive(bool active)
    {
        controls.Enable();
        enabled = active;

        Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !active;

        if (!active)
        {
            scrollDelta = Vector2.zero;
            rotationEnabled = false;
        }
    }
}
