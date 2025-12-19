using UnityEngine;

public class DeathTrap : MonoBehaviour
{
    [SerializeField] PlayerMovement player;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (player == null) return;

        if (other.transform.IsChildOf(player.transform))
        {
            player.Respawn();
        }
    }
}
