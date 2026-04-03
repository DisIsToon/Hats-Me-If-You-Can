using UnityEngine;

public class PotionReactionExample : MonoBehaviour
{
    public PlayerReactionController throwerReaction;
    public float missDelay = 1.5f;

    private bool hitSomething;

    private void OnCollisionEnter(Collision collision)
    {
        if (hitSomething) return;

        AIReactionController ai = collision.collider.GetComponent<AIReactionController>();

        if (ai != null)
        {
            hitSomething = true;

            if (throwerReaction != null)
                throwerReaction.ShowHitReaction();

            ai.ShowGettingHitReaction();
        }
    }

    private void OnDestroy()
    {
        if (!hitSomething && throwerReaction != null)
        {
            throwerReaction.ShowMissReaction();
        }
    }
}