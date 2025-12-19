using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [Serializable]
    public class ActiveEffect
    {
        public string id;
        public Sprite icon;
        public float timeLeft;
    }

    public static EffectManager Instance { get; private set; }

    // UI si to bude číst (read-only kopie)
    public IReadOnlyList<ActiveEffect> Effects => _effects;
    public event Action OnEffectsChanged;

    readonly List<ActiveEffect> _effects = new();
    public event Action<string> OnEffectExpired;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        bool changed = false;

        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            _effects[i].timeLeft -= Time.deltaTime;
            if (_effects[i].timeLeft <= 0f)
            {
                string expiredId = _effects[i].id;
                _effects.RemoveAt(i);
                changed = true;
                OnEffectExpired?.Invoke(expiredId);
            }
        }
        if (changed) OnEffectsChanged?.Invoke();

    }

    /// Přidá nebo obnoví efekt (resetne duration)
    public void AddOrRefresh(string id, Sprite icon, float durationSeconds)
    {
        var e = _effects.Find(x => x.id == id);
        if (e == null)
        {
            e = new ActiveEffect { id = id, icon = icon, timeLeft = durationSeconds };
            _effects.Add(e);
        }
        else
        {
            e.timeLeft = durationSeconds;
        }

        OnEffectsChanged?.Invoke();
    }

    /// Užitečné když chceš efekt zrušit ručně
    public void Remove(string id)
    {
        int idx = _effects.FindIndex(x => x.id == id);
        if (idx >= 0)
        {
            _effects.RemoveAt(idx);
            OnEffectsChanged?.Invoke();
        }
    }

    public void ClearAll()
    {
        if (_effects.Count == 0) return;
        _effects.Clear();
        OnEffectsChanged?.Invoke();
    }

}
