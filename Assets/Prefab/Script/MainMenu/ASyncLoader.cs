using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ASyncLoader : MonoBehaviour {
    [Header("Menu Screens")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;

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
            float targetProgress = (loadOperation.progress / 0.9f) * 0.5f;

            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.deltaTime * 0.2f);

            loadingSlider.value = displayedProgress;

            // Pause checkpoints
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

        // Scene is ready in background
        // Fill remaining 50% smoothly
        while (displayedProgress < 1f) {
            displayedProgress += Time.deltaTime * 0.25f;
            loadingSlider.value = displayedProgress;

            yield return null;
        }

        loadOperation.allowSceneActivation = true;
    }
}