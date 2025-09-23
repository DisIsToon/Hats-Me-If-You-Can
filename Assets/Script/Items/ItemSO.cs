using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Item", fileName = "Item_")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public bool isStackable = true;
}
