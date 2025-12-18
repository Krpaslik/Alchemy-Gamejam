using UnityEngine;

[CreateAssetMenu(menuName = "Game/Effects/Shrink", fileName = "Effect_Shrink")]
public class ShrinkEffect : StatusEffectDefinition
{
    public float scaleMultiplier = 0.5f;

    public override void Apply(PlayerEffectController target)
    {
        target.SetScaleMultiplier(effectId, scaleMultiplier);
    }

    public override void Remove(PlayerEffectController target)
    {
        target.ClearScaleMultiplier(effectId);
    }
}
