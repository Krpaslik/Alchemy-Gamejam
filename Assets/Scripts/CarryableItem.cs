using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class CarryableItem : MonoBehaviour
{
    public ItemTypeData typeData;

    public event Action<CarryableItem> PickedUp;
    public event Action<CarryableItem> Dropped;

    Rigidbody2D _rb;
    Collider2D _col;
    SpriteRenderer _sr;

    public bool IsCarried { get; private set; }

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        ApplyTypeVisual();
    }

    void OnValidate()
    {
        // aby se barva měnila i v editoru
        _sr = GetComponentInChildren<SpriteRenderer>();
        ApplyTypeVisual();
    }

    void ApplyTypeVisual()
    {
        if (_sr != null && typeData != null)
            _sr.color = typeData.color;
    }

    public void RefreshVisual()
    {
        // stejné jako tvoje ApplyTypeVisual, jen veřejné
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && typeData != null)
            sr.color = typeData.color;
    }


    public void PickUp(Transform carryPoint)
    {
        IsCarried = true;

        if (_rb)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.simulated = false;
        }

        if (_col) _col.enabled = false;

        transform.SetParent(carryPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        PickedUp?.Invoke(this);
    }

    public void Drop()
    {
        IsCarried = false;

        transform.SetParent(null);

        if (_col) _col.enabled = true;

        if (_rb)
        {
            _rb.simulated = true;
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = 1f;
            _rb.linearVelocity = Vector2.zero;
        }

        Dropped?.Invoke(this);
    }
}
