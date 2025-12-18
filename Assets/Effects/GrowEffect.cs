using UnityEngine;

[CreateAssetMenu(menuName = "Game/Effects/Grow", fileName = "Effect_Grow")]
public class GrowEffect : StatusEffectDefinition
{
    public float scaleMultiplier = 2f;

    public override void Apply(PlayerEffectController target)
    {
        target.SetScaleMultiplier(effectId, scaleMultiplier);
    }

    public override void Remove(PlayerEffectController target)
    {
        target.ClearScaleMultiplier(effectId);
    }
}
