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
    public float fadeDuration = 1f;
    public float showTime = 2f;

    [Header("Mini Game")]
    public GameObject barrierGameScreen;
    public ClickerBarrier clickerGame;

    private bool isShowing = false;
    private GameTracker gt;

    public bool CRBarrierAlreadyOpened;

    // -------------------------------------------------------
    // Singleton
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
    // Save / Load
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
    // Initialization
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
    }

    // -------------------------------------------------------
    // To be called by the barrier object's collision forwarder
    // -------------------------------------------------------
    public void PlayerHitBarrier()
    {
        if (isShowing) return;

        if (gt != null && gt.IsPuzzleComplete())
            StartCoroutine(ShowAndStartGame(messageObject2));
        else
            StartCoroutine(ShowAndFade(messageObject));
    }

    // -------------------------------------------------------
    // Fade Message
    // -------------------------------------------------------
    private IEnumerator ShowAndFade(GameObject msgObj)
    {
        if (msgObj == null) yield break;

        isShowing = true;
        msgObj.SetActive(true);

        yield return new WaitForSeconds(showTime);

        float elapsed = 0f;
        Image img = msgObj.GetComponent<Image>();
        SpriteRenderer sr = msgObj.GetComponent<SpriteRenderer>();

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            if (img)
            {
                var c = img.color; c.a = alpha; img.color = c;
            }
            else if (sr)
            {
                var c = sr.color; c.a = alpha; sr.color = c;
            }

            yield return null;
        }

        msgObj.SetActive(false);
        isShowing = false;
    }

    // -------------------------------------------------------
    // Start Minigame
    // -------------------------------------------------------
    private IEnumerator ShowAndStartGame(GameObject msgObj)
    {
        if (msgObj == null) yield break;

        isShowing = true;
        msgObj.SetActive(true);

        yield return new WaitForSeconds(showTime);

        msgObj.SetActive(false);
        isShowing = false;

        // Start the minigame
        if (barrierGameScreen != null)
            barrierGameScreen.SetActive(true);

        if (clickerGame != null)
        {
            clickerGame.BarrierGameScreen = barrierGameScreen;
            SoundManager.Instance.PlayPuzzleMusic();

            clickerGame.StartGame();
            StartCoroutine(WaitForVictory());
        }
    }

    // -------------------------------------------------------
    // Victory Handler
    // -------------------------------------------------------
    private IEnumerator WaitForVictory()
    {
        while (!clickerGame.gameEnded)
            yield return null;

        if (clickerGame.victoryScreen.activeSelf)
        {
            CRBarrierAlreadyOpened = true;

            SoundManager.Instance.ReturnToBiomeMusic();
            NotifUIManager.Instance.NotifyBarrierComplete();

            DisableBarrier();
        }
    }

    // -------------------------------------------------------
    // Disable Barrier
    // -------------------------------------------------------
    private void DisableBarrier()
    {
        if (boxCollider) boxCollider.enabled = false;
        if (meshRenderer) meshRenderer.enabled = false;
    }
}
