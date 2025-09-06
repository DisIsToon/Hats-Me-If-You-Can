using UnityEngine;

public class Portal : MonoBehaviour
{
    public Transform targetPortal;   // The portal we want to teleport to
    public float cooldownTime = 1f;  // Prevents instant back-and-forth teleporting

    private bool canTeleport = true;

    private void OnTriggerEnter(Collider other)
    {
        if (canTeleport && other.CompareTag("Player"))
        {
            // Teleport player to target portal
            other.transform.position = targetPortal.position;

            // Optional: Match rotation
            other.transform.rotation = targetPortal.rotation;

            // Start cooldown on both portals
            StartCoroutine(TeleportCooldown());
            Portal targetScript = targetPortal.GetComponent<Portal>();
            if (targetScript != null)
                targetScript.StartCoroutine(targetScript.TeleportCooldown());
        }
    }

    private System.Collections.IEnumerator TeleportCooldown()
    {
        canTeleport = false;
        yield return new WaitForSeconds(cooldownTime);
        canTeleport = true;
    }
}