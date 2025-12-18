using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CauldronStation : MonoBehaviour, IInteractable
{
    public CauldronBowl bowl;
    public CauldronRecipeBook recipeBook;

    [Header("Result Spawn")]
    public CarryableItem itemPrefab;
    public Transform spawnPoint;

    public void UseItem(PlayerInteractor player)
    {
        if (player.IsCarrying)
        {
            Debug.Log("Nejdřív polož itemy do kotle (ruce musí být prázdné).");
            return;
        }

        TryCraft();
    }

    void TryCraft()
    {
        if (bowl == null || recipeBook == null || itemPrefab == null)
        {
            Debug.LogWarning("CauldronStation: chybí reference (bowl/recipeBook/itemPrefab).");
            return;
        }

        bowl.CleanupNulls();

        var items = bowl.ItemsInside
            .Where(i => i != null && !i.IsCarried && i.typeData != null)
            .ToList();

        if (items.Count == 0)
        {
            Debug.Log("Kotel je prázdný.");
            return;
        }

        var availableTypes = items.Select(i => i.typeData).ToList();
        var recipe = FindFirstMatchingRecipe(availableTypes);

        if (recipe == null)
        {
            Debug.Log("Žádný recept neodpovídá surovinám v kotli.");
            return;
        }

        var consume = PickItemsToConsume(items, recipe.ingredients);
        if (consume == null)
        {
            Debug.Log("Recept matchuje, ale nepodařilo se spárovat suroviny (divné duplicitní typy?).");
            return;
        }

        foreach (var it in consume)
            Destroy(it.gameObject);

        var pos = spawnPoint ? spawnPoint.position : bowl.transform.position;
        var result = Instantiate(itemPrefab, pos, Quaternion.identity);
        result.typeData = recipe.resultType;
        result.RefreshVisual();

        Debug.Log($"Uvařeno: {recipe.resultType.name}");
    }

    CauldronRecipe FindFirstMatchingRecipe(List<ItemTypeData> available)
    {
        foreach (var r in recipeBook.recipes)
        {
            if (r == null || r.resultType == null || r.ingredients == null || r.ingredients.Count == 0)
                continue;

            if (ContainsMultiset(available, r.ingredients))
                return r;
        }
        return null;
    }

    static bool ContainsMultiset(List<ItemTypeData> available, List<ItemTypeData> needed)
    {
        var counts = new Dictionary<ItemTypeData, int>();
        foreach (var a in available)
        {
            if (!counts.ContainsKey(a)) counts[a] = 0;
            counts[a]++;
        }

        foreach (var n in needed)
        {
            if (n == null) return false;
            if (!counts.TryGetValue(n, out var c) || c <= 0) return false;
            counts[n] = c - 1;
        }
        return true;
    }

    static List<CarryableItem> PickItemsToConsume(List<CarryableItem> items, List<ItemTypeData> needed)
    {
        var used = new HashSet<CarryableItem>();
        var picked = new List<CarryableItem>();

        foreach (var n in needed)
        {
            var found = items.FirstOrDefault(i => !used.Contains(i) && i.typeData == n);
            if (found == null) return null;
            used.Add(found);
            picked.Add(found);
        }
        return picked;
    }
}
