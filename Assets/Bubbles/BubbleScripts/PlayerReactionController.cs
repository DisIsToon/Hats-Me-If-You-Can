using UnityEngine;

public class PlayerReactionController : MonoBehaviour
{
    public enum ReactionType
    {
        None,
        Miss,
        Hit,
        Close,
        ForestBiome,
        SnowBiome
    }

    [Header("Display")]
    public ReactionDisplay reactionDisplay;

    [Header("Player Sprites")]
    public Sprite missedPotionReaction;
    public Sprite hitPotionReaction;
    public Sprite closeToAIReaction;

    [Header("Biome Sprites")]
    public Sprite forestBiomeReaction;
    public Sprite snowBiomeReaction;

    [Header("Durations")]
    public float missDuration = 1.2f;
    public float hitDuration = 1.2f;
    public float closeDuration = 1.0f;
    public float biomeDuration = 1.3f;

    [Header("Spam Control")]
    public float reactionCooldown = 0.15f;

    private ReactionType currentReaction = ReactionType.None;
    private float lastReactionTime = -999f;

    public void ShowMissReaction(bool force = true)
    {
        ShowReaction(ReactionType.Miss, missedPotionReaction, missDuration, force);
    }

    public void ShowHitReaction(bool force = true)
    {
        ShowReaction(ReactionType.Hit, hitPotionReaction, hitDuration, force);
    }

    public void ShowCloseReaction(bool force = false)
    {
        ShowReaction(ReactionType.Close, closeToAIReaction, closeDuration, force);
    }

    public void ShowForestBiomeReaction(bool force = true)
    {
        ShowReaction(ReactionType.ForestBiome, forestBiomeReaction, biomeDuration, force);
    }

    public void ShowSnowBiomeReaction(bool force = true)
    {
        ShowReaction(ReactionType.SnowBiome, snowBiomeReaction, biomeDuration, force);
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