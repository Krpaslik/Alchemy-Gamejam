using UnityEngine;

public enum ItemEffect
{
    None,
    Heal,
    SpeedBoost
}

[CreateAssetMenu(menuName = "Game/Item Type Data", fileName = "ItemTypeData")]
public class ItemTypeData : ScriptableObject
{
    public string displayName = "Item";
    public Color color = Color.yellow;     // zatím reprezentace "obrázku"
    public ItemEffect effect = ItemEffect.None;

    // parametry efektu
    public float amount = 10f;
    public float duration = 3f;
}
