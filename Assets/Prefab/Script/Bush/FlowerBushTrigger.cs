using UnityEngine;

public class FlowerBushTrigger : MonoBehaviour
{
    [SerializeField] ParticleSystem leafParticles;
    [SerializeField] Animator treeAnimator;
    [SerializeField] Color leafColor = Color.white; // white

    private void Start() {
        if (leafParticles != null) {
            var main = leafParticles.main;
            main.startColor = leafColor;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Player")) {
            if (leafParticles != null)
                leafParticles.Play();

            if (treeAnimator != null)
                treeAnimator.SetTrigger("Shake");
        }
    }
}
