using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CRBarrierSystem : MonoBehaviour, IDataPersistence
{
    public static CRBarrierSystem Instance { get; private set; }

    [Header("Barrier Object")]
    public GameObject barrierObject;
    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;

    [Header("Message Object Settings")]
    public GameObject messageObject;
    public GameObject messageObject2;
    public Animator animator;
    public float fadeDuration = 1f;
    public float showTime = 2f;

    [Header("Mini Game")]
    public GameObject barrierGameScreen;
    public ClickerBarrier clickerGame;

    [Header("Cameras & Fade")]
    public Camera mainCamera;
    public Camera puzzleCamera;
    public Image fadeImage;

    private bool isShowing = false;
    private GameTracker gt;

    public bool CRBarrierAlreadyOpened;

    // -------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (barrierObject != null)
        {
            boxCollider = barrierObject.GetComponent<BoxCollider>();
            meshRenderer = barrierObject.GetComponent<MeshRenderer>();
        }
    }

    // -------------------------------------------------------
    public void LoadData(GameData data)
    {
        CRBarrierAlreadyOpened = data.CRBarrierAlreadyOpened;

        if (CRBarrierAlreadyOpened)
            DisableBarrier();
    }

    public void SaveData(GameData data)
    {
        data.CRBarrierAlreadyOpened = CRBarrierAlreadyOpened;
    }

    // -------------------------------------------------------
    private void Start()
    {
        gt = FindObjectOfType<GameTracker>();

        if (barrierObject != null)
        {
            boxCollider = barrierObject.GetComponent<BoxCollider>();
            meshRenderer = barrierObject.GetComponent<MeshRenderer>();
        }

        if (messageObject) messageObject.SetActive(false);
        if (messageObject2) messageObject2.SetActive(false);
        if (barrierGameScreen) barrierGameScreen.SetActive(false);
        if (puzzleCamera) puzzleCamera.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    public void PlayerHitBarrier()
    {
        if (isShowing) return;

        if (gt != null && gt.IsPuzzleComplete())
            StartCoroutine(ShowAndStartGame(messageObject2));
        else
            StartCoroutine(ShowAndFade(messageObject));
    }

    private IEnumerator ShowAndFade(GameObject msgObj)
    {
        if (msgObj == null) yield break;

        isShowing = true;
        msgObj.SetActive(true);

        // Play entry animation
        animator.Play("captureHint", 0, 0f);

        yield return new WaitForSeconds(showTime);

        // Play exit animation
        animator.SetTrigger("Out");

        yield return new WaitForSeconds(1f); // match animation length

        msgObj.SetActive(false);
        isShowing = false;
    }

    private IEnumerator ShowAndStartGame(GameObject msgObj)
    {
        if (msgObj == null) yield break;

        isShowing = true;
        msgObj.SetActive(true);

        // Entry animation
        animator.Play("captureHint", 0, 0f);

        yield return new WaitForSeconds(showTime);

        // Exit animation
        animator.ResetTrigger("Out");
        animator.SetTrigger("Out");

        yield return new WaitForSeconds(1f);

        msgObj.SetActive(false);
        isShowing = false;

        // Fade out -> switch camera -> fade in
        yield return StartCoroutine(Fade(1f));

        mainCamera.gameObject.SetActive(false);
        puzzleCamera.gameObject.SetActive(true);

        if (barrierGameScreen != null)
            barrierGameScreen.SetActive(true);

        if (clickerGame != null)
        {
            clickerGame.BarrierGameScreen = barrierGameScreen;
            SoundManager.Instance.PlayPuzzleMusic();
            clickerGame.StartGame();
            StartCoroutine(WaitForVictory());
        }

        yield return StartCoroutine(Fade(0f));
    }

    private IEnumerator WaitForVictory()
    {
        while (!clickerGame.gameEnded)
            yield return null;

        if (clickerGame.victoryScreen.activeSelf)
        {
            CRBarrierAlreadyOpened = true;
            SoundManager.Instance.ReturnToBiomeMusic();
            NotifUIManager.Instance.NotifyBarrierComplete();

            // Fade out -> return to main camera -> fade in
            yield return StartCoroutine(Fade(1f));

            barrierGameScreen.SetActive(false);
            puzzleCamera.gameObject.SetActive(false);
            mainCamera.gameObject.SetActive(true);

            yield return StartCoroutine(Fade(0f));

            DisableBarrier();
        }
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
    }

    private void DisableBarrier()
    {
        if (boxCollider) boxCollider.enabled = false;
        if (meshRenderer) meshRenderer.enabled = false;
    }
}
