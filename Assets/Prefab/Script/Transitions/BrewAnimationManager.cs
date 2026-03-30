using UnityEngine;
using System.Collections;

public class BrewAnimationManager : MonoBehaviour {
    public CanvasGroup canvasGroup;
    public Animator animator;
    public float duration = 3f;
    public float fadeDuration = 0.5f;

    private void OnEnable() {
        StartCoroutine(PlayBrew());
    }

    IEnumerator PlayBrew() {
        // Step 1: Reset visibility
        canvasGroup.alpha = 1f;

        // Step 2: Play animation
        animator.Play("brew");

        // Step 3: Wait for your chosen duration
        yield return new WaitForSeconds(duration);

        // Step 4: Fade out
        float t = 0;
        while (t < fadeDuration) {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;

        // Step 5: Disable object
        gameObject.SetActive(false);
    }
}