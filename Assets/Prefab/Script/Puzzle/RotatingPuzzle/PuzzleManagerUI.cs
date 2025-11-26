using UnityEngine;
using System.Collections;

public class PuzzleManagerUI : MonoBehaviour
{
    public static PuzzleManagerUI Instance { get; set; }

    [Header("Cameras")]
    public Camera mainCamera;      // Your normal gameplay camera
    public Camera puzzleCamera;    // A special camera aimed at the puzzle

    [Header("Puzzle Parts")]
    public GameObject rotatingPuzzleScreen;
    public GameObject solvedPopup;
    public GameObject completePuzzlePopup;
    public bool isOpen;
    private bool canPlayPuzzle = false;
    public RotatingRingUI[] rings;
    public GameObject mainScreen;

    [Header("Fade Setting")]
    public CanvasGroup fadePanel;   // The black overlay
    public float fadeDuration = 0.5f;

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
        isOpen = true;

        // Fade to black
        yield return StartCoroutine(Fade(0, 1));

        // Switch cameras
        mainCamera.enabled = false;
        puzzleCamera.enabled = true;

        // Fade back in
        yield return StartCoroutine(Fade(1, 0));

        // UI appears
        rotatingPuzzleScreen.SetActive(true);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canPlayPuzzle && !isOpen)
        {
            SelectionManager.Instance.puzzleDetected = false;
            mainScreen.SetActive(false);
            StartCoroutine(OpenPuzzleSequence());
 
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
        mainScreen.SetActive(true);
        StartCoroutine(ClosePuzzleSequence());
    }

    IEnumerator ClosePuzzleSequence()
    {
        // Fade to black
        yield return StartCoroutine(Fade(0, 1));

        rotatingPuzzleScreen.SetActive(false);

        // Restore cameras
        puzzleCamera.enabled = false;
        mainCamera.enabled = true;

        // Fade in again
        yield return StartCoroutine(Fade(1, 0));

        isOpen = false;
    }

    public void OnRingSolved(RotatingRingUI ring)
    {
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
        completePuzzlePopup.SetActive(true);
        yield return new WaitForSeconds(1f);
        completePuzzlePopup.SetActive(false);
        rotatingPuzzleScreen.SetActive(false);
        Debug.Log("Quest Pass rcv!");
    }

    void CheckIfAllSolved()
    {
        foreach (var r in rings)
        {
            if (!r.isCorrect)
                return;
        }

        Debug.Log("✅ All Rings Solved!");
        StartCoroutine(ShowCompletePuzzlePopup());
    }
}