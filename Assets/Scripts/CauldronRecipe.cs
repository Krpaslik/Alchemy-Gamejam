using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Cauldron Recipe", fileName = "CauldronRecipe")]
public class CauldronRecipe : ScriptableObject
{
    [Header("Ingredients (order doesn't matter)")]
    public List<ItemTypeData> ingredients = new();

    [Header("Result")]
    public ItemTypeData resultType;
    public string title;
    public string description;

    // Kolik výsledných itemů (zatím necháme 1)
    public int resultCount = 1;
}
