using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CarryableItem : MonoBehaviour
{
    public ItemTypeData typeData;

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
            _rb.simulated = false; // vypne fyziku
        }

        if (_col) _col.enabled = false; // ať se to neplete do kolizí při nesení

        transform.SetParent(carryPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Drop()
    {
        IsCarried = false;

        transform.SetParent(null);
        //transform.position = worldPosition;

        if (_col) _col.enabled = true;

        if (_rb)
        {
            _rb.simulated = true;
            _rb.linearVelocity = Vector2.zero;
        }
    }

    // Později: “použití” předmětu (když ho někde položíš / aktivuješ)
    public void ApplyEffect(GameObject user)
    {
        if (typeData == null) return;

        switch (typeData.effect)
        {
            case ItemEffect.Heal:
                Debug.Log($"{user.name} heal +{typeData.amount} (demo)");
                break;

            case ItemEffect.SpeedBoost:
                Debug.Log($"{user.name} speed boost +{typeData.amount} na {typeData.duration}s (demo)");
                break;
        }
    }
}
