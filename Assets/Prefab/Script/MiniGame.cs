using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGame : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform meter;       // Tall meter area
    public RectTransform marker;      // Moving bar
    public List<RectTransform> safeZones; // Green zones (children of meter)
    public Image progressFill;        // Image with Fill Method = Vertical
    public TMP_Text resultText;       // Optional

    [Header("Tuning")]
    public float markerSpeed = 400f;
    public float bouncePadding = 6f;
    public float fillGainPerSec = 0.35f;
    public float drainPerSec = 0.25f;
    public float winThreshold = 1.0f;
    public bool autoStart = true;

    float dir = 1f;
    float progress = 0f;
    bool playing = false;

    float minY, maxY;

    void Awake()
    {
        if (progressFill) progressFill.fillAmount = 0f;
        if (resultText) resultText.text = "";
    }

    void OnEnable()
    {
        if (autoStart) StartMinigame();
    }

    public void StartMinigame()
    {
        float half = meter.rect.height * 0.5f;
        minY = -half + bouncePadding;
        maxY = half - bouncePadding;

        var m = marker.anchoredPosition;
        m.y = Random.Range(minY, maxY);
        marker.anchoredPosition = m;

        dir = Random.value < 0.5f ? -1f : 1f;
        progress = 0f;
        if (progressFill) progressFill.fillAmount = 0f;
        if (resultText) resultText.text = "";
        playing = true;
    }

    void Update()
    {
        if (!playing) return;

        var pos = marker.anchoredPosition;
        pos.y += dir * markerSpeed * Time.unscaledDeltaTime;

        if (pos.y >= maxY) { pos.y = maxY; dir = -1f; }
        if (pos.y <= minY) { pos.y = minY; dir = 1f; }

        marker.anchoredPosition = pos;

        bool inZone = IsMarkerInAnyZone();
        bool engaging = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

        float delta = Time.unscaledDeltaTime;
        if (engaging && inZone) progress += fillGainPerSec * delta;
        else progress -= drainPerSec * delta;

        progress = Mathf.Clamp01(progress);
        if (progressFill) progressFill.fillAmount = progress;

        if (progress >= winThreshold) Win();
    }

    bool IsMarkerInAnyZone()
    {
        float markerY = marker.anchoredPosition.y;
        float halfH = marker.rect.height * 0.5f;

        foreach (var z in safeZones)
        {
            float zy = z.anchoredPosition.y;
            float zHalf = z.rect.height * 0.5f;

            bool overlap = (markerY + halfH) >= (zy - zHalf) && (markerY - halfH) <= (zy + zHalf);
            if (overlap) return true;
        }
        return false;
    }

    void Win()
    {
        playing = false;
        if (resultText) resultText.text = "Success!";
        // Optional callback
    }

    public void Fail()
    {
        playing = false;
        if (resultText) resultText.text = "Failed!";
    }
}
