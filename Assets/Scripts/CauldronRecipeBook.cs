using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Cauldron Recipe Book", fileName = "CauldronRecipeBook")]
public class CauldronRecipeBook : ScriptableObject
{
    public List<CauldronRecipe> recipes = new();
}
