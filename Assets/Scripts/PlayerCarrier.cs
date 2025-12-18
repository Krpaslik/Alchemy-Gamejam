using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCarrier : MonoBehaviour
{
    [Header("Carry")]
    public Transform carryPoint;          // empty nad hlavou
    public float pickupRadius = 0.6f;
    public LayerMask itemLayer;

    [Header("Drop")]
    public float dropDistanceDown = 0.8f; // kam pod sebe to položit
    public LayerMask groundLayer;
    public float groundCheckDistance = 2f;

    CarryableItem _carried;

    public bool HasItem => _carried != null;
    public CarryableItem CarriedItem => _carried;

    // Input System: Interact (Button)
    public void OnItemTake(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return; // jako GetKeyDown

        if (_carried == null)
            TryPickUp();
        else
            DropItem();
    }

    public bool TryPickUp()
    {
        var hit = Physics2D.OverlapCircle(transform.position, pickupRadius, itemLayer);
        if (!hit) return false;

        var item = hit.GetComponentInParent<CarryableItem>();
        if (!item || item.IsCarried) return false;

        _carried = item;
        _carried.PickUp(carryPoint);
        return true;
    }

    public void DropItem()
    {
        if (_carried == null) return;
        _carried.Drop();
        _carried = null;
    }

    public void Clear()
    {
        _carried = null;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
