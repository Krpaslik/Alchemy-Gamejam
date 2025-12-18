using UnityEngine;

[CreateAssetMenu(menuName="Game/Effects/Reverse Gravity", fileName="Effect_ReverseGravity")]
public class ReverseGravityEffect : StatusEffectDefinition
{
    public override void Apply(PlayerEffectController target)
    {
        var m = target.GetComponent<PlayerMovement>();
        if (m) m.SetReverseGravity(true);
    }

    public override void Remove(PlayerEffectController target)
    {
        var m = target.GetComponent<PlayerMovement>();
        if (m) m.ResetReverseGravity();
    }
}
