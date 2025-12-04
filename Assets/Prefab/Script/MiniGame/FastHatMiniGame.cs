using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FastHatMinigame : MonoBehaviour
{
    [Header("Bar / Markers")]
    public RectTransform barArea;
    public RectTransform movingMarker;
    public RectTransform targetMarker;

    [Header("Marker Movement")]
    [Tooltip("Starting speed of the moving marker.")]
    public float baseMarkerSpeed = 2f;

    [Tooltip("How much speed to add on each successful hit.")]
    public float speedIncreasePerHit = 0.5f;

    [Tooltip("Maximum allowed marker speed.")]
    public float maxMarkerSpeed = 10f;

    [Header("Difficulty")]
    [Tooltip("Successful hits needed to win.")]
    public int requiredHits = 6;

    [Tooltip("How many hits are lost on a miss.")]
    public int penaltyOnMiss = 1;

    [Tooltip("How many seconds are lost on a miss.")]
    public float missTimePenalty = 0.5f;

    [Header("Progress UI")]
    public Slider progressSlider;

    [Header("Timer")]
    public float totalTime = 10f;
    public TMP_Text timerText;

    [Header("Input")]
    public KeyCode hitKey = KeyCode.Space;

    [Header("Game Control")]
    [Tooltip("Player movement script to disable during minigame.")]
    public MonoBehaviour playerController;
    public bool pauseGameTime = true;

    [Header("Result Text")]
    public TMP_Text resultText;
    public string successMessage = "FAST HAT CAUGHT!";
    public string failMessage = "FAILED";
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public float resultDuration = 1f;

    [Header("Camera / Hat Trigger")]
    [Tooltip("Trigger that manages Cinemachine + hat visibility.")]
    public FastHatMinigameTrigger trigger;

    [Header("NPC To Show On Success (Chase)")]
    public GameObject npcChase;   // <- assign Chase here (set inactive in scene)

    // ---- internal state ----
    int currentHits = 0;
    float timeLeft;
    float markerTime = 0f;
    bool active = false;
    float previousTimeScale = 1f;
    float currentMarkerSpeed;

    void OnEnable()
    {
        Debug.Log("FastHatMinigame: OnEnable -> StartMinigame");
        StartMinigame();
    }

    public void StartMinigame()
    {
        active = true;
        currentHits = 0;
        timeLeft = totalTime;
        markerTime = 0f;
        currentMarkerSpeed = baseMarkerSpeed;

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

        RandomizeTargetPosition();
    }

    void Update()
    {
        if (!active) return;
        if (barArea == null || movingMarker == null || targetMarker == null) return;

        float dt = pauseGameTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // --- SPEEDING MARKER ---
        markerTime += dt * currentMarkerSpeed;
        float t = Mathf.PingPong(markerTime, 1f);
        float width = barArea.rect.width;

        Vector2 pos = movingMarker.anchoredPosition;
        pos.x = Mathf.Lerp(-width * 0.5f, width * 0.5f, t);
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
        Rect movingRect = GetWorldRect(movingMarker);
        Rect targetRect = GetWorldRect(targetMarker);

        // SUCCESS
        if (movingRect.Overlaps(targetRect, true))
        {
            currentHits++;

            if (progressSlider != null)
                progressSlider.value = currentHits;

            // Speed up
            currentMarkerSpeed = Mathf.Min(currentMarkerSpeed + speedIncreasePerHit, maxMarkerSpeed);

            RandomizeTargetPosition();

            if (currentHits >= requiredHits)
                EndMinigame(true);
        }
        else
        {
            // MISS penalties
            currentHits = Mathf.Max(0, currentHits - penaltyOnMiss);

            if (progressSlider != null)
                progressSlider.value = currentHits;

            timeLeft = Mathf.Max(0f, timeLeft - missTimePenalty);

            if (timerText != null)
                timerText.text = timeLeft.ToString("0.0");
        }
    }

    void RandomizeTargetPosition()
    {
        if (barArea == null || targetMarker == null) return;

        float barWidth = barArea.rect.width;
        float targetWidth = targetMarker.rect.width;

        float halfRange = (barWidth * 0.5f) - (targetWidth * 0.5f);

        Vector2 pos = targetMarker.anchoredPosition;
        pos.x = Random.Range(-halfRange, halfRange);
        targetMarker.anchoredPosition = pos;
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

        // ✅ If success, tell GameTracker we captured the Fast Hat
        if (success && GameTracker.Instance != null)
        {
            GameTracker.Instance.CaptureHat("FastHat");
            GameTracker.Instance.fastHatAlreadyCaptured = true;
        }

        // ✅ On success, show Chase
        if (success && npcChase != null)
        {
            npcChase.SetActive(true);
        }

        // Let trigger handle camera + hat
        if (trigger == null)
            trigger = FindObjectOfType<FastHatMinigameTrigger>();

        if (trigger != null)
        {
            Debug.Log("FastHatMinigame: Calling trigger.OnFastHatMinigameEnd(" + success + ")");
            trigger.OnFastHatMinigameEnd(success);
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
