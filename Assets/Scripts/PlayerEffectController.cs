using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectController : MonoBehaviour
{
    class ActiveEffect
    {
        public StatusEffectDefinition def;
        public ItemTypeData sourceItem;
        public float timeLeft;
    }

    // effectId → aktivní efekt
    private readonly Dictionary<string, ActiveEffect> _active = new();

    // effectId → scale multiplier
    private readonly Dictionary<string, float> _scaleMultipliers = new();

    private Transform _visualRoot;
    private Vector3 _baseScale;

    // UI hooky
    public System.Action<StatusEffectDefinition, ItemTypeData, float> OnEffectUpdated;
    public System.Action<string> OnEffectRemoved;

    void Awake()
    {
        // pokud máš grafiku v child objektu, dej sem referenci místo transform
        _visualRoot = transform;
        _baseScale = _visualRoot.localScale;
    }

    void Update()
    {
        if (_active.Count == 0) return;

        float dt = Time.deltaTime;
        var toRemove = new List<string>();

        foreach (var kv in _active)
        {
            var e = kv.Value;
            e.timeLeft -= dt;

            OnEffectUpdated?.Invoke(e.def, e.sourceItem, e.timeLeft);

            if (e.timeLeft <= 0f)
                toRemove.Add(kv.Key);
        }

        for (int i = 0; i < toRemove.Count; i++)
            RemoveEffect(toRemove[i]);
    }

    /// <summary>
    /// Přidá efekt. Pokud už existuje stejný effectId, jen resetne duration.
    /// </summary>
    public void AddEffectFromItem(StatusEffectDefinition def, ItemTypeData sourceItem)
    {
        if (def == null || sourceItem == null)
            return;

        float duration = Mathf.Max(0f, sourceItem.duration);

        // už existuje → reset času
        if (_active.TryGetValue(def.effectId, out var existing))
        {
            existing.timeLeft = duration;
            OnEffectUpdated?.Invoke(existing.def, existing.sourceItem, existing.timeLeft);
            return;
        }

        var e = new ActiveEffect
        {
            def = def,
            sourceItem = sourceItem,
            timeLeft = duration
        };

        _active.Add(def.effectId, e);

        def.Apply(this);
        OnEffectUpdated?.Invoke(def, sourceItem, e.timeLeft);

        if (duration <= 0f)
            RemoveEffect(def.effectId);
    }

    private void RemoveEffect(string effectId)
    {
        if (!_active.TryGetValue(effectId, out var e))
            return;

        e.def.Remove(this);
        _active.Remove(effectId);
        OnEffectRemoved?.Invoke(effectId);
    }

    // ===== API pro efekty (příklad: scale) =====

    public void SetScaleMultiplier(string effectId, float mul)
    {
        _scaleMultipliers[effectId] = mul;
        RecalculateScale();
    }

    public void ClearScaleMultiplier(string effectId)
    {
        if (_scaleMultipliers.Remove(effectId))
            RecalculateScale();
    }

    void RecalculateScale()
    {
        float mul = 1f;
        foreach (var v in _scaleMultipliers.Values)
            mul *= v;

        // zachovej flip z PlayerMovement (ten mění znaménko X)
        float signX = Mathf.Sign(_visualRoot.localScale.x);
        _visualRoot.localScale = new Vector3(
            _baseScale.x * mul * signX,
            _baseScale.y * mul,
            _baseScale.z
        );
    }
}