using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Tutorial Popup")]
    [SerializeField] private GameObject tutorialPopupUI;
    [SerializeField] private float tutorialPopupDelay = 2f;

    // PUBLIC function you can assign to a Button
    public void ShowTutorialPopupAfterDelay()
    {
        StartCoroutine(ShowPopupCoroutine());
    }

    private IEnumerator ShowPopupCoroutine()
    {
        yield return new WaitForSeconds(tutorialPopupDelay);

        if (tutorialPopupUI != null)
        {
            tutorialPopupUI.SetActive(true);
        }
    }

    public void OnTutorialYesClicked()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);
        SceneManager.LoadScene("TutorialScene");
    }

    public void OnTutorialSkipClicked()
    {
        if (tutorialPopupUI != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.clickedSound.clip);
            tutorialPopupUI.SetActive(false);
        }
    }
}