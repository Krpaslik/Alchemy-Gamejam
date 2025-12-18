using UnityEngine;

[CreateAssetMenu(menuName="Game/Effects/Glide", fileName="Effect_Glide")]
public class GlideEffect : StatusEffectDefinition
{
    [Range(0f, 1f)]
    public float glideMultiplier = 0.25f; // menší = větší vznášení

    public override void Apply(PlayerEffectController target)
    {
        var m = target.GetComponent<PlayerMovement>();
        if (m) m.SetGlideMultiplier(glideMultiplier);
    }

    public override void Remove(PlayerEffectController target)
    {
        var m = target.GetComponent<PlayerMovement>();
        if (m) m.ResetGlideMultiplier();
    }
}
