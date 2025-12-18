using System.Collections.Generic;
using UnityEngine;

public class CauldronBowl : MonoBehaviour
{
    private readonly HashSet<CarryableItem> _inside = new();

    public IReadOnlyCollection<CarryableItem> ItemsInside => _inside;

    void OnTriggerEnter2D(Collider2D col)
    {
        var item = col.GetComponentInParent<CarryableItem>();
        if (item == null) return;
        if (item.IsCarried) return;

        _inside.Add(item);
    }

    void OnTriggerExit2D(Collider2D col)
    {
        var item = col.GetComponentInParent<CarryableItem>();
        if (item == null) return;

        _inside.Remove(item);
    }

    public void CleanupNulls()
    {
        _inside.RemoveWhere(i => i == null);
    }
}
