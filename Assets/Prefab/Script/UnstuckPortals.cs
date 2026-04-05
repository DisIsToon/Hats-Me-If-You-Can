using UnityEngine;
using System.Collections;

public class PortalTeleport : MonoBehaviour
{
    public string playerTag = "Player";
    public Transform destinationPoint;
    public float teleportCooldown = 1f;

    private bool canTeleport = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!canTeleport) return;
        if (!other.CompareTag(playerTag)) return;
        if (destinationPoint == null)
        {
            Debug.LogWarning("Destination Point is not assigned.");
            return;
        }

        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        other.transform.position = destinationPoint.position;
        other.transform.rotation = destinationPoint.rotation;

        if (cc != null) cc.enabled = true;

        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        canTeleport = false;
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }
}