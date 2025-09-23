using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Cauldron : MonoBehaviour
{
    [Header("Exactly 3 recipes to choose from (drag in Inspector)")]
    public PotionRecipeSO[] recipes = new PotionRecipeSO[3];

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;     // Press to open/close menu
    public float interactHintDistance = 3.0f;   // If you don't use triggers, fallback distance check (optional)

    [Header("UI Mode")]
    public bool useCanvasUI = false;            // If true: no OnGUI; you show/hide your own Canvas
    public float onGuiWidth = 440f;             // Width of the debug OnGUI panel

    [Header("Cursor Control")]
    public bool unlockCursorWhenMenuOpens = true;

    [Header("Events (use these for Canvas UI)")]
    public UnityEvent onMenuOpened;
    public UnityEvent onMenuClosed;
    public UnityEvent onBrewed;                 // Fires after a successful brew

    // Runtime state
    private PlayerInventory _player;
    private bool _playerInRange;
    private bool _showMenu;
    private string _lastMessage = "";
    private Camera _mainCam;

    void Reset()
    {
        // Ensure trigger collider & kinematic rigidbody so triggers fire
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Awake()
    {
        _mainCam = Camera.main;
        // Normalize recipes length to exactly 3 (safety)
        if (recipes == null || recipes.Length != 3)
            recipes = new PotionRecipeSO[3];
    }

    void OnTriggerEnter(Collider other)
    {
        var p = other.GetComponent<PlayerInventory>();
        if (p != null)
        {
            _player = p;
            _playerInRange = true;
            if (!useCanvasUI)
                _lastMessage = $"Press {interactKey} to brew.";
        }
    }

    void OnTriggerExit(Collider other)
    {
        var p = other.GetComponent<PlayerInventory>();
        if (p != null && p == _player)
        {
            _playerInRange = false;
            _player = null;
            ToggleMenu(false);
        }
    }

    void Update()
    {
        // Optional fallback if you don't want to use triggers (leave as-is otherwise)
        // It will consider the player "in range" if close enough to the object and a PlayerInventory exists in scene.
        if (!_playerInRange && _player == null)
        {
            var anyPlayer = FindObjectOfType<PlayerInventory>();
            if (anyPlayer != null && Vector3.Distance(anyPlayer.transform.position, transform.position) <= interactHintDistance)
            {
                _player = anyPlayer;
                _playerInRange = true;
                if (!useCanvasUI)
                    _lastMessage = $"Press {interactKey} to brew.";
            }
        }

        if (!_playerInRange || _player == null) return;

        // Toggle menu
        if (Input.GetKeyDown(interactKey))
        {
            ToggleMenu(!_showMenu);
        }
    }

    void OnDisable()
    {
        // Safety: if object gets disabled while open, restore cursor & fire close event
        if (_showMenu)
        {
            ToggleMenu(false);
        }
    }

    private void ToggleMenu(bool open)
    {
        _showMenu = open;

        if (!useCanvasUI)
            _lastMessage = open ? "Choose a potion to brew." : "Closed cauldron menu.";

        if (unlockCursorWhenMenuOpens)
        {
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = open;
        }

        if (open) onMenuOpened?.Invoke();
        else onMenuClosed?.Invoke();
    }

    // Public API if you’re using your own Canvas Buttons.
    public void Brew(int index)
    {
        if (_player == null) return;
        if (recipes == null || recipes.Length != 3) return;
        if (index < 0 || index >= recipes.Length) return;

        var r = recipes[index];
        if (r == null || r.resultPotion == null) return;

        if (_player.Inventory.SpendIngredients(r))
        {
            _player.Inventory.AddResult(r);
            if (!useCanvasUI)
                _lastMessage = $"Brewed {r.resultPotion.itemName} x{r.resultQuantity}!";

            onBrewed?.Invoke();
        }
        else
        {
            if (!useCanvasUI)
                _lastMessage = "Missing ingredients.";
        }
    }

    // ---------- SIMPLE DEBUG OnGUI (turn off by checking 'useCanvasUI') ----------
    void OnGUI()
    {
        if (useCanvasUI) return;                 // You’re handling UI elsewhere
        if (!_playerInRange || _player == null) return;

        // Hint box
        var hintRect = new Rect(20, 20, 380, 60);
        GUI.Box(hintRect, string.IsNullOrEmpty(_lastMessage) ? $"Press {interactKey} to open cauldron." : _lastMessage);

        if (!_showMenu) return;

        // Menu box
        var boxRect = new Rect(20, 100, onGuiWidth, 280);
        GUI.Box(boxRect, "Cauldron — Choose a Potion");

        if (recipes == null || recipes.Length != 3)
        {
            GUI.Label(new Rect(30, 130, onGuiWidth - 20, 30), "Assign exactly 3 recipes in the Inspector.");
            return;
        }

        float y = 140f;
        for (int i = 0; i < recipes.Length; i++)
        {
            var r = recipes[i];
            if (r == null)
            {
                GUI.Label(new Rect(30, y, onGuiWidth - 40, 20), $"Slot {i + 1}: <missing recipe>");
                y += 80f;
                continue;
            }

            bool can = _player.Inventory.CanAfford(r);
            string line1 = $"{i + 1}) {r.resultPotion?.itemName ?? "<No Result>"} x{r.resultQuantity}";
            string line2 = BuildIngredientLine(r);

            GUI.Label(new Rect(30, y, onGuiWidth - 40, 20), line1 + (can ? "" : "  — Missing ingredients"));
            GUI.Label(new Rect(30, y + 20, onGuiWidth - 40, 20), line2);

            bool prev = GUI.enabled;
            GUI.enabled = can;
            if (GUI.Button(new Rect(30, y + 45, 120, 25), "Brew"))
            {
                Brew(i);
            }
            GUI.enabled = prev;

            y += 90f;
        }

        // Inventory debug panel
        GUI.Box(new Rect(25 + onGuiWidth, 100, 260, 280), "Inventory");
        GUI.Label(new Rect(35 + onGuiWidth, 125, 240, 240), _player.Inventory.DebugContents());
    }

    private string BuildIngredientLine(PotionRecipeSO r)
    {
        if (r.ingredients == null || r.ingredients.Count == 0)
            return "Requires: (None)";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("Requires: ");
        for (int j = 0; j < r.ingredients.Count; j++)
        {
            var ing = r.ingredients[j];
            var have = _player.Inventory.GetCount(ing.item);
            sb.Append($"{ing.item.itemName} x{ing.quantity} (you: {have})");
            if (j < r.ingredients.Count - 1) sb.Append(", ");
        }
        return sb.ToString();
    }
}
