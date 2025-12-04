using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WinterBarrierSystem : MonoBehaviour, IDataPersistence
{
    public static WinterBarrierSystem Instance { get; private set; }

    [Header("Barrier Object")]
    public GameObject barrierObject;
    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;

    [Header("Message Object Settings")]
    public GameObject messageObject;
    public GameObject messageObject2;
    public float fadeDuration = 1f;
    public float showTime = 2f;

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera puzzleCamera;

    [Header("Mini Game")]
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
    //  SAVE / LOAD
    // -------------------------------------------------------
    public void LoadData(GameData data)
    {
        WBBarrierAlreadyOpened = data.WBBarrierAlreadyOpened;

        if (WBBarrierAlreadyOpened)
            DisableBarrier();
    }

    public void SaveData(GameData data)
    {
        data.WBBarrierAlreadyOpened = WBBarrierAlreadyOpened;
    }

    // -------------------------------------------------------
    //  START
    // -------------------------------------------------------
    private void Start()
    {
        gt = FindObjectOfType<GameTracker>();

        if (messageObject) messageObject.SetActive(false);
        if (messageObject2) messageObject2.SetActive(false);

        // Make sure main camera is on
        if (mainCamera) mainCamera.gameObject.SetActive(true);
        if (puzzleCamera) puzzleCamera.gameObject.SetActive(false);
    }

    // -------------------------------------------------------
    //  WHEN PLAYER COLLIDES WITH BARRIER
    // -------------------------------------------------------
    public void PlayerHitBarrier()
    {
        if (isShowing) return;

        if (gt != null && gt.shyHatCaptured)
            StartCoroutine(ShowAndStartGame(messageObject2));
        else
            StartCoroutine(ShowAndFade(messageObject));
    }

    // -------------------------------------------------------
    //  FADE MESSAGE
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

    // -------------------------------------------------------
    //  SHOW MESSAGE, THEN START MINIGAME CAMERA VIEW
    // -------------------------------------------------------
    private IEnumerator ShowAndStartGame(GameObject msgObj)
    {
        if (msgObj == null) yield break;

        isShowing = true;
        msgObj.SetActive(true);

        yield return new WaitForSeconds(showTime);

        msgObj.SetActive(false);
        isShowing = false;

        // Switch cameras
        SwitchToPuzzleCamera();

        // Start the clicker minigame
        if (clickerGame != null)
        {
            SoundManager.Instance.PlayPuzzleMusic();
            clickerGame.StartGame();

            StartCoroutine(WaitForVictory());
        }
    }

    // -------------------------------------------------------
    //  WAIT FOR CLICKER GAME RESULT
    // -------------------------------------------------------
    private IEnumerator WaitForVictory()
    {
        while (!clickerGame.gameEnded)
            yield return null;

        // Return to main camera
        SwitchToMainCamera();

        if (clickerGame.victoryScreen.activeSelf)
        {
            WBBarrierAlreadyOpened = true;
            DisableBarrier();

            SoundManager.Instance.ReturnToBiomeMusic();
            NotifUIManager.Instance.NotifyBarrierComplete();
        }
        else
        {
            // If failed, still go back to normal
            SoundManager.Instance.ReturnToBiomeMusic();
        }
    }

    // -------------------------------------------------------
    //  CAMERA SWITCHING
    // -------------------------------------------------------
    private void SwitchToPuzzleCamera()
    {
        if (mainCamera) mainCamera.gameObject.SetActive(false);
        if (puzzleCamera) puzzleCamera.gameObject.SetActive(true);
    }

    private void SwitchToMainCamera()
    {
        if (puzzleCamera) puzzleCamera.gameObject.SetActive(false);
        if (mainCamera) mainCamera.gameObject.SetActive(true);
    }

    // -------------------------------------------------------
    //  DISABLE BARRIER OBJECT
    // -------------------------------------------------------
    private void DisableBarrier()
    {
        if (boxCollider) boxCollider.enabled = false;
        if (meshRenderer) meshRenderer.enabled = false;
    }
}
