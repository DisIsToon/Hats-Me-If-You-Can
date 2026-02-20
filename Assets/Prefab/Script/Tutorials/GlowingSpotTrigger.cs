using UnityEngine;

public class GlowingSpotTrigger : MonoBehaviour
{
    public enum SpotType
    {
        Movement1,
        Movement2,
        Jump
    }

    public SpotType spotType;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        switch (spotType)
        {
            case SpotType.Movement1:
                SoundManager.Instance.PlaySFX(SoundManager.Instance.matchingPuzzleCardClicked.clip);
                TutorialManager.Instance.MovementSpot1Reached();
                break;

            case SpotType.Movement2:
                SoundManager.Instance.PlaySFX(SoundManager.Instance.matchingPuzzleCardClicked.clip);
                TutorialManager.Instance.MovementSpot2Reached();
                break;

            case SpotType.Jump:
                SoundManager.Instance.PlaySFX(SoundManager.Instance.matchingPuzzleCardClicked.clip);
                TutorialManager.Instance.JumpSpotReached();
                break;
        }
    }
}
