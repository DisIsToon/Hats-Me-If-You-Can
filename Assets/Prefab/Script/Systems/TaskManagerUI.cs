using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using TMPro;
public class TaskManagerUI : MonoBehaviour
{
    public TextMeshProUGUI taskText;
    public GameObject taskBG;      // drag your Task background here
    public GameObject taskTextObj; // drag the Text (or parent) here

    private bool isVisible = true;
    private string lastTask = "";
    private List<Task> tasks = new List<Task>();

    [Header("Animation")]
    public RectTransform taskBGRect;
    public RectTransform taskTextRect;

    public float slideDuration = 0.45f;
    public float hiddenX = 1000f; // off-screen right
    public float visibleX = 0f;  // normal position

    [Header("Final Positions")]
    public Vector2 taskBGVisiblePos = new Vector2(-633.4f, 342.3f);
    public Vector2 taskTextVisiblePos = new Vector2(-742.46f, 342.4f);

    private Coroutine currentAnim;
    private bool hasShownInitialPopup = false;

    void Start()
    {
        SetupTasks();

        // Ensure they start OFF-SCREEN even if inactive
        taskBGRect.anchoredPosition = new Vector2(taskBGVisiblePos.x - hiddenX, taskBGVisiblePos.y);
        taskTextRect.anchoredPosition = new Vector2(taskTextVisiblePos.x - hiddenX, taskTextVisiblePos.y);

        StartCoroutine(StartPopup());
    }

    IEnumerator StartPopup()
    {
        yield return new WaitForSeconds(5f);

        PlaySlideIn();
        hasShownInitialPopup = true; // mark as already shown
    }

    void Update()
    {
        UpdateCurrentTask();
    }

    public void ToggleTaskUI()
    {
        isVisible = !isVisible;

        taskBG.SetActive(isVisible);
        taskTextObj.SetActive(isVisible);
    }

    IEnumerator SlideIn()
    {
        float time = 0f;

        taskBG.SetActive(true);
        taskTextObj.SetActive(true);

        Canvas.ForceUpdateCanvases();

        // Start = off-screen right (relative to final pos)
        Vector2 bgStart = new Vector2(taskBGVisiblePos.x - hiddenX, taskBGVisiblePos.y);
        Vector2 textStart = new Vector2(taskTextVisiblePos.x - hiddenX, taskTextVisiblePos.y);

        // End = EXACT positions you want
        Vector2 bgEnd = taskBGVisiblePos;
        Vector2 textEnd = taskTextVisiblePos;

        taskBGRect.anchoredPosition = bgStart;
        taskTextRect.anchoredPosition = textStart;

        while (time < slideDuration)
        {
            time += Time.deltaTime;
            float t = time / slideDuration;

            // smoother for left → right UI motion
            t = Mathf.SmoothStep(0f, 1f, t);

            // Slight polish (optional but nice)
            float overshoot = 1.05f;
            t = Mathf.Lerp(t, Mathf.Sin(t * Mathf.PI * 0.5f) * overshoot, 0.15f);

            taskBGRect.anchoredPosition = Vector2.Lerp(bgStart, bgEnd, t);
            taskTextRect.anchoredPosition = Vector2.Lerp(textStart, textEnd, t);

            yield return null;
        }

        // Snap exactly to final (prevents tiny offset bugs)
        taskBGRect.anchoredPosition = bgEnd;
        taskTextRect.anchoredPosition = textEnd;

        yield return new WaitForSeconds(3f);

        taskBG.SetActive(false);
        taskTextObj.SetActive(false);
    }

    public void PlaySlideIn()
    {
        if (currentAnim != null)
            StopCoroutine(currentAnim);
        
        currentAnim = StartCoroutine(SlideIn());
    }

