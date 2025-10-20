using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Checkpoints", menuName = "ScriptableObjects/Checkpoints", order = 1)]

public class Checkpoints : ScriptableObject
{
    public string name;
    public bool isCompleted;                                                                        

}
