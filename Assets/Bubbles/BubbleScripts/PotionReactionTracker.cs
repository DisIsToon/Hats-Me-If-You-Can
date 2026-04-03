using UnityEngine;

public class PotionReactionTracker : MonoBehaviour
{
    [Header("Reaction")]
    public PlayerReactionController playerReactionController;

    [Header("Settings")]
    public bool destroyShowsMiss = true;

    private bool validHitTriggered = false;
    private bool missAlreadyShown = false;

    void Start()
    {
        if (playerReactionController == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerReactionController = playerObj.GetComponent<PlayerReactionController>();
        }
    }

    public void MarkValidHit()
    {
        validHitTriggered = true;
    }

    public void ShowMissReaction()
    {
        if (missAlreadyShown) return;

        missAlreadyShown = true;

        if (playerReactionController != null)
            playerReactionController.ShowMissReaction(true);
    }

    private void OnDestroy()
    {
        if (!destroyShowsMiss) return;
        if (validHitTriggered) return;

        ShowMissReaction();
    }
}