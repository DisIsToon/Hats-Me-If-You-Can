using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WinterBarrierSystem : MonoBehaviour
{
    [Header("Message Object Settings")]
    public GameObject messageObject;     // Optional UI/2D message
    public GameObject messageObject2;    // UI to show if ShyHat is captured
    public float fadeDuration = 1f;
    public float showTime = 2f;

    private bool isShowing = false;
    private Image uiImage;
    private SpriteRenderer spriteRenderer;

    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;

    private GameTracker gt;

    private void Start()
    {
        gt = FindObjectOfType<GameTracker>();
        if (gt == null)
        {
            Debug.LogError("GameTracker not found in the scene!");
        }

        boxCollider = GetComponent<BoxCollider>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (messageObject != null)
        {
            uiImage = messageObject.GetComponent<Image>();
            spriteRenderer = messageObject.GetComponent<SpriteRenderer>();
            messageObject.SetActive(false);
        }

        if (messageObject2 != null)
        {
            // Deactivate second message initially
            if (messageObject2.TryGetComponent(out Image img))
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
            if (messageObject2.TryGetComponent(out SpriteRenderer sr))
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
            messageObject2.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isShowing && gt != null)
        {
            if (gt.shyHatCaptured)
            {
                // Show the second message and disable barrier after fade
                StartCoroutine(ShowAndFade(messageObject2, true));
            }
            else
            {
                // Show normal message if ShyHat is not captured
                StartCoroutine(ShowAndFade(messageObject, false));
            }
        }
    }

    private IEnumerator ShowAndFade(GameObject msgObj, bool disableBarrierAfter)
    {
        if (msgObj == null) yield break;

        isShowing = true;
        msgObj.SetActive(true);

        // Set full alpha initially
        SetAlpha(msgObj, 1f);

        // Wait for showTime
        yield return new WaitForSeconds(showTime);

        // Fade out over fadeDuration
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            SetAlpha(msgObj, alpha);
            yield return null;
        }

        msgObj.SetActive(false);

        // Disable barrier components if required
        if (disableBarrierAfter)
        {
            if (boxCollider != null) boxCollider.enabled = false;
            if (meshRenderer != null) meshRenderer.enabled = false;
        }

        isShowing = false;
    }

    private void SetAlpha(GameObject msgObj, float alpha)
    {
        if (msgObj.TryGetComponent(out Image img))
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
        else if (msgObj.TryGetComponent(out SpriteRenderer sr))
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
