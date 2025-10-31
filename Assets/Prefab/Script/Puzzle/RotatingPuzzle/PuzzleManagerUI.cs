using UnityEngine;
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
}