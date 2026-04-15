using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;

public class WinterBarrierSystem : MonoBehaviour
{
    public static WinterBarrierSystem Instance { get; private set; }

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

    [Header("Main Camera")]
    public Camera mainCamera; // only one real Unity camera

    [Header("Puzzle Cinemachine Camera")]
    public CinemachineCamera puzzleCamera;

    [Header("Mini Game")]
    public ClickerBarrier clickerGame;

    private bool isShowing = false;
    private GameTracker gt;

    public bool WBBarrierAlreadyOpened;

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

    public void LoadData(GameData2 data)
    {
        WBBarrierAlreadyOpened = data.WBBarrierAlreadyOpened;

        // NEW: check biome discovery
        if (data.castleRuinAlreadyDiscovered)
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

    private void Start()
    {
        gt = FindObjectOfType<GameTracker>();

        if (messageObject) messageObject.SetActive(false);
        if (messageObject2) messageObject2.SetActive(false);

        // keep puzzle camera inactive at start
        if (puzzleCamera)
            puzzleCamera.Priority = 0;

        if (GameDataManager2.Instance.currentData != null)
        {
            LoadData(GameDataManager2.Instance.currentData);
        }
    }

    public void PlayerHitBarrier()
    {
        if (isShowing) return;

        if (gt != null && gt.shyHatCaptured)
            StartCoroutine(ShowAndStartGame(messageObject2));
        else
            StartCoroutine(ShowAndFade(messageObject));
    }

    private IEnumerator ShowAndFade(GameObject msgObj)
    {
        if (msgObj == null) yield break;

        isShowing = true;
        msgObj.SetActive(true);

        animator.Play("captureHint", 0, 0f);

        yield return new WaitForSeconds(showTime);

        animator.SetTrigger("Out");

        yield return new WaitForSeconds(1f);

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

        SwitchToPuzzleCamera();

        if (clickerGame != null)
        {
            SoundManager.Instance.PlayPuzzleMusic();
            clickerGame.StartGame();

            StartCoroutine(WaitForVictory());
        }
    }

    private IEnumerator WaitForVictory()
    {
        while (!clickerGame.gameEnded)
            yield return null;

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
            SoundManager.Instance.ReturnToBiomeMusic();
        }
    }

    private void SwitchToPuzzleCamera()
    {
        if (puzzleCamera)
            puzzleCamera.Priority = 20;
    }

    private void SwitchToMainCamera()
    {
        if (puzzleCamera)
            puzzleCamera.Priority = 0;
    }

    public void DisableBarrier()
    {
        if (boxCollider) boxCollider.enabled = false;
        if (meshRenderer) meshRenderer.enabled = false;
    }
}