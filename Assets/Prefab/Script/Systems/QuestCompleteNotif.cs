using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestCompleteNotif : MonoBehaviour {
    public static QuestCompleteNotif Instance { get; set; }

    [Header("Main Notification")]
    public RectTransform questCompleteNotif;
    public CanvasGroup canvasGroup;

    [Header("Character Images")]
    public GameObject liraImage;
    public GameObject tulipImage;
    public GameObject mallowImage;

    [Header("Animation Settings")]
    public float fallDistance = 80f;
    public float fallDuration = 0.35f;
    public float fadeDuration = 0.35f;

    private Vector2 startPosition;
    private Coroutine currentRoutine;

    private void Awake() {
        Instance = this;

        if (canvasGroup == null)
            canvasGroup = questCompleteNotif.GetComponent<CanvasGroup>();

        startPosition = questCompleteNotif.anchoredPosition;
    }

    // ------------------------------
    // PUBLIC FUNCTIONS TO CALL
    // ------------------------------

    public void ShowLiraComplete() {
        ShowNotification(liraImage);
    }

    public void ShowTulipComplete() {
        ShowNotification(tulipImage);
    }

    public void ShowMallowComplete() {
        ShowNotification(mallowImage);
    }

    // ------------------------------
    // CORE NOTIFICATION LOGIC
    // ------------------------------

    private void ShowNotification(GameObject characterImage) {
        // Enable correct image, hide others
        liraImage.SetActive(characterImage == liraImage);
        tulipImage.SetActive(characterImage == tulipImage);
        mallowImage.SetActive(characterImage == mallowImage);

        // Reset visuals
        questCompleteNotif.gameObject.SetActive(true);
        questCompleteNotif.anchoredPosition = startPosition;
        canvasGroup.alpha = 0f; // start invisible for fade in

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation() {
        float time = 0;

        // ------------------------------
        // FADE IN
        // ------------------------------
        while (time < fadeDuration) {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            canvasGroup.alpha = t;
            yield return null;
        }

        canvasGroup.alpha = 1f;

        // ------------------------------
        // STAY (2 seconds)
        // ------------------------------
        yield return new WaitForSeconds(2f);

        time = 0;
        Vector2 endPos = startPosition + new Vector2(0, -fallDistance);

        // Falling + fading
        while (time < fallDuration) {
            time += Time.deltaTime;
            float t = time / fallDuration;

            // Move downward
            questCompleteNotif.anchoredPosition = Vector2.Lerp(startPosition, endPos, t);

            // Fade out simultaneously
            canvasGroup.alpha = 1f - t;

            yield return null;
        }

        // Final cleanup
        canvasGroup.alpha = 0f;
        questCompleteNotif.gameObject.SetActive(false);
    }
}