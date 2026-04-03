using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles equipping a throwable item from inventory, aiming, arc preview,
/// and throwing. Attach this to the Player.
/// Each potion can use its own impact VFX prefab.
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
    public float impactVFXLifetime = 2f;
    public Vector3 impactVFXScale = Vector3.one;

    [Header("Audio")]
    public AudioClip throwSFX;
    [Range(0f, 5f)] public float throwVolume = 2f;

    public AudioClip breakSFX;
    [Range(0f, 5f)] public float breakVolume = 3f;

    [System.Serializable]
    public class ThrowableDefinition
    {
        public string itemName;
        public GameObject prefab;
        public GameObject impactVFXPrefab;
    }

    [Header("Throwable Items (from Inventory)")]
    public List<ThrowableDefinition> throwableItems = new List<ThrowableDefinition>();

    [Header("Arc Visuals (Laser Beam)")]
    public Material laserBeamMaterial;
    public bool instanceLaserMaterial = true;

    private LineRenderer arcRenderer;
    private GameObject landingMarker;

    private GameObject heldItem;
    private Rigidbody heldRb;
    private Collider heldCol;
    private EquippableItem heldEquippable;

    private bool isAiming;
    private bool isHighArc;

    private RigidbodyPlayerWithSprintAndStamina player;

    private GameObject currentImpactVFXPrefab;

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

        // Auto-detect an item already parented to the hand
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
        if (heldItem == null || heldRb == null) return;

        if (heldEquippable != null)
        {
            heldEquippable.OnThrow();
        }

        GameObject thrownObject = heldItem;
        Rigidbody thrownRb = heldRb;
        Collider thrownCol = heldCol;

        thrownObject.transform.SetParent(null);
        thrownRb.isKinematic = false;
        thrownRb.useGravity = true;

        if (thrownCol != null)
            thrownCol.enabled = true;

        float force = GetCurrentThrowForce();
        Vector3 throwDir = GetThrowDirection();
        thrownRb.AddForce(throwDir * force, ForceMode.Impulse);

        // Play throw sound near the player/camera so it is easier to hear
        if (throwSFX != null)
        {
            Vector3 soundPos = cameraTransform != null ? cameraTransform.position : transform.position;
            AudioSource.PlayClipAtPoint(throwSFX, soundPos, throwVolume);
        }

        ThrowableImpact impact = thrownObject.GetComponent<ThrowableImpact>();
        if (impact == null)
        {
            impact = thrownObject.AddComponent<ThrowableImpact>();
        }

        Debug.Log("Throwing with VFX: " + (currentImpactVFXPrefab != null ? currentImpactVFXPrefab.name : "NULL"));

        impact.Arm(
            destroyDelayAfterHit,
            currentImpactVFXPrefab,
            impactVFXLifetime,
            impactVFXScale,
            breakSFX,
            breakVolume
        );

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
        if (heldItem == null || handPosition == null) return;

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
                if (Physics.Linecast(lastPoint, point, out RaycastHit hit, arcCollisionMask, QueryTriggerInteraction.Ignore))
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

        if (landingMarker != null)
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

        // Match held object to throwable definition and assign proper impact VFX
        currentImpactVFXPrefab = null;

        string heldName = heldItem.name.Replace("(Clone)", "").Trim();

        foreach (ThrowableDefinition def in throwableItems)
        {
            if (def.prefab == null) continue;

            string defName = def.prefab.name.Replace("(Clone)", "").Trim();

            if (heldName == defName)
            {
                currentImpactVFXPrefab = def.impactVFXPrefab;
                break;
            }
        }

        Debug.Log("Held item: " + heldName + " | Impact VFX: " +
            (currentImpactVFXPrefab != null ? currentImpactVFXPrefab.name : "NULL"));
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

        currentImpactVFXPrefab = def.impactVFXPrefab;

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
        currentImpactVFXPrefab = null;

        arcRenderer.enabled = false;
        if (landingMarker) landingMarker.SetActive(false);
    }
}

public class ThrowableImpact : MonoBehaviour
{
    private bool armed = false;
    private bool hasHit = false;
    private float destroyDelay = 0.08f;
    private GameObject impactVFXPrefab;
    private float impactVFXLifetime = 2f;
    private Vector3 impactVFXScale = Vector3.one;

    private AudioClip impactSFX;
    private float impactSFXVolume = 1f;

    public void Arm(float delay, GameObject vfxPrefab, float vfxLifetime, Vector3 vfxScale, AudioClip sfx, float sfxVolume)
    {
        destroyDelay = delay;
        impactVFXPrefab = vfxPrefab;
        impactVFXLifetime = vfxLifetime;
        impactVFXScale = vfxScale;
        impactSFX = sfx;
        impactSFXVolume = sfxVolume;
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

        Vector3 hitPoint = transform.position;
        Vector3 hitNormal = Vector3.up;
        Quaternion hitRotation = Quaternion.identity;

        if (collision.contactCount > 0)
        {
            ContactPoint contact = collision.contacts[0];
            hitPoint = contact.point + contact.normal * 0.15f;
            hitNormal = contact.normal;
            hitRotation = Quaternion.LookRotation(hitNormal);
        }

        if (impactVFXPrefab != null)
        {
            GameObject vfx = Instantiate(impactVFXPrefab, hitPoint, hitRotation);
            vfx.transform.localScale = impactVFXScale;

            Debug.Log("Spawned VFX: " + vfx.name);

            Destroy(vfx, impactVFXLifetime);
        }
        else
        {
            Debug.LogWarning("Impact VFX Prefab is NULL on thrown object.");
        }

        // Play break sound near the camera/player so it is easier to hear
        if (impactSFX != null)
        {
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : hitPoint;
            AudioSource.PlayClipAtPoint(impactSFX, soundPos, impactSFXVolume);
        }

        EquipSystem.Instance.ConsumeSelectedItem();

        Destroy(gameObject, destroyDelay);
    }
}