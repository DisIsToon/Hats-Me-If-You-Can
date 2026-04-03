using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerBiomeReactionTrigger : MonoBehaviour
{
    public enum BiomeReactionType
    {
        Forest,
        Snow
    }

    [Header("Biome Reaction")]
    public BiomeReactionType biomeReactionType;

    [Header("Options")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerReactionController playerReaction = other.GetComponent<PlayerReactionController>();
        if (playerReaction == null)
            return;

        switch (biomeReactionType)
        {
            case BiomeReactionType.Forest:
                playerReaction.ShowForestBiomeReaction(true);
                break;

            case BiomeReactionType.Snow:
                playerReaction.ShowSnowBiomeReaction(true);
                break;
        }

        hasTriggered = true;
    }
}