using UnityEngine;

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
}
