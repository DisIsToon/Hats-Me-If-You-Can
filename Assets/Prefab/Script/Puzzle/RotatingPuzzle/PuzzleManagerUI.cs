using UnityEngine;
<<<<<<< Updated upstream
using System.Collections;

public class PuzzleManagerUI : MonoBehaviour
{
    public GameObject rotatingPuzzleScreen;
    public GameObject solvedPopup;
    public GameObject completePuzzlePopup;
    public bool isOpen;

    public RotatingRingUI[] rings;
    void Start()
    {
        rotatingPuzzleScreen.SetActive(false);
        isOpen = false;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("p is pressed");
        }
        if (Input.GetKeyDown(KeyCode.P) && !isOpen)
        {
            Debug.Log("p is pressed, inventory open");
            rotatingPuzzleScreen.SetActive(true);
            isOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.P) && isOpen)
        {
            Debug.Log("p is pressed, inventory closed");
            rotatingPuzzleScreen.SetActive(false);

            isOpen = false;
        }
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
=======

public class PuzzleManagerUI : MonoBehaviour
{
    [Header("Puzzle Setup")]
    public RotatingRingUI[] rings;
    public GameObject solvedPanel;

    [Header("Trigger Settings")]
    public KeyCode triggerKey = KeyCode.P;

    private bool isSolved = false;
    private bool isOpen = false;

    private void Start()
    {
        gameObject.SetActive(false);

        foreach (var ring in rings)
        {
            ring.OnSnapped += CheckPuzzleSolved;
            ring.OnRingSolved += HandleRingSolved;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            if (isOpen)
                ClosePuzzle();
            else
                OpenPuzzle();
        }
    }

    private void HandleRingSolved(RotatingRingUI ring)
    {
        Debug.Log($"✅ Ring {ring.name} locked in correct position.");
        CheckPuzzleSolved();
    }

    private void CheckPuzzleSolved()
    {
        if (isSolved) return;

        foreach (var ring in rings)
        {
            if (!ring.IsLocked())
                return;
        }

        PuzzleSolved();
    }

    private void PuzzleSolved()
    {
        isSolved = true;
        Debug.Log("🎉 Puzzle Fully Solved!");

        if (solvedPanel != null)
            solvedPanel.SetActive(true);
    }

    public void OpenPuzzle()
    {
        isOpen = true;
        gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ClosePuzzle()
    {
        isOpen = false;
        gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public bool IsOpen() => isOpen;
>>>>>>> Stashed changes
}
