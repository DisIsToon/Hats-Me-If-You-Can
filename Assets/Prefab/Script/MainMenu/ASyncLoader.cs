using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ASyncLoader : MonoBehaviour {
    [Header("Menu Screens")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;

    [Header("Progress Bar")]
    [SerializeField] private Image loadingBar;

    private bool pausedOnce = false;
    private bool pausedTwice = false;
    private bool pausedThree = false;

    public void LoadLevelBtn(string levelToLoad) {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);

        StartCoroutine(LoadLevelAsync(levelToLoad));
    }

    IEnumerator LoadLevelAsync(string levelToLoad) {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false;

        float displayedProgress = 0f;

        // First 50% = real loading progress
        while (loadOperation.progress < 0.9f) {

            float targetProgress = loadOperation.progress;

            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.deltaTime * 0.5f);
            loadingBar.fillAmount = displayedProgress;

            //pause Checkpoints
            if (displayedProgress > 0.15f && !pausedOnce) {
                pausedOnce = true;
                yield return new WaitForSeconds(0.6f);
            }

            if (displayedProgress > 0.30f && !pausedTwice) {
                pausedTwice = true;
                yield return new WaitForSeconds(0.6f);
            }

            if (displayedProgress > 0.45f && !pausedThree) {
                pausedThree = true;
                yield return new WaitForSeconds(0.6f);
            }

            yield return null;
        }

        // Wait until scene is fully ready
        while (!loadOperation.isDone) {
            // Smoothly fill the last 10%
            displayedProgress = Mathf.MoveTowards(displayedProgress, 1f, Time.deltaTime * 0.5f);
            loadingBar.fillAmount = displayedProgress;

            // Only activate the scene when bar visually full
            if (displayedProgress >= 1f)
                loadOperation.allowSceneActivation = true;

            yield return null;
        }
    }
}