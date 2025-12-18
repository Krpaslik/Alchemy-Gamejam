using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Item Type Data", fileName = "ItemTypeData")]
public class ItemTypeData : ScriptableObject
{
    public string displayName = "NewItem";
    public Color color = Color.white;

    // parametry efektu
    public float duration = 0f;
    
    [Header("Effect on Use")]
    public List<StatusEffectDefinition> useEffects = new();
}