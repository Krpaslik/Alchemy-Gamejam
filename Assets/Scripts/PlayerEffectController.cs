using System.Collections.Generic;
using UnityEngine;
using MoveControl2D;

public class PlayerEffectController : MonoBehaviour
{
    class ActiveEffect
    {
        public StatusEffectDefinition def;
        public ItemTypeData sourceItem;
        public float timeLeft;
    }

    private readonly Dictionary<string, ActiveEffect> _active = new();
    private readonly Dictionary<string, float> _scaleMultipliers = new();

    private Transform _visualRoot;
    private Vector3 _baseScale;

    // DŮLEŽITÉ: controller, aby se dal refreshnout po změně scale
    private CharacterController2D _cc2d;

    public System.Action<StatusEffectDefinition, ItemTypeData, float> OnEffectUpdated;
    public System.Action<string> OnEffectRemoved;
    public PlayerMovement Movement { get; private set; }

    void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
        _cc2d = GetComponent<CharacterController2D>();

        // zatím škáluješ celý objekt
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

    public void AddEffectFromItem(StatusEffectDefinition def, ItemTypeData sourceItem)
    {
        if (def == null || sourceItem == null)
            return;

        float duration = Mathf.Max(0f, sourceItem.duration);

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
        EffectManager.Instance.AddOrRefresh(def.effectId, sourceItem.icon, duration);

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

        float signX = Mathf.Sign(_visualRoot.localScale.x);
        if (signX == 0) signX = 1;

        // 1) world Y spodku collideru PŘED změnou
        float bottomBefore = 0f;
        bool hasCC = _cc2d != null && _cc2d.boxCollider != null;
        if (hasCC)
            bottomBefore = _cc2d.boxCollider.bounds.min.y;

        // 2) změna scale
        _visualRoot.localScale = new Vector3(
            _baseScale.x * mul * signX,
            _baseScale.y * mul,
            _baseScale.z
        );

        // 3) promítni scale do fyziky
        Physics2D.SyncTransforms();

        // 4) world Y spodku collideru PO změně
        if (hasCC)
        {
            float bottomAfter = _cc2d.boxCollider.bounds.min.y;

            // 5) posun tak, aby nohy zůstaly na zemi (spodek collideru na stejné Y)
            float deltaY = bottomBefore - bottomAfter;
            if (Mathf.Abs(deltaY) > 0.00001f)
            {
                _cc2d.transform.position += new Vector3(0f, deltaY, 0f);
                Physics2D.SyncTransforms();
            }

            // 6) refresh ray spacing + okamžitě vyřeš případné překryvy
            _cc2d.recalculateDistanceBetweenRays();
            _cc2d.move(Vector3.zero);
        }
    }

    public void ClearAllEffects()
    {
        var ids = new List<string>(_active.Keys);
        foreach (var id in ids)
            RemoveEffect(id);

        _active.Clear();
        _scaleMultipliers.Clear();

        _visualRoot.localScale = _baseScale;

        if (_cc2d != null)
            _cc2d.recalculateDistanceBetweenRays();

        Physics2D.SyncTransforms();
    }
}
