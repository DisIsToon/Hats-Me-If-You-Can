using UnityEngine;

public class LeafTrigger : MonoBehaviour
{
    [SerializeField] ParticleSystem leafParticles;
    [SerializeField] Animator treeAnimator;

    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.CompareTag("Player")) 
        {
            if (leafParticles != null)
                leafParticles.Play();

            if (treeAnimator != null)
                treeAnimator.SetTrigger("Shake");
        }
    }
}
