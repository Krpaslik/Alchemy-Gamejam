using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    [SerializeField] GameObject winText; // root objekt s TMP

    bool used = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;

        used = true;

        if (winText != null)
            winText.SetActive(true);

        // pokud má být jednorázový
        Destroy(gameObject);
    }
}
