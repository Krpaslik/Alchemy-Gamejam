using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerCarrier))]
public class PlayerInteractor : MonoBehaviour
{
    private PlayerCarrier _carrier;

    // aktuální interact target, do kterého stojíš (např. stolek u kotle)
    private IInteractable _current;

    void Awake()
    {
        _carrier = GetComponent<PlayerCarrier>();
    }

    // TUTO metodu napojíš v PlayerInput (Invoke Unity Events)
    public void OnUseItem(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return; // jako GetKeyDown
        UseItem();
    }

    void UseItem()
    {
        // PRIORITA: když nic neneseš, zkus sebrat nejbližší item
        if (!_carrier.HasItem)
        {
            if (_carrier.TryPickUp()) return;
        }

        // PRIORITA 2: když něco neseš a má to efekt, použij to (potion)
        if (_carrier.HasItem)
        {
            var carried = _carrier.CarriedItem;
            var data = carried != null ? carried.typeData : null;

            if (data != null && data.useEffects != null && data.useEffects.Count > 0)
            {
                var effects = GetComponent<PlayerEffectController>();
                if (effects != null)
                {
                    foreach (var eff in data.useEffects)
                        effects.AddEffectFromItem(eff, data);

                    // spotřebuj item
                    Destroy(carried.gameObject);
                    _carrier.Clear(); // musí existovat - pokud nemáš, napiš a upravím na tvou verzi

                    return; // důležité: už nic dalšího (drop, target) se neprovádí
                }
            }
        }

        // Jinak když něco neseš, pusť to
        if (_carrier.HasItem)
        {
            _carrier.DropItem();
            return;
        }

        // Když máš před sebou UseItem target (stolek/kniha), použij ho
        if (_current != null)
        {
            _current.UseItem(this);
            return;
        }
    }

    public bool IsCarrying => _carrier.HasItem;

    // registrace targetu přes trigger
    void OnTriggerEnter2D(Collider2D col)
    {
        var interactable = col.GetComponentInParent<IInteractable>();
        if (interactable != null)
            _current = interactable;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        var interactable = col.GetComponentInParent<IInteractable>();
        if (interactable != null && _current == interactable)
            _current = null;
    }
}

// jednoduché rozhraní pro věci “se kterýma jde interagovat”
public interface IInteractable
{
    void UseItem(PlayerInteractor player);
}
