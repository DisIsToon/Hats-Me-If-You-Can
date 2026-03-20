using UnityEngine;

public class TransitionController : MonoBehaviour {
    public static TransitionController Instance;

    public GameObject transitionCanvas; // Canvas that contains Animator
    public Animator transitionAnimator; // Animator on the canvas

    private void Awake() {
        Instance = this;
        transitionCanvas.SetActive(false); // hide by default
    }

    // Call this to start the transition
    public void PlayTransition() {
        transitionCanvas.SetActive(true);             // 1. show canvas
        transitionAnimator.Play("inTransition");      // 2. play animation
        StartCoroutine(HideAfterAnimation());         // 3. hide when done
    }

    private System.Collections.IEnumerator HideAfterAnimation() {
        yield return null; // wait one frame so animator updates
        float animLength = transitionAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength); // wait animation duration
        transitionCanvas.SetActive(false);           // hide canvas
    }
}