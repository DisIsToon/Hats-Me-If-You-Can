using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ForestZoneMusicTrigger : MonoBehaviour
{
    [Header("Biome Name (must match SoundManager)")]
    public string biomeName = "Forest";

    [Header("Player Settings")]
    public string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (!other.CompareTag(playerTag))
            return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SwitchBiomeMusic(biomeName);
        }

        hasTriggered = true;

        // Disable trigger completely to prevent any future calls
        GetComponent<Collider>().enabled = false;
    }
}