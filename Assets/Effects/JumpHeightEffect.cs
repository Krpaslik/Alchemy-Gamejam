using UnityEngine;

[CreateAssetMenu(menuName="Game/Effects/Jump Height", fileName="Effect_JumpHeight")]
public class JumpHeightEffect : StatusEffectDefinition
{
    public float multiplier = 2f;

    public override void Apply(PlayerEffectController target)
    {
        var m = target.GetComponent<PlayerMovement>();
        if (m) m.SetJumpHeightMultiplier(multiplier);
    }

    public override void Remove(PlayerEffectController target)
    {
        var m = target.GetComponent<PlayerMovement>();
        if (m) m.ResetJumpHeightMultiplier();
    }
}
