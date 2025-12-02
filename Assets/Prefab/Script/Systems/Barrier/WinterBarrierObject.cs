using UnityEngine;

public class WinterBarrierObject : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        WinterBarrierSystem.Instance.PlayerHitBarrier();
    }
}
