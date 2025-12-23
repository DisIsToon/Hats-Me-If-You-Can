using UnityEngine;
using System.Collections;
using TMPro;

public class PuzzleManagerUI : MonoBehaviour
{
    public static PuzzleManagerUI Instance { get; set; }

    [Header("Cameras")]
    public Camera mainCamera;
    public Camera puzzleCamera;

    [Header("Puzzle Object")]
    public GameObject puzzleObject;   // assign your puzzle's GameObject here

    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject inventoryBTN;
    public GameObject quickSlots;

    [Header("Puzzle Parts")]
    public GameObject rotatingPuzzleScreen;
    public GameObject solvedPopup;
    public GameObject completePuzzlePopup;
    public bool isOpen;
    private bool canPlayPuzzle = false;
    public RotatingRingUI[] rings;
    public bool puzzleComplete;

    [Header("Cotton")]
    public GameObject cotton;
    public GameObject cottonPrefab;
    public GameObject position;

    [Header("Fade Setting")]
    public CanvasGroup fadePanel;
    public float fadeDuration = 0.5f;

    [Header("Puzzle Timer")]
    public float puzzleTimeLimit = 20f;
    private Coroutine timerRoutine;

    [Header("Timer UI")]
    public TMP_Text timerText;     // ⏳ TMP text to show time

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        rotatingPuzzleScreen.SetActive(false);
        isOpen = false;
        puzzleCamera.enabled = false;

        fadePanel.alpha = 0;

        if (timerText != null)
            timerText.text = "";
    }

    IEnumerator Fade(float start, float end)
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(start, end, t / fadeDuration);
            fadePanel.alpha = a;
            yield return null;
        }
        fadePanel.alpha = end;
    }

    IEnumerator OpenPuzzleSequence()
    {
        SoundManager.Instance.PlayPuzzleMusic();

        isOpen = true;

        yield return StartCoroutine(Fade(0, 1));

        mainCamera.enabled = false;
        puzzleCamera.enabled = true;

        yield return StartCoroutine(Fade(1, 0));

        rotatingPuzzleScreen.SetActive(true);

        // Start puzzle timer
        timerRoutine = StartCoroutine(PuzzleTimer());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canPlayPuzzle && !isOpen && !puzzleComplete)
        {
            SelectionManager.Instance.puzzleDetected = false;
            StartCoroutine(OpenPuzzleSequence());
            mainScreen.SetActive(false);
            quickSlots.SetActive(false);
            inventoryBTN.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.E) && isOpen)
        {
            PuzzleScreenOff();
        }
    }

    public void SetCanPlayPuzzle(bool value)
    {
        canPlayPuzzle = value;
    }

    public void PuzzleScreenOff()
    {
        SoundManager.Instance.ReturnToBiomeMusic();

        mainScreen.SetActive(true);
        quickSlots.SetActive(true);
        inventoryBTN.SetActive(true);

        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        if (timerText != null)
            timerText.text = "";

        StartCoroutine(ClosePuzzleSequence());
    }

    IEnumerator ClosePuzzleSequence()
    {
        yield return StartCoroutine(Fade(0, 1));

        rotatingPuzzleScreen.SetActive(false);

        puzzleCamera.enabled = false;
        mainCamera.enabled = true;

        yield return StartCoroutine(Fade(1, 0));

        isOpen = false;
    }

    // ⏳ TIMER ROUTINE WITH TMP UPDATE
    IEnumerator PuzzleTimer()
    {
        float t = puzzleTimeLimit;

        while (t > 0 && !puzzleComplete)
        {
            t -= Time.deltaTime;

            // Update timer UI
            if (timerText != null)
            {
                float display = Mathf.Max(0, t);
                timerText.text = display.ToString("F1"); // shows one decimal
            }

            yield return null;
        }

        // Timer expired
        if (!puzzleComplete && isOpen)
        {
            Debug.Log("Rotating Puzzle Time off");
            SoundManager.Instance.PlaySFX(SoundManager.Instance.puzzleGameOverSound.clip);
            Debug.Log("⏳ Puzzle time ended!");
            PuzzleScreenOff();
        }
    }

    public void OnRingSolved(RotatingRingUI ring)
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.rotatingPuzzlePartCompleteSound.clip);

        if (solvedPopup)
            StartCoroutine(ShowSolvedPopup());

        CheckIfAllSolved();
    }

    IEnumerator ShowSolvedPopup()
    {
        solvedPopup.SetActive(true);
        yield return new WaitForSeconds(1f);
        solvedPopup.SetActive(false);
    }

    IEnumerator ShowCompletePuzzlePopup()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.pzzleCompleteSound.clip);

        QuestManager.Instance.SetLiraPuzzleComplete();
        completePuzzlePopup.SetActive(true);
        puzzleComplete = true;

        // 🔥 Disable collider on puzzle object
        if (puzzleObject != null)
        {
            BoxCollider col = puzzleObject.GetComponent<BoxCollider>();
            if (col != null)
                col.enabled = false;
        }

        // 🔥 Prevent puzzle from ever opening again
        canPlayPuzzle = false;

        // Spawn cotton reward
        if (cottonPrefab != null && cotton != null)
        {
            Instantiate(cottonPrefab, position.transform.position, position.transform.rotation);
        }

        // Stop timer
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);

        if (timerText != null)
            timerText.text = "";

        GameTracker.Instance.SetPuzzleComplete(true);

        yield return new WaitForSeconds(1f);

        PuzzleScreenOff();
        completePuzzlePopup.SetActive(false);
        rotatingPuzzleScreen.SetActive(false);

        NotifUIManager.Instance.NotifyPuzzleComplete();
    }



    void CheckIfAllSolved()
    {
        foreach (var r in rings)
        {
            if (!r.isCorrect)
                return;
        }

        Debug.Log("All Rings Solved!");
        StartCoroutine(ShowCompletePuzzlePopup());
    }
}
