using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AIProximityReaction : MonoBehaviour
{
    public AIReactionController aiReactionController;
    public PlayerReactionController playerReactionController;

    [Tooltip("If true, AI shows reaction when player gets close.")]
    public bool showAIReaction = true;

    [Tooltip("If true, player shows reaction when near AI.")]
    public bool showPlayerReaction = true;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (showAIReaction && aiReactionController != null)
            aiReactionController.ShowPlayerNearReaction();

        if (showPlayerReaction)
        {
            PlayerReactionController player = other.GetComponent<PlayerReactionController>();
            if (player != null)
                player.ShowCloseReaction();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (aiReactionController != null)
            aiReactionController.HideReaction();

        PlayerReactionController player = other.GetComponent<PlayerReactionController>();
        if (player != null)
            player.HideReaction();
    }
}