    void SetupTasks()
    {
        var qm = QuestManager.Instance;
        var gt = GameTracker.Instance;

        tasks = new List<Task>()
        {
       
            // 1. Capture Shy Hat
            new Task {
                description = "Capture Shy Hat",
                isCompleted = () => gt.shyHatCaptured,
                canShow = () => true
            },

            // 2. Talk to Lira
            new Task {
                description = "Help Lira the Forest Sprite",
                isCompleted = () => qm.liraQuestAccepted,
                canShow = () => !qm.liraQuestAccepted
            },

            // 3. Complete Tree Puzzle
            new Task {
                description = "Complete the Tree Puzzle",
                isCompleted = () => qm.puzzleComplete,
                canShow = () => qm.liraQuestAccepted && !qm.puzzleComplete
            },

            // 4. Go to Winter Forest (LOCK after Tulip)
            new Task {
                description = "Go to the Winter Forest",
                isCompleted = () => gt.visitedWinterForest,
                canShow = () => qm.winterForestPass && !qm.questCompleteTulip
            },
       
            // 5. Talk to Tulip
            new Task {
                description = "Help Tulip",
                isCompleted = () => qm.tulipQuestAccepted,
                canShow = () => qm.winterForestPass && !qm.tulipQuestAccepted
            },

            // 6. Find Lost Hat
            new Task {
                description = "Find Lost Hat",
                isCompleted = () => qm.questCompleteTulip,
                canShow = () => qm.tulipQuestAccepted && !qm.questCompleteTulip
            },

            // 7. Go to Castle (LOCK after Mallow)
            new Task {
                description = "Go to the Castle Ruins",
                isCompleted = () => gt.visitedCastleRuin,
                canShow = () => qm.questCompleteTulip && !qm.questCompleteMallow
            },

            // 8. Talk to Mallow
            new Task {
                description = "Help Mallow",
                isCompleted = () => qm.mallowQuestAccepted,
                canShow = () => qm.questCompleteTulip && !qm.mallowQuestAccepted
            },

            // 9. Find Lumishroom
            new Task {
                description = "Find Lumishroom",
                isCompleted = () => qm.questCompleteMallow,
                canShow = () => qm.mallowQuestAccepted && !qm.questCompleteMallow
            },

            // 10. Capture Fast Hat
            new Task {
                description = "Capture Fast Hat",
                isCompleted = () => gt.fastHatCaptured,
                canShow = () => qm.questCompleteMallow && !gt.fastHatCaptured
            },

            // 11. Capture Jump Hat
            new Task {
                description = "Capture Jump Hat",
                isCompleted = () => gt.jumpHatCaptured,
                canShow = () => gt.fastHatCaptured && !gt.jumpHatCaptured
            },

            // 12. Find Headmaster
            new Task {
                description = "Find Headmaster Eira",
                isCompleted = () => qm.HeadMasterQuestAccepted,
                canShow = () => gt.AllHatsCaptured() && !qm.HeadMasterQuestAccepted
            },
        };
    }

    void UpdateCurrentTask()
    {
        foreach (var task in tasks)
        {
            if (!task.canShow())
                continue;

            if (task.isCompleted())
                continue;

            if (lastTask != task.description)
            {
                lastTask = task.description;
                taskText.text = task.description;

                // Prevent double popup at start
                if (hasShownInitialPopup)
                {
                    StartCoroutine(HandleTaskPopup());
                }
            }

            return;
        }

        // If all tasks complete
        if (lastTask != "Current Task Complete!")
        {
            lastTask = "Current Task Complete!";
            taskText.text = lastTask;

            if (hasShownInitialPopup)
            {
                StartCoroutine(HandleTaskPopup());
            }
        }
    }

    IEnumerator HandleTaskPopup()
    {
        yield return new WaitForSeconds(1f);

        //BLOCK if video is playing
        if (GameTracker.Instance != null &&
            GameTracker.Instance.videoRawImage != null &&
            GameTracker.Instance.videoRawImage.gameObject.activeSelf)
        {
            yield break;
        }

        PlaySlideIn();
    }
}