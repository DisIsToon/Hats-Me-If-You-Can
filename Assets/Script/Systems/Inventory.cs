using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public class StartingStack
    {
        public ItemSO item;
        public int quantity = 1;
    }

    [Header("Debug / Starting Items")]
    public List<StartingStack> startingItems = new List<StartingStack>();

    private readonly Dictionary<ItemSO, int> _stacks = new Dictionary<ItemSO, int>();

    void Awake()
    {
        foreach (var s in startingItems)
        {
            if (s.item == null || s.quantity <= 0) continue;
            Add(s.item, s.quantity);
        }
    }

    public int GetCount(ItemSO item) =>
        (item != null && _stacks.TryGetValue(item, out var c)) ? c : 0;

    public bool Has(ItemSO item, int quantity = 1) =>
        item != null && quantity > 0 && GetCount(item) >= quantity;

    public bool Add(ItemSO item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;
        if (_stacks.TryGetValue(item, out var c)) _stacks[item] = c + quantity;
        else _stacks[item] = quantity;
        return true;
    }

    public bool Remove(ItemSO item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;
        if (!_stacks.TryGetValue(item, out var c) || c < quantity) return false;
        c -= quantity;
        if (c <= 0) _stacks.Remove(item);
        else _stacks[item] = c;
        return true;
    }

    public bool CanAfford(PotionRecipeSO recipe)
    {
        if (recipe == null) return false;
        foreach (var ing in recipe.ingredients)
        {
            if (ing.item == null || ing.quantity <= 0) return false;
            if (!Has(ing.item, ing.quantity)) return false;
        }
        return true;
    }

    public bool SpendIngredients(PotionRecipeSO recipe)
    {
        if (!CanAfford(recipe)) return false;
        foreach (var ing in recipe.ingredients)
            Remove(ing.item, ing.quantity);
        return true;
    }

    public void AddResult(PotionRecipeSO recipe)
    {
        if (recipe?.resultPotion == null || recipe.resultQuantity <= 0) return;
        Add(recipe.resultPotion, recipe.resultQuantity);
    }

    // Debug text for the OnGUI panel
    public string DebugContents()
    {
        if (_stacks.Count == 0) return "(empty)";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var kv in _stacks)
            sb.AppendLine($"{kv.Key.itemName}: {kv.Value}");
        return sb.ToString();
    }
}
