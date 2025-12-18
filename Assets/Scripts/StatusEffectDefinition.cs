using UnityEngine;

public abstract class StatusEffectDefinition : ScriptableObject
{
    public string effectId;

    public abstract void Apply(PlayerEffectController target);
    public abstract void Remove(PlayerEffectController target);
}
