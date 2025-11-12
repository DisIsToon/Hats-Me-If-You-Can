using UnityEngine;
using UnityEngine.UI;
using TMPro;    
using System.Collections;

public class ClickerBarrier : MonoBehaviour
{
    [Header("Barrier Settings")]
    public float maxBarrierHP = 10f;
    public float barrierHP;
    public float damagePerClick = 1f;

    [Header("UI Components")]
    public Image barrierImage;
    public TMP_Text timerText;
    public GameObject victoryScreen;
    public GameObject gameOverScreen;
    public GameObject BarrierGameScreen;

    [Header("Timer Settings")]
    public float timeLimit = 10f;
    public float timeRemaining;
    public bool gameEnded = false;

    [Header("Click Effect Settings")]
    public GameObject clickEffectPrefab; // prefab with Image component
    public Transform effectParent;       // typically your Canvas
    public float fadeDuration = 0.8f;
    public Vector2 randomSpawnOffset = new Vector2(300f, 150f); // screen space spread
    public float fallSpeed = 100f; // how fast the effect falls

    public void StartGame()
    {
        barrierHP = maxBarrierHP;
        timeRemaining = timeLimit;
        UpdateOpacity();
        gameEnded = false;


        if (victoryScreen) victoryScreen.SetActive(false);
        if (gameOverScreen) gameOverScreen.SetActive(false);
        if (BarrierGameScreen) BarrierGameScreen.SetActive(true);

        Debug.Log("ClickerBarrier game started!");
    }

    public void Update()
    {
        if (gameEnded) return;

        // countdown timer
        timeRemaining -= Time.deltaTime;
        if (timerText != null)
            timerText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();

        if (timeRemaining <= 0)
        {
            GameOver();
        }
    }

    // Called by Event Trigger → PointerClick
    public void OnClickBarrier()
    {
        if (gameEnded) return;

        barrierHP -= damagePerClick;
        barrierHP = Mathf.Clamp(barrierHP, 0, maxBarrierHP);
        UpdateOpacity();

        SpawnClickEffect();

        if (barrierHP <= 0)
        {
            Victory();
        }
    }

    public void UpdateOpacity()
    {
        if (barrierImage != null)
        {
            Color c = barrierImage.color;
            c.a = barrierHP / maxBarrierHP;
            barrierImage.color = c;
        }
    }

    public void SpawnClickEffect()
    {
        if (clickEffectPrefab == null || effectParent == null) return;

        // Random screen position
        Vector2 randomOffset = new Vector2(
            Random.Range(-randomSpawnOffset.x, randomSpawnOffset.x),
            Random.Range(-randomSpawnOffset.y, randomSpawnOffset.y)
        );

        Vector2 spawnPos = (Vector2)Input.mousePosition + randomOffset;

        // Instantiate UI effect
        GameObject effect = Instantiate(clickEffectPrefab, effectParent);
        effect.transform.position = spawnPos;

        Image img = effect.GetComponent<Image>();
        if (img != null)
            StartCoroutine(FallAndFade(img));
    }

    System.Collections.IEnumerator FallAndFade(Image img)
    {
        Color startColor = img.color;
        float t = 0f;
        Vector3 startPos = img.transform.position;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = t / fadeDuration;

            // Fade out
            float alpha = Mathf.Lerp(1f, 0f, normalized);
            img.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            // Fall down
            img.transform.position = startPos + Vector3.down * (normalized * fallSpeed);

            yield return null;
        }

        Destroy(img.gameObject);
    }

    IEnumerator ShowVictoryPopup()
    {
        yield return new WaitForSeconds(1f);
        victoryScreen.SetActive(false);
        BarrierGameScreen.SetActive(false);
    }

    IEnumerator ShowGameOverPopup()
    {
        yield return new WaitForSeconds(1f);
        gameOverScreen.SetActive(false);
        BarrierGameScreen.SetActive(false);
    }

    public void Victory()
    {
        gameEnded = true;

        if (victoryScreen != null)
            victoryScreen.SetActive(true);

        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        StartCoroutine(ShowVictoryPopup());
    }

    public void GameOver()
    {
        gameEnded = true;

        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);

        if (victoryScreen != null)
            victoryScreen.SetActive(false);

        StartCoroutine(ShowGameOverPopup());
    }
}
