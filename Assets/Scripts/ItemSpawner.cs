using System.Collections;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public CarryableItem itemPrefab;
    public ItemTypeData spawnType;
    public Transform spawnAnchor;               // když null, bere transform tohoto objektu

    [Header("Timing")]
    public float respawnDelay = 10f;

    [Header("Behavior")]
    public bool freezeUntilPickedUp = true;     // dokud není sebrán, žádná fyzika
    public bool destroyPreviousOnPickup = true; // anti-stack (znič starý kus, pokud existuje)

    CarryableItem _current;
    CarryableItem _lastPicked;
    Coroutine _respawnRoutine;

    void Start()
    {
        SpawnNow();
    }

    void OnDisable()
    {
        Unhook(_current);
        if (_respawnRoutine != null) StopCoroutine(_respawnRoutine);
    }

    void SpawnNow()
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning($"{name}: Chybí itemPrefab.");
            return;
        }

        var anchor = spawnAnchor != null ? spawnAnchor : transform;
        _current = Instantiate(itemPrefab, anchor.position, anchor.rotation);

        if (spawnType != null)
        {
            _current.typeData = spawnType;
            _current.RefreshVisual();
        }

        if (freezeUntilPickedUp)
            FreezePhysics(_current, true);

        Hook(_current);
    }

    void Hook(CarryableItem item)
    {
        if (item == null) return;
        item.PickedUp += OnItemPickedUp;
    }

    void Unhook(CarryableItem item)
    {
        if (item == null) return;
        item.PickedUp -= OnItemPickedUp;
    }

    void OnItemPickedUp(CarryableItem item)
    {
        // když hráč sebral nový kus a starý z minula ještě existuje, znič starý
        if (destroyPreviousOnPickup)
        {
            if (_lastPicked != null && _lastPicked != item)
            {
                Destroy(_lastPicked.gameObject);
            }
            _lastPicked = item;
        }
        if (_respawnRoutine != null) StopCoroutine(_respawnRoutine);
        _respawnRoutine = StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnNow();
    }

    static void FreezePhysics(CarryableItem item, bool freeze)
    {
        if (item == null) return;

        var rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (freeze)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;   // nebo Static
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 1f; // pokud máš jiné defaulty, nastav podle sebe
            }
        }

        var col = item.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }
}
