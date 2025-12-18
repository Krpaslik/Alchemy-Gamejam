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
        // 1) PRIORITA: když nic neneseš, zkus sebrat nejbližší item
        if (!_carrier.HasItem)
        {
            if (_carrier.TryPickUp()) return;
        }

        // 2) Když máš před sebou UseItem target (stolek/kniha), použij ho
        // (doporučení: craft jen když nemáš item v ruce, viz CauldronStation níže)
        if (_current != null)
        {
            _current.UseItem(this);
            return;
        }

        // 3) Jinak když něco neseš, pusť to (bez teleportu)
        if (_carrier.HasItem)
        {
            _carrier.DropItem();
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
