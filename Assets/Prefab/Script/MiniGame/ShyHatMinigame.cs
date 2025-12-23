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

    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject inventoryBTN;
    public GameObject quickSlots;

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
    public TestShyHatBarrier shyHatBarrier;

    [Header("Camera Focus (optional)")]
    public ShyHatMinigameTrigger trigger;   // will auto-find if null

    [Header("NPC To Show On Success (Ivy)")]
    public GameObject npcIvy;   // <- assign Ivy here (set inactive in scene)

    [Header("Panels To Hide During Minigame")]
    public GameObject panelToHide1;
    public GameObject panelToHide2;
    public GameObject panelToHide3;

    int currentHits = 0;
    float timeLeft;
    float markerTime = 0f;
    bool active = false;
    float previousTimeScale = 1f;

    void OnEnable()
    {
        // Safe auto-find if you forget to assign it
        if (trigger == null)
            trigger = FindObjectOfType<ShyHatMinigameTrigger>();

        Debug.Log("ShyHatMinigame: OnEnable -> StartMinigame");
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
    }

    void Update()
    {
        if (!active) return;
        if (barArea == null || movingMarker == null || targetMarker == null) return;

        float dt = pauseGameTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // --- MOVE MARKER LEFT & RIGHT ---
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
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.failedcatchSound.clip);
            EndMinigame(false);
        }

    }

    void TryHit()
    {
        Rect movingRect = GetWorldRect(movingMarker);
        Rect targetRect = GetWorldRect(targetMarker);
        SoundManager.Instance.PlaySFX(SoundManager.Instance.matchingPuzzleCardMaatch.clip);
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

        mainScreen.SetActive(true);
        quickSlots.SetActive(true);
        inventoryBTN.SetActive(true);

        if (!active) return;
        active = false;

        SoundManager.Instance.ReturnToBiomeMusic();

        if (resultText != null)
        {
            resultText.text = success ? successMessage : failMessage;
            resultText.color = success ? successColor : failColor;
        }

        // ✅ On success, notify GameTracker via barrier
        if (success && shyHatBarrier != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.shyHatCapturedSound.clip);
            shyHatBarrier.CaptureShyHat();
        }

        // ✅ On success, show Ivy
        if (success && npcIvy != null)
        {
            npcIvy.SetActive(true);
        }

        // ✅ Tell the trigger to restore camera & handle AI (success/fail)
        if (trigger == null)
            trigger = FindObjectOfType<ShyHatMinigameTrigger>();

        if (trigger != null)
        {
            Debug.Log("ShyHatMinigame: Calling trigger.OnShyHatMinigameEnd(" + success + ")");
            trigger.OnShyHatMinigameEnd(success);
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
