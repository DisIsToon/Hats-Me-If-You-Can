using UnityEngine;

[System.Serializable]
public class TutorialStep
{
    public string stepID;
    [TextArea] public string instructionText;
    public TutorialCondition completionCondition;
}
