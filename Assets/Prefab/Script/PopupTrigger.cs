using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupTrigger : MonoBehaviour
{
    [Header("Popup Screen")]
    public GameObject popupScreen;
    public float visibleDuration = 2f;
    public float fadeDuration = 1f;
    public Animator animator;

    private bool hasTriggered = false;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (popupScreen != null)
        {
            canvasGroup = popupScreen.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                Debug.LogError("CanvasGroup missing on popupScreen!");
            }

            popupScreen.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        hasTriggered = true;
        StartCoroutine(ShowAndFadePopup());
    }

    private IEnumerator ShowAndFadePopup()
    {
        popupScreen.SetActive(true);
        animator.Play("captureHint", 0, 0f);

        yield return new WaitForSeconds(visibleDuration);

        animator.SetTrigger("Out");

        yield return new WaitForSeconds(0.5f);

        popupScreen.SetActive(false);
    }
}
