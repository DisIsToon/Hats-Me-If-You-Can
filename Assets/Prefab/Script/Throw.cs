using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles equipping a throwable item, aiming, arc preview, and throwing.
/// Uses a LineRenderer with a custom laser beam material (e.g. MinionsArt Laser).
/// Works with ItemSlot + InventoryItem + QuickSlot tag (no separate InventorySystem logic).
/// Attach this to the Player.
/// </summary>
public class Throw : MonoBehaviour
{
    [Header("References")]
    public Transform handPosition;
    public Transform cameraTransform;

    [Header("Throw Settings")]
    public float throwForce = 10f;
    [Range(0f, 60f)]
    public float throwElevationAngle = 18f;

    public Vector3 normalHoldOffset = Vector3.zero;
    public Vector3 aimHoldOffset = new Vector3(0f, 0.1f, 0.4f);
    public float aimSmoothSpeed = 10f;

    [Header("Throw Arc Settings")]
    public LayerMask arcCollisionMask;
    public int arcResolution = 60;
    public float arcTimeStep = 0.04f;
    public float lineWidth = 0.05f;

    [Header("Landing Marker")]
    public GameObject landingMarkerPrefab;
    public float landingMarkerScale = 0.25f;

    [Header("High Arc / Lob Mode")]
    public bool enableHighArc = true;
    public KeyCode toggleHighArcKey = KeyCode.V;
    [Range(0f, 80f)] public float lobElevationAngle = 38f;
    public float lobForceMultiplier = 1.4f;

    [System.Serializable]
    public class ThrowableDefinition
    {
        public string itemName;      // optional: used if you call EquipFromInventory by name
        public GameObject prefab;    // prefab to equip in hand / throw
    }

    [Header("Throwable Items (Prefabs)")]
    public List<ThrowableDefinition> throwableItems = new List<ThrowableDefinition>();

    // 🔹 Arc visuals (MinionsArt laser)
    [Header("Arc Visuals (Laser Beam)")]
    [Tooltip("Material that uses the MinionsArt Laser shader (LaserBeam/LaserBeam2D/BirpLaser).")]
    public Material laserBeamMaterial;

    [Tooltip("Create a unique instance of the material for this LineRenderer.")]
    public bool instanceLaserMaterial = true;

    LineRenderer arcRenderer;
    GameObject landingMarker;

    GameObject heldItem;
    Rigidbody heldRb;
    Collider heldCol;
    EquippableItem heldEquippable;

    bool isAiming;
    bool isHighArc;

    RigidbodyPlayerWithSprintAndStamina player;

    // Cache of quick slots (ItemSlots tagged as QuickSlot)
    ItemSlot[] quickSlots;

    void Start()
    {
        player = GetComponent<RigidbodyPlayerWithSprintAndStamina>();

        // Arc Renderer setup
        arcRenderer = gameObject.AddComponent<LineRenderer>();
        arcRenderer.useWorldSpace = true;
        arcRenderer.positionCount = arcResolution;

        // Width
        arcRenderer.startWidth = lineWidth;
        arcRenderer.endWidth = lineWidth;

        // Texture mode: for laser shaders this is usually TILED
        arcRenderer.textureMode = LineTextureMode.Tile;

        // Alignment (View makes it face the camera nicely)
        arcRenderer.alignment = LineAlignment.View;

        // Material
        if (laserBeamMaterial != null)
        {
            arcRenderer.material = instanceLaserMaterial
                ? new Material(laserBeamMaterial)
                : laserBeamMaterial;
        }
        else
        {
            // Fallback if you forget to assign the laser material
            arcRenderer.material = new Material(Shader.Find("Sprites/Default"));
            arcRenderer.material.color = Color.white;
        }

        // MinionsArt shader scrolls on its own via material properties
        arcRenderer.enabled = false;

        // Landing marker
        landingMarker = landingMarkerPrefab ? Instantiate(landingMarkerPrefab)
                                            : CreateDefaultLandingMarker();
        landingMarker.SetActive(false);

        // 🔹 Cache all quickslots in the scene (objects tagged "QuickSlot")
        List<ItemSlot> slotList = new List<ItemSlot>();
        GameObject[] quickSlotObjects = GameObject.FindGameObjectsWithTag("QuickSlot");
        foreach (var go in quickSlotObjects)
        {
            ItemSlot slot = go.GetComponent<ItemSlot>();
            if (slot != null)
                slotList.Add(slot);
        }
        quickSlots = slotList.ToArray();
    }

