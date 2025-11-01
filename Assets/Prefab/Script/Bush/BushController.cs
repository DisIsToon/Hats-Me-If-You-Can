using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BushController : MonoBehaviour
{
    private Animator animator;
    [SerializeField] float low = 0.5f;
    [SerializeField] float high = 1.2f;
    [SerializeField] bool shouldStartWithOffset = false;
    [SerializeField] Vector2 startOffset = new Vector2(0f, 1f);
    private bool isShaking = false;

    void Start() {
        animator = GetComponent<Animator>();
        animator.speed = Random.Range(low, high);

        if (shouldStartWithOffset)
            animator.Play(0, -1, Random.Range(startOffset.x, startOffset.y));
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !isShaking) {
            animator.SetTrigger("Shake"); // trigger the shake animation
            isShaking = true;
            StartCoroutine(ResetShake());
        }
    }

    IEnumerator ResetShake() {
        // wait a bit so it can shake again later
        yield return new WaitForSeconds(Random.Range(0.4f, 0.8f));
        isShaking = false;
    }
}
