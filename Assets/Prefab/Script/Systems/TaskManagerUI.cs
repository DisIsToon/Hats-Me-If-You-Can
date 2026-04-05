using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaskManagerUI : MonoBehaviour
{
    public TextMeshProUGUI taskText;

    private List<Task> tasks = new List<Task>();

    void Start()
    {
        SetupTasks();
    }

    void Update()
    {
        UpdateCurrentTask();
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

            taskText.text = task.description;
            return;
        }

        taskText.text = "Current Task Complete!";
    }
}