using UnityEngine;

[System.Serializable]
public class Task
{
    public string description;

    public System.Func<bool> isCompleted;
    public System.Func<bool> canShow;
}
