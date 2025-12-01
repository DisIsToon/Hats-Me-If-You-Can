using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    public static ThirdPersonCameraController Instance { get; private set; }

    [SerializeField] public float zoomSpeed = 2f;
    [SerializeField] public float zoomLerpSpeed = 10f;
    [SerializeField] public float minDistance = 3f;
    [SerializeField] public float maxDistance = 15f;

    public InputSystem_Actions controls;

    public CinemachineCamera cam;
    public CinemachineOrbitalFollow orbital;
    public Vector2 scrollDelta;

    public float targetZoom;
    public float currentZoom;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        controls = new InputSystem_Actions();
        controls.Enable();
        controls.CameraControls.MouseZoom.performed += HandleMouseScroll;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam = GetComponent<CinemachineCamera>();
        orbital = cam.GetComponent<CinemachineOrbitalFollow>();

        if (orbital != null)
        {
            targetZoom = currentZoom = orbital.Radius;   // ✔ correct property for your version
        }
    }

    public void HandleMouseScroll(InputAction.CallbackContext context)
    {
        scrollDelta = context.ReadValue<Vector2>();
    }

    void Update()
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

        if (scrollDelta.y != 0f)
        {
            targetZoom = Mathf.Clamp(
                orbital.Radius - scrollDelta.y * zoomSpeed,
                minDistance,
                maxDistance
            );

            scrollDelta = Vector2.zero;
        }

        float bumperData = controls.CameraControls.GamepadZoom.ReadValue<float>();

        if (bumperData != 0f)
        {
            targetZoom = Mathf.Clamp(
                orbital.Radius - bumperData * zoomSpeed,
                minDistance,
                maxDistance
            );
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);

        orbital.Radius = currentZoom;   // ✔ correct property
    }

    public void SetCameraActive(bool active)
    {
        if (active)
        {
            controls.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            enabled = true;  // re-enable Update()
        }
        else
        {
            controls.Disable();
            scrollDelta = Vector2.zero;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            enabled = false; // stop Update() entirely
        }
    }
}
