using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGame : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform meter;
    public RectTransform marker;
    public List<RectTransform> safeZones;
    public Image progressFill;
    public TMP_Text resultText;
    public TMP_Text timerText; // Optional text for showing time

    [Header("Tuning")]
    public float markerSpeed = 400f;
    public float bouncePadding = 6f;
    public float fillGainPerSec = 0.35f;
    public float drainPerSec = 0.25f;
    public float winThreshold = 1.0f;
    public bool autoStart = true;

    [Header("Timer Settings")]
    public float timeLimit = 10f; // total time before fail
    float timer = 0f;

    float dir = 1f;
    float progress = 0f;
    bool playing = false;
    bool frozen = false; // freeze progress after fill
    float minY, maxY;

    void Awake()
    {
        if (progressFill) progressFill.fillAmount = 0f;
        if (resultText) resultText.text = "";
        if (timerText) timerText.text = "";
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
        timer = timeLimit;
        frozen = false;

        if (progressFill) progressFill.fillAmount = 0f;
        if (resultText) resultText.text = "";
        if (timerText) timerText.text = Mathf.CeilToInt(timer).ToString("0");

        playing = true;
    }

    void Update()
    {
        if (!playing) return;

        // Timer countdown
        timer -= Time.unscaledDeltaTime;
        if (timerText) timerText.text = Mathf.CeilToInt(timer).ToString("0");

        if (timer <= 0f)
        {
            Fail();
            return;
        }

        // Marker motion
        var pos = marker.anchoredPosition;
        pos.y += dir * markerSpeed * Time.unscaledDeltaTime;
        if (pos.y >= maxY) { pos.y = maxY; dir = -1f; }
        if (pos.y <= minY) { pos.y = minY; dir = 1f; }
        marker.anchoredPosition = pos;

        if (frozen) return; // stop updating progress if frozen

        bool inZone = IsMarkerInAnyZone();
        bool engaging = Input.GetKey(KeyCode.Space);

        float delta = Time.unscaledDeltaTime;
        if (engaging && inZone) progress += fillGainPerSec * delta;
        else progress -= drainPerSec * delta;

        progress = Mathf.Clamp01(progress);
        if (progressFill) progressFill.fillAmount = progress;

        if (progress >= winThreshold)
        {
            frozen = true; // freeze progress
            Win();
        }
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
    }

    public void Fail()
    {
        playing = false;
        if (resultText) resultText.text = "Time's up!";
    }
}
