using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WinterBarrierSystem : MonoBehaviour, IDataPersistence
{
    public static WinterBarrierSystem Instance { get; set; }

    [Header("Barrier Object")]
    public GameObject barrierObject;
    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;

    [Header("Message Object Settings")]
    public GameObject messageObject;      // Message when ShyHat not captured
    public GameObject messageObject2;     // Message when ShyHat captured
    public float fadeDuration = 1f;
    public float showTime = 2f;

    [Header("Mini Game")]
    public GameObject barrierGameScreen;
    public ClickerBarrier clickerGame;

    private bool isShowing = false;
    private GameTracker gt;

    public bool WBBarrierAlreadyOpened;

    // -------------------------------------------------------
    //  SINGLETON
    // -------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
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
    //  SAVE / LOAD
    // -------------------------------------------------------
    public void LoadData(GameData data)
    {
        this.WBBarrierAlreadyOpened = data.WBBarrierAlreadyOpened;

        Debug.Log("Loaded WBBarrierAlreadyOpened = " + data.WBBarrierAlreadyOpened);

        if (WBBarrierAlreadyOpened && barrierObject != null)
        {
            Debug.Log("DisableBarrier DisableBarrier DisableBarrier ");
            DisableBarrier();
        }
    }

    public void SaveData(GameData data)
    {
        data.WBBarrierAlreadyOpened = this.WBBarrierAlreadyOpened;
    }

    // -------------------------------------------------------
    //  START
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
    //  CALLED FROM THE BARRIER OBJECT COLLISION SCRIPT
    // -------------------------------------------------------
    public void PlayerHitBarrier()
    {
        if (isShowing) return;

        if (gt != null && gt.shyHatCaptured)
        {
            // Show special message then start minigame
            StartCoroutine(ShowAndStartGame(messageObject2));
        }
        else
        {
            // Show basic message only
            StartCoroutine(ShowAndFade(messageObject));
        }
    }

    // -------------------------------------------------------
    //  UI + MINIGAME HANDLING
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
                Color c = img.color; c.a = alpha; img.color = c;
            }
            else if (sr)
            {
                Color c = sr.color; c.a = alpha; sr.color = c;
            }

            yield return null;
        }

        msgObj.SetActive(false);
        isShowing = false;
    }

    private IEnumerator ShowAndStartGame(GameObject msgObj)
    {
        if (msgObj == null) yield break;

        isShowing = true;
        msgObj.SetActive(true);

        yield return new WaitForSeconds(showTime);

        msgObj.SetActive(false);
        isShowing = false;

        // Start the mini-game
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

    private IEnumerator WaitForVictory()
    {
        // Wait for ClickerBarrierTMP to finish
        while (!clickerGame.gameEnded)
        {
            yield return null;
        }

        // Victory handling
        if (clickerGame.victoryScreen.activeSelf)
        {
            WBBarrierAlreadyOpened = true;
            DisableBarrier();

            SoundManager.Instance.ReturnToBiomeMusic();
            NotifUIManager.Instance.NotifyBarrierComplete();
        }
    }

    private void DisableBarrier()
    {
        if (boxCollider) boxCollider.enabled = false;
        if (meshRenderer) meshRenderer.enabled = false;
    }
}
