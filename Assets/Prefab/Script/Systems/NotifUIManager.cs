using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotifUIManager : MonoBehaviour
{
    public static NotifUIManager Instance { get; set; }

    [Header("References")]
    public GameObject notifUIScreen;
    public TextMeshProUGUI notifText;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float showDuration = 0.6f;
    public float fadeDuration = 0.6f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
            canvasGroup = notifUIScreen.GetComponent<CanvasGroup>();
    }

    // ------------------------------
    // --- PUBLIC CALLABLE EVENTS ---
    // ------------------------------

    public void NotifyItemPicked(string itemName)
    {
        ShowNotification($"Picked up a {itemName}");
    }

    public void NotifyQuestAccepted(string questName)
    {
        ShowNotification($"{questName}: Quest Accepted");
    }

    public void NotifyBiomeDiscovered(string biomeName)
    {
        ShowNotification($"{biomeName}: Discovered");
    }

    public void NotifyHatCaptured(string hatName)
    {
        ShowNotification($"Captured {hatName}");
    }

    public void NotifyPotionBrewed(string potionName)
    {
        ShowNotification($"Brewed {potionName}");
    }

    public void NotifyPuzzleComplete ()
    {
        ShowNotification($"Puzzle Complete");
    }

    public void NotifyBarrierComplete()
    {
        ShowNotification($"Barrier Oppened");
    }

    public void NotifyMeetHeadMaster()
    {
        ShowNotification($"Meet Headmaster Eira near the cauldron");
    }


    public void NotifyCustom(string msg)
    {
        ShowNotification(msg);
    }

    // ------------------------------
    // --- CORE NOTIFICATION LOGIC ---
    // ------------------------------

    private void ShowNotification(string msg)
    {
        notifText.text = msg;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(NotificationRoutine());
    }

    private IEnumerator NotificationRoutine()
    {
        notifUIScreen.SetActive(true);
        canvasGroup.alpha = 1f;

        // Stay visible
        yield return new WaitForSeconds(showDuration);

        // Fade out
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        notifUIScreen.SetActive(false);
    }
}
