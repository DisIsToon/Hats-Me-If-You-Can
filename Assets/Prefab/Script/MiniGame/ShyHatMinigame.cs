using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShyHatMinigame : MonoBehaviour
{
    [Header("Bar / Markers")]
    public RectTransform barArea;
    public RectTransform movingMarker;
    public RectTransform targetMarker;
    public float markerSpeed = 1.5f;

    [Header("Progress")]
    public Slider progressSlider;
    public int requiredHits = 3;

    [Header("Timer")]
    public float totalTime = 8f;
    public TMP_Text timerText;

    [Header("Input")]
    public KeyCode hitKey = KeyCode.Space;

    [Header("Game Control")]
    public MonoBehaviour playerController;
    public bool pauseGameTime = true;

    [Header("Result Text")]
    public TMP_Text resultText;
    public string successMessage = "SUCCESS!";
    public string failMessage = "FAILED";
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public float resultDuration = 1f;

    [Header("Shy Hat Barrier Script")]
    [Tooltip("Reference to the TestShyHatBarrier script that talks to GameTracker.")]
    public TestShyHatBarrier shyHatBarrier;   // << we will call this on success

    private int currentHits = 0;
    private float timeLeft;
    private float markerTime = 0f;
    private bool active = false;
    private float previousTimeScale = 1f;

    void OnEnable()
    {
        StartMinigame();
    }

    public void StartMinigame()
    {
        active = true;
        currentHits = 0;
        timeLeft = totalTime;
        markerTime = 0f;

        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = requiredHits;
            progressSlider.value = 0;
        }

        if (timerText != null)
            timerText.text = timeLeft.ToString("0.0");

        if (resultText != null)
            resultText.text = "";

        if (playerController != null)
            playerController.enabled = false;

        if (pauseGameTime)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    void Update()
    {
        if (!active) return;
        if (barArea == null || movingMarker == null || targetMarker == null) return;

        float dt = pauseGameTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // --- MOVE MARKER UP & DOWN ---
        markerTime += dt * markerSpeed;
        float t = Mathf.PingPong(markerTime, 1f);
        float height = barArea.rect.height;

        Vector2 pos = movingMarker.anchoredPosition;
        pos.y = Mathf.Lerp(-height * 0.5f, height * 0.5f, t);
        movingMarker.anchoredPosition = pos;

        // --- INPUT ---
        if (Input.GetKeyDown(hitKey))
            TryHit();

        // --- TIMER ---
        timeLeft -= dt;
        if (timeLeft < 0f) timeLeft = 0f;

        if (timerText != null)
            timerText.text = timeLeft.ToString("0.0");

        if (timeLeft <= 0f)
            EndMinigame(false);
    }

    void TryHit()
    {
        // Overlap check between moving marker and target marker
        Rect movingRect = GetWorldRect(movingMarker);
        Rect targetRect = GetWorldRect(targetMarker);

        if (movingRect.Overlaps(targetRect, true))
        {
            currentHits++;

            if (progressSlider != null)
                progressSlider.value = currentHits;

            if (currentHits >= requiredHits)
                EndMinigame(true);
        }
    }

    Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return new Rect(corners[0], corners[2] - corners[0]);
    }

    void EndMinigame(bool success)
    {
        if (!active) return;
        active = false;

        if (resultText != null)
        {
            resultText.text = success ? successMessage : failMessage;
            resultText.color = success ? successColor : failColor;
        }

        if (success)
        {
            Debug.Log("ShyHat Minigame SUCCESS");

            // ⭐ Call the external script that talks to GameTracker ⭐
            if (shyHatBarrier != null)
            {
                shyHatBarrier.CaptureShyHat();
            }
            else
            {
                Debug.LogWarning("ShyHatMinigame: shyHatBarrier is not assigned in inspector.");
            }
        }

        StartCoroutine(CloseAfterDelay());
    }

    IEnumerator CloseAfterDelay()
    {
        float elapsed = 0f;
        while (elapsed < resultDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (playerController != null)
            playerController.enabled = true;

        if (pauseGameTime)
            Time.timeScale = previousTimeScale;

        if (resultText != null)
            resultText.text = "";

        gameObject.SetActive(false);
    }
}