    void Update()
    {
        // Toggle lob mode
        if (enableHighArc && Input.GetKeyDown(toggleHighArcKey))
        {
            isHighArc = !isHighArc;
        }

        // Auto-detect an item in the hand if something is parented but heldItem is not set
        if (heldItem == null && handPosition != null && handPosition.childCount > 0)
        {
            GameObject child = handPosition.GetChild(0).gameObject;
            SetupHeldItem(child);
        }

        // If still nothing equipped → no aiming or throwing
        if (heldItem == null)
        {
            arcRenderer.enabled = false;
            if (landingMarker) landingMarker.SetActive(false);
            return;
        }

        // Aiming (right mouse)
        isAiming = Input.GetMouseButton(1);
        if (handPosition)
        {
            Vector3 targetOffset = isAiming ? aimHoldOffset : normalHoldOffset;
            heldItem.transform.localPosition = Vector3.Lerp(
                heldItem.transform.localPosition,
                targetOffset,
                Time.deltaTime * aimSmoothSpeed);
        }

        // Arc preview
        if (isAiming)
        {
            arcRenderer.enabled = true;
            DrawThrowArcAndMarker();
        }
        else
        {
            arcRenderer.enabled = false;
            if (landingMarker) landingMarker.SetActive(false);
        }

        // UI / menu gating with null checks
        bool uiBlocked = false;
        if (DialogSystem.Instance != null && DialogSystem.Instance.dialogUIActive) uiBlocked = true;
        if (CraftingSystem.Instance != null && CraftingSystem.Instance.isOpen) uiBlocked = true;
        if (InventorySystem.Instance != null && InventorySystem.Instance.isOpen) uiBlocked = true;
        if (QuestManager.Instance != null && QuestManager.Instance.isQuestMenuOpen) uiBlocked = true;
        if (CardsController.Instance != null && CardsController.Instance.isOpen) uiBlocked = true;
        if (PuzzleManagerUI.Instance != null && PuzzleManagerUI.Instance.isOpen) uiBlocked = true;
        if (Notes.Instance != null && Notes.Instance.activeNote) uiBlocked = true;
        if (NewHatalougeManager.Instance != null && NewHatalougeManager.Instance.isOpen) uiBlocked = true;
        if (player != null &&
            player.currentInteractingNPC != null &&
            player.currentInteractingNPC.isTalkingWithPlayer)
        {
            uiBlocked = true;
        }

        // Throw (left mouse) if not blocked
        if (!uiBlocked && Input.GetMouseButtonDown(0))
        {
            ThrowHeldItem();
        }
    }

    #region Core Throw Logic
    void ThrowHeldItem()
    {
        if (!heldItem || !heldRb) return;

        if (heldEquippable != null)
        {
            heldEquippable.OnThrow();
        }

        // Cache reference before clearing so coroutine still has the object
        GameObject thrownObject = heldItem;

        // Detach & enable physics
        thrownObject.transform.SetParent(null);
        heldRb.isKinematic = false;
        heldRb.useGravity = true;
        if (heldCol) heldCol.enabled = true;

        float force = GetCurrentThrowForce();
        Vector3 throwDir = GetThrowDirection();
        heldRb.AddForce(throwDir * force, ForceMode.Impulse);

        // 🔹 Reduce stack in the matching quickslot InventoryItem
        ReduceQuickSlotStackForHeldItem();

        // 🔹 Bottle-style: destroy after it lands / slows down
        StartCoroutine(DestroyAfterThrow(thrownObject));

        // Clear state on this component for the held world object
        heldItem = null;
        heldRb = null;
        heldCol = null;
        heldEquippable = null;

        arcRenderer.enabled = false;
        if (landingMarker) landingMarker.SetActive(false);
    }

    void ReduceQuickSlotStackForHeldItem()
    {
        if (heldEquippable == null) return;
        if (quickSlots == null || quickSlots.Length == 0) return;

        // We use the EquippableItem.itemName to match the InventoryItem
        string eqName = heldEquippable.itemName;
        if (string.IsNullOrEmpty(eqName)) return;

        foreach (var slot in quickSlots)
        {
            if (slot == null) continue;

            GameObject uiItem = slot.Item; // from your ItemSlot script
            if (uiItem == null) continue;

            // Must match your real InventoryItem component
            var invItem = uiItem.GetComponent<InventoryItem>();
            if (invItem == null) continue;

            // TODO: Change "itemName" and "itemCount" if your InventoryItem uses different field names
            if (invItem.itemName == eqName)
            {
                invItem.itemCount--;

                if (invItem.itemCount <= 0)
                {
                    // remove the item from this quickslot
                    Object.Destroy(uiItem);
                }
                else
                {
                    // If you have UI text for count, update it here:
                    // invItem.RefreshCountText();
                }

                break; // found matching quickslot, done
            }
        }
    }

    float GetCurrentThrowForce()
    {
        float f = isAiming ? throwForce * 1.2f : throwForce;
        if (isHighArc) f *= lobForceMultiplier;
        return f;
    }

    Vector3 GetThrowDirection()
    {
        Vector3 planarForward = cameraTransform
            ? Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized
            : Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        Vector3 right = cameraTransform ? cameraTransform.right : transform.right;

        float elevation = isHighArc ? lobElevationAngle : throwElevationAngle;
        Vector3 dir = Quaternion.AngleAxis(elevation, right) * planarForward;

        const float minPitchDeg = 10f;
        float minY = Mathf.Sin(minPitchDeg * Mathf.Deg2Rad);
        if (dir.y < minY) dir.y = minY;

        return dir.normalized;
    }

