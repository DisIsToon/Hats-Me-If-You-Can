using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShyHatMinigame : MonoBehaviour
{
    [Header("Arc / Wand")]
    [Tooltip("RectTransform of the bunny arc image (parent of wand & target).")]
    public RectTransform arcRect;

    [Tooltip("Wand image that should move along the blue arc.")]
    public RectTransform wandRect;

    [Tooltip("Target marker sitting somewhere on the same arc.")]
    public RectTransform targetRect;

    [Tooltip("If > 0, use this radius. If 0, radius is taken from wand's starting position.")]
    public float radiusOverride = 0f;

    [Tooltip("How wide the arc sweep is, in degrees, around the top (e.g. 80–110).")]
    public float maxArcAngle = 90f;   // this is +/- from the top

    [Tooltip("How fast the wand sweeps back and forth.")]
    public float sweepSpeed = 1.5f;

    [Tooltip("Allowed angle difference for a hit (in degrees).")]
    public float hitTolerance = 8f;

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
    public TestShyHatBarrier shyHatBarrier;

    // Internal state
    int currentHits = 0;
    float timeLeft;
    bool active = false;
    float previousTimeScale = 1f;

    float sweepTime = 0f;        // 0..1 ping-pong driver
    float wandArcAngle = 0f;     // -maxArcAngle..+maxArcAngle (relative around the top)
    float targetArcAngle = 0f;   // fixed angle for the target (same convention)

    float radius;                // actual radius used
    Vector2 centerLocal = Vector2.zero;  // center of the arc in local space

    void OnEnable()
    {
        StartMinigame();
    }

    public void StartMinigame()
    {
        if (arcRect == null || wandRect == null || targetRect == null)
        {
            Debug.LogError("ShyHatMinigame: Please assign arcRect, wandRect, and targetRect.");
            gameObject.SetActive(false);
            return;
        }

        active = true;
        currentHits = 0;
        timeLeft = totalTime;
        sweepTime = 0f;

        // center is (0,0) in local space if wand & target are children of arcRect
        centerLocal = Vector2.zero;

        // determine radius
        if (radiusOverride > 0f)
        {
            radius = radiusOverride;
        }
        else
        {
            // use the starting distance of the wand from the center
            radius = wandRect.localPosition.magnitude;
        }

        // read target's angle based on its local position on the arc
        Vector2 tLocal = targetRect.localPosition;
        float targetWorldAngle = Mathf.Atan2(tLocal.y, tLocal.x) * Mathf.Rad2Deg; // 0° = right, 90° = up
        // convert to "arc angle around the top": 0° at top, negative to left, positive to right
        targetArcAngle = Mathf.DeltaAngle(90f, targetWorldAngle);

        // init UI stuff
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = requiredHits;
            progressSlider.value = 0;
        }

        if (timerText != null)
            timerText.text = totalTime.ToString("0.0");

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

        float dt = pauseGameTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // --- MOVE WAND ALONG THE ARC (position on a circle) ---
        sweepTime += dt * sweepSpeed;
        float t = Mathf.PingPong(sweepTime, 1f);  // 0..1
        wandArcAngle = Mathf.Lerp(-maxArcAngle, maxArcAngle, t); // -max..+max around the top

        // worldAngle: 90° is straight up from center, plus wandArcAngle to sweep left/right
        float worldAngle = 90f + wandArcAngle;
        float rad = worldAngle * Mathf.Rad2Deg * Mathf.Deg2Rad; // (mistake fix) but easier:
        // correction: we shouldn't double convert; let's just recalc:
        rad = worldAngle * Mathf.Deg2Rad;

        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        wandRect.localPosition = centerLocal + offset;

        // Optional: rotate wand so it points towards center or along the arc
        // Here we point it towards the center:
        Vector2 toCenter = (centerLocal - offset).normalized;
        float rotAngle = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;
        // adjust -90 so the wand's "up" or "top" points correctly depending on sprite
        wandRect.localEulerAngles = new Vector3(0f, 0f, rotAngle - 90f);

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
        // difference between where wand is on the arc vs target's arc angle
        float diff = Mathf.DeltaAngle(wandArcAngle, targetArcAngle);

        if (Mathf.Abs(diff) <= hitTolerance)
        {
            currentHits++;

            if (progressSlider != null)
                progressSlider.value = currentHits;

            if (currentHits >= requiredHits)
                EndMinigame(true);
        }
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
