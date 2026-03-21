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

    [Header("3D Barrier Object")]
    public Renderer barrierRenderer; // <-- assign your 3D object's renderer

    [Header("UI Components")]
    public TMP_Text timerText;
    public GameObject victoryScreen;
    public GameObject gameOverScreen;
    public GameObject BarrierGameScreen;
    public GameObject MainScreen;
    public GameObject QuickSlotScreen;
    public GameObject InventoryBTN;

    [Header("Timer Settings")]
    public float timeLimit = 10f;
    public float timeRemaining;
    public bool gameEnded = false;

    [Header("Click Effect Settings")]
    public GameObject clickEffectPrefab;
    public Transform effectParent;
    public float fadeDuration = 0.8f;
    public Vector2 randomSpawnOffset = new Vector2(300f, 150f);
    public float fallSpeed = 100f;

    public void StartGame()
    {
        MainScreen.SetActive(false);
        InventoryBTN.SetActive(false);
        QuickSlotScreen.SetActive(false);

        barrierHP = maxBarrierHP;
        timeRemaining = timeLimit;
        gameEnded = false;

        UpdateObjectFade(); // fade object based on HP

        if (victoryScreen) victoryScreen.SetActive(false);
        if (gameOverScreen) gameOverScreen.SetActive(false);
        if (BarrierGameScreen) BarrierGameScreen.SetActive(true);
    }

    void Update()
    {
        if (gameEnded) return;

        timeRemaining -= Time.deltaTime;

        if (timerText != null)
            timerText.text = "Time: " + Mathf.Ceil(timeRemaining).ToString();

        if (timeRemaining <= 0)
            GameOver();
    }

    public void OnClickBarrier()
    {
        if (gameEnded) return;

        
        barrierHP -= damagePerClick;
        barrierHP = Mathf.Clamp(barrierHP, 0, maxBarrierHP);

        UpdateObjectFade();
        SpawnClickEffect();

        if (barrierHP <= 0)
            Victory();
    }

    // Fade 3D object (material alpha)
    void UpdateObjectFade()
    {
        if (barrierRenderer == null) return;

        float alpha = barrierHP / maxBarrierHP;

        // Loop through all materials (if the object has multiple)
        foreach (Material mat in barrierRenderer.materials)
        {
            if (mat.HasProperty("_Color"))
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
        }
    }

    public void SpawnClickEffect()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.buttonClickedSound.clip);
        if (clickEffectPrefab == null || effectParent == null) return;

        Vector2 randomOffset = new Vector2(
            Random.Range(-randomSpawnOffset.x, randomSpawnOffset.x),
            Random.Range(-randomSpawnOffset.y, randomSpawnOffset.y)
        );

        Vector2 spawnPos = (Vector2)Input.mousePosition + randomOffset;

        GameObject effect = Instantiate(clickEffectPrefab, effectParent);
        effect.transform.position = spawnPos;

        Image img = effect.GetComponent<Image>();
        if (img != null)
            StartCoroutine(FallAndFade(img));
    }

    IEnumerator FallAndFade(Image img)
    {
        Color startColor = img.color;
        float t = 0f;
        Vector3 startPos = img.transform.position;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = t / fadeDuration;

            float alpha = Mathf.Lerp(1f, 0f, normalized);
            img.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            img.transform.position = startPos + Vector3.down * (normalized * fallSpeed);

            yield return null;
        }

        Destroy(img.gameObject);
    }

    IEnumerator ShowVictoryPopup()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.pzzleCompleteSound.clip);
        yield return new WaitForSeconds(3f);
        victoryScreen.SetActive(false);
        BarrierGameScreen.SetActive(false);
    }

    IEnumerator ShowGameOverPopup()
    {
        yield return new WaitForSeconds(3f);
        gameOverScreen.SetActive(false);
        BarrierGameScreen.SetActive(false);

    }

    public void Victory()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.pzzleCompleteSound.clip);
        gameEnded = true;

        if (victoryScreen != null)
            victoryScreen.SetActive(true);

        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        MainScreen.SetActive(true);
        QuickSlotScreen.SetActive(true);
        InventoryBTN.SetActive(true);
        StartCoroutine(ShowVictoryPopup());
    }

    public void GameOver()
    {
        gameEnded = true;


        if (gameOverScreen != null)
        gameOverScreen.SetActive(true);

        if (victoryScreen != null)
            victoryScreen.SetActive(false);
        MainScreen.SetActive(true);
        QuickSlotScreen.SetActive(true);
        InventoryBTN.SetActive(true);
        StartCoroutine(ShowGameOverPopup());
    }
}
