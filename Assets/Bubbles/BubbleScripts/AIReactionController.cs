using UnityEngine;

public class AIReactionController : MonoBehaviour
{
    public enum ReactionType
    {
        None,
        Idle,
        Hit,
        Different,
        Near
    }

    [Header("Display")]
    public ReactionDisplay reactionDisplay;

    [Header("Sprites")]
    public Sprite idleReaction;
    public Sprite gettingHitReaction;
    public Sprite differentReaction;
    public Sprite playerNearReaction;

    [Header("Durations")]
    public float idleDuration = 1.2f;
    public float hitDuration = 1.2f;
    public float differentDuration = 1.0f;
    public float nearDuration = 1.0f;

    [Header("Spam Control")]
    public float reactionCooldown = 0.2f;

    private ReactionType currentReaction = ReactionType.None;
    private float lastReactionTime = -999f;

    public void ShowIdleReaction(bool force = false)
    {
        ShowReaction(ReactionType.Idle, idleReaction, idleDuration, force);
    }

    public void ShowGettingHitReaction(bool force = true)
    {
        ShowReaction(ReactionType.Hit, gettingHitReaction, hitDuration, force);
    }

    public void ShowDifferentReaction(bool force = false)
    {
        ShowReaction(ReactionType.Different, differentReaction, differentDuration, force);
    }

    public void ShowPlayerNearReaction(bool force = false)
    {
        ShowReaction(ReactionType.Near, playerNearReaction, nearDuration, force);
    }

    public void ClearReactionState()
    {
        currentReaction = ReactionType.None;
    }

    public void HideReaction()
    {
        if (reactionDisplay != null)
            reactionDisplay.HideImmediate();

        currentReaction = ReactionType.None;
    }

    private void ShowReaction(ReactionType type, Sprite sprite, float duration, bool force)
    {
        if (reactionDisplay == null || sprite == null)
            return;

        if (!force)
        {
            if (currentReaction == type)
                return;

            if (Time.time - lastReactionTime < reactionCooldown)
                return;
        }

        currentReaction = type;
        lastReactionTime = Time.time;
        reactionDisplay.Show(sprite, duration);
    }
}