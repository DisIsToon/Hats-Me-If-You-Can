using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RotatingRingUI : MonoBehaviour
{
    [Header("Rotation Settings")]
    public int segments = 12;                  // number of snap points (12 = 30° each)
    public float rotationSpeed = 500f;         // degrees per second for animation

    [Header("UI Buttons")]
    public Button rotateLeftButton;
    public Button rotateRightButton;

    [Header("Puzzle State")]
    public bool isCorrect = false;
    public bool isLocked = false;

    private float snapAngle;
    private float targetRotation;
    private RectTransform rectTransform;
    private PuzzleManagerUI puzzleManager;
    private bool isRotating = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        puzzleManager = FindObjectOfType<PuzzleManagerUI>();

        snapAngle = 360f / segments;
        targetRotation = rectTransform.localEulerAngles.z;

        // hook up buttons
        if (rotateLeftButton)
            rotateLeftButton.onClick.AddListener(() => TryRotate(true));
        if (rotateRightButton)
            rotateRightButton.onClick.AddListener(() => TryRotate(false));
    }

    void TryRotate(bool left)
    {
        if (isLocked || isRotating) return;

        float direction = left ? 1f : -1f;
        targetRotation += snapAngle * direction;

        StartCoroutine(SmoothSnapRotation());
    }

    IEnumerator SmoothSnapRotation()
    {
        isRotating = true;

        float startZ = rectTransform.localEulerAngles.z;
        float endZ = targetRotation;
        float totalAngle = Mathf.DeltaAngle(startZ, endZ);
        float rotated = 0f;

        while (Mathf.Abs(rotated) < Mathf.Abs(totalAngle))
        {
            float step = rotationSpeed * Time.deltaTime * Mathf.Sign(totalAngle);
            rotated += step;
            rectTransform.Rotate(0, 0, step);
            yield return null;
        }

        rectTransform.localEulerAngles = new Vector3(0, 0, endZ);
        isRotating = false;

        CheckCorrectRotation();
    }

    void CheckCorrectRotation()
    {
        float z = rectTransform.localEulerAngles.z;
        float normalized = NormalizeAngle(z);

        // ✅ Correct if rotation is near 0°, 360°, or 720° (multiple of 360)
        float mod360 = Mathf.Abs(z % 360f);
        isCorrect = (Mathf.Approximately(mod360, 0f) || Mathf.Approximately(mod360, 360f));

        if (isCorrect)
        {
            isLocked = true;
            if (puzzleManager)
                puzzleManager.OnRingSolved(this);
        }
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }
}
