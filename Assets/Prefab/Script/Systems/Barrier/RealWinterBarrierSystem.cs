using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class RealWinterBarrierSystem : MonoBehaviour
{
    public static RealWinterBarrierSystem Instance { get; private set; }

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

    [Header("Cameras")]
    public Camera mainCamera;
    public CinemachineCamera puzzleCamera;

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

    public void LoadData(GameData2 data)
    {
        WBBarrierAlreadyOpened = data.WBBarrierAlreadyOpened;

        // NEW: check biome discovery
        if (data.winterBiomeAlreadyDiscovered)
        {
            DisableBarrier();
        }

        // Existing logic (optional)
        if (WBBarrierAlreadyOpened)
        {
            DisableBarrier();
        }
    }

    public void SaveData(GameData2 data)
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

        // Keep puzzle Cinemachine camera lower priority at start
        if (puzzleCamera) puzzleCamera.Priority = 0;

        if (GameDataManager2.Instance.currentData != null)
        {
            LoadData(GameDataManager2.Instance.currentData);
        }
    }

    // -------------------------------------------------------
    //  WHEN PLAYER COLLIDES WITH BARRIER
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
    //  FADE MESSAGE
    // -------------------------------------------------------
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
        if (puzzleCamera) puzzleCamera.Priority = 20;
    }

    private void SwitchToMainCamera()
    {
        if (puzzleCamera) puzzleCamera.Priority = 0;
    }

    // -------------------------------------------------------
    //  DISABLE BARRIER OBJECT
    // -------------------------------------------------------
    public void DisableBarrier()
    {
        if (boxCollider) boxCollider.enabled = false;
        if (meshRenderer) meshRenderer.enabled = false;
    }
}