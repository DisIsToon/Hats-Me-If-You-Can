using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles equipping a throwable item from inventory, aiming, arc preview,
/// and throwing. Attach this to the Player.
/// Thrown objects disappear shortly after collision so hit logic still works.
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
    public Vector3 aimHoldOffset = Vector3.zero;
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

    [Header("Impact")]
    public float destroyDelayAfterHit = 0.08f;

    [System.Serializable]
    public class ThrowableDefinition
    {
        public string itemName;
        public GameObject prefab;
    }

    [Header("Throwable Items (from Inventory)")]
    public List<ThrowableDefinition> throwableItems = new List<ThrowableDefinition>();

    [Header("Arc Visuals (Laser Beam)")]
    public Material laserBeamMaterial;
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

    void Start()
    {
        player = GetComponent<RigidbodyPlayerWithSprintAndStamina>();

        arcRenderer = gameObject.AddComponent<LineRenderer>();
        arcRenderer.useWorldSpace = true;
        arcRenderer.positionCount = arcResolution;
        arcRenderer.startWidth = lineWidth;
        arcRenderer.endWidth = lineWidth;
        arcRenderer.textureMode = LineTextureMode.Tile;
        arcRenderer.alignment = LineAlignment.View;

        if (laserBeamMaterial != null)
        {
            arcRenderer.material = instanceLaserMaterial
                ? new Material(laserBeamMaterial)
                : laserBeamMaterial;
        }
        else
        {
            arcRenderer.material = new Material(Shader.Find("Sprites/Default"));
            arcRenderer.material.color = Color.white;
        }

        arcRenderer.enabled = false;

        landingMarker = landingMarkerPrefab ? Instantiate(landingMarkerPrefab)
                                            : CreateDefaultLandingMarker();
        landingMarker.SetActive(false);
    }

    void Update()
    {
        if (enableHighArc && Input.GetKeyDown(toggleHighArcKey))
        {
            isHighArc = !isHighArc;
        }

        if (heldItem == null && handPosition != null && handPosition.childCount > 0)
        {
            GameObject child = handPosition.GetChild(0).gameObject;
            SetupHeldItem(child);
        }

        if (heldItem == null)
        {
            arcRenderer.enabled = false;
            if (landingMarker) landingMarker.SetActive(false);
            return;
        }

        isAiming = Input.GetMouseButton(1);

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

        if (!uiBlocked && Input.GetMouseButtonDown(0))
        {
            ThrowHeldItem();
        }
    }

    void ThrowHeldItem()
    {
        if (!heldItem || !heldRb) return;

        if (heldEquippable != null)
        {
            heldEquippable.OnThrow();
        }

        GameObject thrownObject = heldItem;
        Rigidbody thrownRb = heldRb;

        thrownObject.transform.SetParent(null);
        thrownRb.isKinematic = false;
        thrownRb.useGravity = true;
        if (heldCol) heldCol.enabled = true;

        float force = GetCurrentThrowForce();
        Vector3 throwDir = GetThrowDirection();
        thrownRb.AddForce(throwDir * force, ForceMode.Impulse);

        ThrowableImpact impact = thrownObject.GetComponent<ThrowableImpact>();
        if (impact == null)
        {
            impact = thrownObject.AddComponent<ThrowableImpact>();
        }
        impact.Arm(destroyDelayAfterHit);

        heldItem = null;
        heldRb = null;
        heldCol = null;
        heldEquippable = null;

        arcRenderer.enabled = false;
        if (landingMarker) landingMarker.SetActive(false);
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

        Vector3 startPos = handPosition.position;
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

        heldItem.transform.localPosition = normalHoldOffset;
        heldItem.transform.localRotation = Quaternion.identity;
    }

    public void EquipFromInventory(string itemName)
    {
        if (handPosition == null)
        {
            Debug.LogWarning("Throw: Hand position not assigned.");
            return;
        }

        ThrowableDefinition def = throwableItems.Find(t => t.itemName == itemName);
        if (def == null || def.prefab == null)
        {
            Debug.LogWarning($"Throw: No throwable prefab mapped for item '{itemName}'.");
            return;
        }

        if (heldItem != null)
        {
            Destroy(heldItem);
        }

        GameObject newItem = Instantiate(def.prefab, handPosition);
        SetupHeldItem(newItem);
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
}

public class ThrowableImpact : MonoBehaviour
{
    bool armed = false;
    bool hasHit = false;
    float destroyDelay = 0.08f;

    public void Arm(float delay)
    {
        destroyDelay = delay;
        StartCoroutine(ArmNextFrame());
    }

    IEnumerator ArmNextFrame()
    {
        yield return null;
        armed = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!armed || hasHit) return;

        hasHit = true;

        // Let your existing collision / minigame scripts react first,
        // then remove the bottle shortly after.
        Destroy(gameObject, destroyDelay);
    }
}