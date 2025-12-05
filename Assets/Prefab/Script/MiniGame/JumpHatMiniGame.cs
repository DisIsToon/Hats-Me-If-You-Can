using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class JumpHatMinigame : MonoBehaviour
{
    [Header("Bar / Markers")]
    public RectTransform barArea;
    public RectTransform movingMarker;
    public RectTransform targetMarker;

    [Header("Marker Movement")]
    [Tooltip("Constant speed of the moving marker.")]
    public float markerSpeed = 2f;

    [Header("Difficulty")]
    [Tooltip("Successful hits needed to win.")]
    public int requiredHits = 5;

    [Tooltip("How many hits are lost on a miss.")]
    public int penaltyOnMiss = 1;

    [Tooltip("How many seconds are lost on a miss.")]
    public float missTimePenalty = 0.5f;
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject inventoryBTN;
    public GameObject quickSlots;

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
    public string successMessage = "JUMP HAT CLEARED!";
    public string failMessage = "FAILED";
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public float resultDuration = 1f;

    [Header("Barrier / GameTracker Hook")]
    [Tooltip("Barrier or object that should handle JumpHat capture (TestJumpHatBarrier).")]
    public TestJumpHatBarrier jumpHatBarrier;

    [Header("Camera Focus (optional)")]
    [Tooltip("Trigger that manages Cinemachine camera + hat disappearing.")]
    public JumpHatMinigameTrigger trigger;

    [Header("NPC To Show On Success (Lou)")]
    public GameObject npcLou;   // <- assign Lou here (set inactive in scene)

    [Header("Panels To Hide During Minigame")]
    public GameObject panelToHide1;
    public GameObject panelToHide2;
    public GameObject panelToHide3;

    // ----- internal state -----
    int currentHits = 0;
    float timeLeft;
    float markerTime = 0f;
    bool active = false;
    float previousTimeScale = 1f;

    void OnEnable()
    {
        Debug.Log("JumpHatMinigame: OnEnable -> StartMinigame");
        StartMinigame();
    }

    public void StartMinigame()
    {
        mainScreen.SetActive(false);
        quickSlots.SetActive(false);
        inventoryBTN.SetActive(false);

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

        // Hide panels when minigame starts
        if (panelToHide1 != null) panelToHide1.SetActive(false);
        if (panelToHide2 != null) panelToHide2.SetActive(false);
        if (panelToHide3 != null) panelToHide3.SetActive(false);

        RandomizeTargetPosition();
    }

    void Update()
    {
        if (!active) return;
        if (barArea == null || movingMarker == null || targetMarker == null) return;

        float dt = pauseGameTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // --- CONSTANT SPEED MARKER MOVEMENT ---
        markerTime += dt * markerSpeed;
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
        mainScreen.SetActive(true);
        quickSlots.SetActive(true);
        inventoryBTN.SetActive(true);


        if (!active) return;
        active = false;

        if (resultText != null)
        {
            resultText.text = success ? successMessage : failMessage;
            resultText.color = success ? successColor : failColor;
        }

        // ⭐ If minigame succeeded, notify GameTracker via barrier
        if (success && jumpHatBarrier != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.pzzleCompleteSound);
            jumpHatBarrier.CaptureJumpHat();
        }

        // ⭐ On success, show Lou
        if (success && npcLou != null)
        {
            npcLou.SetActive(true);
        }

        // ⭐ Tell the trigger to restore camera & handle hat based on success/fail
        if (trigger == null)
            trigger = FindObjectOfType<JumpHatMinigameTrigger>();

        if (trigger != null)
        {
            Debug.Log("JumpHatMinigame: Calling trigger.OnJumpHatMinigameEnd(" + success + ")");
            trigger.OnJumpHatMinigameEnd(success);
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

        // Show panels again when minigame finishes
        if (panelToHide1 != null) panelToHide1.SetActive(true);
        if (panelToHide2 != null) panelToHide2.SetActive(true);
        if (panelToHide3 != null) panelToHide3.SetActive(true);

        gameObject.SetActive(false);
    }
}
