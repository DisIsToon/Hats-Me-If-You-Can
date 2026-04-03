using System.Collections;
using UnityEngine;

public class ReactionDisplay : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Transform followTarget;

    [Header("Follow")]
    public Vector3 offset = new Vector3(0f, 2f, 0f);
    public bool billboardToCamera = true;

    [Header("Timing")]
    public float defaultDuration = 1.5f;

    [Header("Animation")]
    public float popInDuration = 0.12f;
    public float popOutDuration = 0.12f;
    public float overshootScale = 1.15f;
    public Vector3 hiddenScale = Vector3.zero;
    public Vector3 shownScale = Vector3.one;

    private Camera mainCam;
    private Coroutine activeRoutine;
    private Vector3 baseScale;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        mainCam = Camera.main;
        baseScale = shownScale;

        if (transform.localScale != Vector3.zero)
            baseScale = transform.localScale;

        shownScale = baseScale;
        transform.localScale = hiddenScale;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }

    void LateUpdate()
    {
        if (followTarget != null)
            transform.position = followTarget.position + offset;

        if (billboardToCamera && mainCam != null)
        {
            Vector3 dir = transform.position - mainCam.transform.position;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    public void Show(Sprite sprite, float duration = -1f)
    {
        if (spriteRenderer == null || sprite == null)
            return;

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        spriteRenderer.sprite = sprite;
        spriteRenderer.enabled = true;

        float finalDuration = duration > 0f ? duration : defaultDuration;
        activeRoutine = StartCoroutine(ShowRoutine(finalDuration));
    }

    public void HideImmediate()
    {
        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        transform.localScale = hiddenScale;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        activeRoutine = null;
    }

    private IEnumerator ShowRoutine(float visibleDuration)
    {
        transform.localScale = hiddenScale;

        Vector3 overshoot = shownScale * overshootScale;

        float t = 0f;
        while (t < popInDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / popInDuration);
            transform.localScale = Vector3.Lerp(hiddenScale, overshoot, normalized);
            yield return null;
        }

        t = 0f;
        while (t < 0.06f)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / 0.06f);
            transform.localScale = Vector3.Lerp(overshoot, shownScale, normalized);
            yield return null;
        }

        yield return new WaitForSeconds(visibleDuration);

        t = 0f;
        Vector3 startScale = transform.localScale;
        while (t < popOutDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / popOutDuration);
            transform.localScale = Vector3.Lerp(startScale, hiddenScale, normalized);
            yield return null;
        }

        transform.localScale = hiddenScale;
        spriteRenderer.enabled = false;
        activeRoutine = null;
    }
}