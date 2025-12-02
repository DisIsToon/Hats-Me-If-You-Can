using UnityEngine;

public class CRBarrierObject : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        CRBarrierSystem.Instance.PlayerHitBarrier();
    }
}
