using System.Collections;
using UnityEngine;

public class PopupTrigger : MonoBehaviour
{
    [Header("Popup Screen")]
    public GameObject popupScreen;
    public float visibleDuration = 2f;
    public float fadeDuration = 1f;

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
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(visibleDuration);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        popupScreen.SetActive(false);
    }
}