    void DrawThrowArcAndMarker()
    {
        if (!heldItem || !handPosition) return;

        Vector3 startPos = handPosition.position + (transform.forward * 0.08f);
        float force = GetCurrentThrowForce();

        Vector3 throwDir = GetThrowDirection();
        Vector3 startVel = throwDir * force;

        Vector3[] points = new Vector3[arcResolution];
        bool hitFound = false;
        RaycastHit hitInfo = default;

        Vector3 lastPoint = startPos;
        for (int i = 0; i < arcResolution; i++)
        {
            float t = i * arcTimeStep;
            Vector3 point = startPos + startVel * t + 0.5f * Physics.gravity * (t * t);
            points[i] = point;

            if (i > 0)
            {
                if (Physics.Linecast(lastPoint, point, out RaycastHit hit, arcCollisionMask,
                    QueryTriggerInteraction.Ignore))
                {
                    points[i] = hit.point;
                    for (int j = i + 1; j < arcResolution; j++) points[j] = hit.point;
                    hitFound = true;
                    hitInfo = hit;
                    break;
                }
            }
            lastPoint = point;
        }

        arcRenderer.positionCount = arcResolution;
        arcRenderer.SetPositions(points);

        if (landingMarker)
        {
            landingMarker.SetActive(hitFound);
            if (hitFound)
            {
                landingMarker.transform.position = hitInfo.point + hitInfo.normal * 0.01f;
                landingMarker.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
        }
    }
    #endregion

    #region Destroy After Throw
    /// <summary>
    /// Waits until the thrown object slows down / lands, then destroys it.
    /// Makes the throwable behave like a bottle that breaks and disappears.
    /// </summary>
    private IEnumerator DestroyAfterThrow(GameObject item)
    {
        if (item == null) yield break;

        Rigidbody rb = item.GetComponent<Rigidbody>();

        // If no rigidbody, just destroy after a short delay
        if (rb == null)
        {
            yield return new WaitForSeconds(2f);
            if (item != null) Destroy(item);
            yield break;
        }

        float elapsed = 0f;
        float minLifetime = 0.1f;   // ensure it exists at least briefly
        float maxLifetime = 5f;     // safety timeout

        while (item != null && elapsed < maxLifetime)
        {
            elapsed += Time.deltaTime;

            if (elapsed > minLifetime)
            {
                // sqrMagnitude cheaper than magnitude
                if (rb.linearVelocity.sqrMagnitude < 0.01f)
                {
                    break;
                }
            }

            yield return null;
        }

        if (item != null)
        {
            Destroy(item);
        }
    }
    #endregion

    #region Utils & Public API
    GameObject CreateDefaultLandingMarker()
    {
        GameObject ring = new GameObject("LandingMarker_Auto");
        var lr = ring.AddComponent<LineRenderer>();
        lr.loop = true;
        lr.widthMultiplier = 0.02f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = new Color(1, 1, 1, 0.9f);
        int segs = 40;
        lr.positionCount = segs;
        float r = landingMarkerScale;
        for (int i = 0; i < segs; i++)
        {
            float a = (i / (float)segs) * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, 0, Mathf.Sin(a) * r));
        }
        return ring;
    }

    public bool HasItem() => heldItem != null;
    public bool IsAiming() => isAiming;

    void SetupHeldItem(GameObject newItem)
    {
        if (newItem == null) return;

        heldItem = newItem;
        heldRb = heldItem.GetComponent<Rigidbody>();
        heldCol = heldItem.GetComponent<Collider>();
        heldEquippable = heldItem.GetComponent<EquippableItem>();

        if (heldRb != null)
        {
            heldRb.isKinematic = true;
            heldRb.useGravity = false;
        }

        if (heldCol != null)
        {
            heldCol.enabled = false;
        }

        if (heldEquippable != null)
        {
            heldEquippable.OnEquip();
        }
    }

    /// <summary>
    /// Optional: if you want to equip by name instead of quickslot directly.
    /// Searches quickslots for a matching InventoryItem.itemName.
    /// </summary>
    public void EquipFromInventory(string itemName)
    {
        if (quickSlots == null || quickSlots.Length == 0)
        {
            Debug.LogWarning("EquipFromInventory: No quickSlots found.");
            return;
        }

        foreach (var slot in quickSlots)
        {
            if (slot == null || slot.Item == null) continue;

            var invItem = slot.Item.GetComponent<InventoryItem>();
            if (invItem == null) continue;

            // TODO: change "itemName" if your InventoryItem uses a different field name
            if (invItem.itemName == itemName)
            {
                // Just rely on your existing system to spawn item into hand,
                // or you can instantiate prefab here if you want.
                // For now, we assume your other code handles equipping.
                return;
            }
        }

        Debug.LogWarning($"EquipFromInventory: No quickslot found with item '{itemName}'.");
    }

    public void Unequip()
    {
        if (heldEquippable != null)
            heldEquippable.OnUnequip();

        if (heldItem != null)
            Destroy(heldItem);

        heldItem = null;
        heldRb = null;
        heldCol = null;
        heldEquippable = null;

        arcRenderer.enabled = false;
        if (landingMarker) landingMarker.SetActive(false);
    }
    #endregion
}
