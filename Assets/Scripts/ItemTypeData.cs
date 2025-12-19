using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Item Type Data", fileName = "ItemTypeData")]
public class ItemTypeData : ScriptableObject
{
    public string displayName = "NewItem";

    [Header("Visuals")]
    public Color color = Color.white;
    public Sprite icon;

    [Header("Effect timing")]
    public float duration = 0f;

    [Header("Effect on Use")]
    public List<StatusEffectDefinition> useEffects = new();
}
