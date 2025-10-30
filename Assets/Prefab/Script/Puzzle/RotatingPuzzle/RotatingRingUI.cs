using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class RotatingRingUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Rotation Settings")]
    [Tooltip("Number of divisions in one full 360° rotation (e.g., 12 = 30° per snap)")]
    public int segments = 12;

    [Tooltip("Rotation speed sensitivity for dragging")]
    public float rotationSpeed = 1.0f;

    [Tooltip("The correct rotation for this ring (0, 360, 720, etc.)")]
    public float correctRotation = 0f;

    [Tooltip("UI Popup that appears when this ring is solved (optional)")]
    public GameObject solvedPopupUI;

    public Action OnSnapped; // Event for manager
    public Action<RotatingRingUI> OnRingSolved; // Event for solved state

    private RectTransform rectTransform;
    private bool isDragging = false;
    private bool isLocked = false;
    private bool isAlreadyCorrect = false;
    private float startAngle;

    private float snapStep => 360f / Mathf.Max(1, segments);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (solvedPopupUI != null)
            solvedPopupUI.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked) return;

        isDragging = true;
        Vector2 dir = eventData.position - (Vector2)rectTransform.position;
        startAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - rectTransform.eulerAngles.z;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isLocked) return;

        Vector2 dir = eventData.position - (Vector2)rectTransform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - startAngle;

        rectTransform.rotation = Quaternion.Euler(0, 0, angle * rotationSpeed);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging || isLocked) return;
        isDragging = false;

        float currentZ = rectTransform.eulerAngles.z;
        float snappedZ = Mathf.Round(currentZ / snapStep) * snapStep;
        rectTransform.rotation = Quaternion.Euler(0, 0, snappedZ);

        OnSnapped?.Invoke();

        // Check if correct after snapping
        if (IsInCorrectPosition() && !isAlreadyCorrect)
        {
            isAlreadyCorrect = true;
            isLocked = true;

            if (solvedPopupUI != null)
                StartCoroutine(ShowSolvedPopup());

            OnRingSolved?.Invoke(this);
        }
    }

    private IEnumerator ShowSolvedPopup()
    {
        solvedPopupUI.SetActive(true);
        yield return new WaitForSeconds(1f);
        solvedPopupUI.SetActive(false);
    }

    public bool IsInCorrectPosition()
    {
        float currentZ = rectTransform.eulerAngles.z;
        float normalizedZ = Mathf.Repeat(currentZ, 360f);
        float normalizedCorrect = Mathf.Repeat(correctRotation, 360f);

        if (Mathf.Approximately(normalizedCorrect, 0f))
        {
            float diffToUpright = Mathf.Abs(Mathf.DeltaAngle(normalizedZ, 0f));
            return diffToUpright < (snapStep / 2f);
        }
        else
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(normalizedZ, normalizedCorrect));
            return diff < (snapStep / 2f);
        }
    }

    public bool IsCorrect() => IsInCorrectPosition();
    public bool IsLocked() => isLocked;

    public void SetRotation(float degrees)
    {
        rectTransform.localEulerAngles = new Vector3(0, 0, degrees);
    }

    public float GetRotation()
    {
        return Mathf.Repeat(rectTransform.eulerAngles.z, 360f);
    }
}
