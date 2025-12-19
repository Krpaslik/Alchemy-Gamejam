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
        RefreshVisual();
    }

    void OnValidate()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
        RefreshVisual();
    }


    public void RefreshVisual()
    {
        if (_sr == null || typeData == null) return;

        _sr.color = typeData.color;

        if (typeData.icon != null)
            _sr.sprite = typeData.icon;
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

        transform.SetParent(carryPoint, true);
        transform.rotation = Quaternion.identity;

        // nejdřív přibližně na carryPoint
        transform.position = carryPoint.position;

        if (_col != null)
        {
            // posun nahoru tak, aby spodek collideru byl na carryPoint
            float deltaY = carryPoint.position.y - _col.bounds.min.y;
            transform.position += new Vector3(0f, deltaY, 0f);
        }

        if (_col) _col.enabled = false;

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
