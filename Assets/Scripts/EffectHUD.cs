using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectHUD : MonoBehaviour
{
    [System.Serializable]
    public class IconMap { public string id; public Sprite icon; }

    [Header("UI")]
    [SerializeField] Transform container;
    [SerializeField] EffectRowUI rowPrefab;

    [Header("Icons")]
    [SerializeField] List<IconMap> icons = new();

    Dictionary<string, Sprite> _iconById = new();
    Dictionary<string, EffectRowUI> _rowById = new();
    bool _subscribed;

    void Awake()
    {
        if (container == null) container = transform;

        _iconById.Clear();
        foreach (var m in icons)
            if (!string.IsNullOrWhiteSpace(m.id))
                _iconById[m.id] = m.icon;
    }

    IEnumerator Start()
    {
        // počkej, než EffectManager existuje
        while (EffectManager.Instance == null) yield return null;

        Subscribe();
        RebuildRows();
    }

    void OnEnable()
    {
        // když se objekt zapne později, Start už doběhl, tak jen zajisti subscribe
        if (EffectManager.Instance != null) Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void Subscribe()
    {
        if (_subscribed) return;
        if (EffectManager.Instance == null) return;

        EffectManager.Instance.OnEffectsChanged += RebuildRows;
        _subscribed = true;
    }

    void Unsubscribe()
    {
        if (!_subscribed) return;
        if (EffectManager.Instance != null)
            EffectManager.Instance.OnEffectsChanged -= RebuildRows;

        _subscribed = false;
    }

    void Update()
    {
        var mgr = EffectManager.Instance;
        if (mgr == null) return;

        // jen update času/ikony – žádný rebuild
        foreach (var e in mgr.Effects)
        {
            if (_rowById.TryGetValue(e.id, out var row) && row != null)
            {
                row.Set(e.icon, e.timeLeft);
            }
        }
    }

    void RebuildRows()
    {
        var mgr = EffectManager.Instance;
        if (mgr == null) return;

        // aktivní ID
        var active = new HashSet<string>();
        foreach (var e in mgr.Effects) active.Add(e.id);

        // smaž neaktivní řádky
        var toRemove = new List<string>();
        foreach (var kv in _rowById)
            if (!active.Contains(kv.Key))
                toRemove.Add(kv.Key);

        foreach (var id in toRemove)
        {
            if (_rowById.TryGetValue(id, out var row) && row != null)
                Destroy(row.gameObject);
            _rowById.Remove(id);
        }

        // vytvoř nové řádky
        foreach (var e in mgr.Effects)
        {
            if (_rowById.ContainsKey(e.id)) continue;

            var row = Instantiate(rowPrefab, container);
            _rowById[e.id] = row;

            row.Set(e.icon, e.timeLeft);
        }
    }
}
