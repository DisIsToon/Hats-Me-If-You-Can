using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Ingredient
{
    public ItemSO item;
    public int quantity;
}

[CreateAssetMenu(menuName = "RPG/Potion Recipe", fileName = "Recipe_")]
public class PotionRecipeSO : ScriptableObject
{
    [Header("Inputs")]
    public List<Ingredient> ingredients = new List<Ingredient>();

    [Header("Output")]
    public ItemSO resultPotion;
    public int resultQuantity = 1;
}
