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
    public Animator animator;

    [Header("Settings")]
    public float showDuration = 1.5f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;
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

    private IEnumerator NotificationRoutine() {
        notifUIScreen.SetActive(true);

        // Play reveal animation (default state)
        animator.Play("notifReveal", 0, 0f);

        yield return new WaitForSeconds(showDuration);

        // Trigger exit animation
        animator.SetTrigger("Out");

        // Wait for animation to finish (adjust based on your animation length)
        yield return new WaitForSeconds(0.6f);

        notifUIScreen.SetActive(false);
    }
}
