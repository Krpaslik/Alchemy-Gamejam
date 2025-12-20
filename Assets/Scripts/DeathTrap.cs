using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathTrap : MonoBehaviour
{
    void Reset()
    {
        // pojistka: trap musí být trigger
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // najde PlayerMovement i když collider patří child objektu
        var player = other.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        player.Respawn();
    }
}
