using UnityEngine;

public class TutorialEndTrigger : MonoBehaviour
{
    [Header("Object to move")]
    [SerializeField] Transform objectToMove;

    [Header("Move destination")]
    [SerializeField] Transform destination;

    bool used;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;

        if (objectToMove == null || destination == null)
        {
            Debug.LogError("Missing reference on TutorialEndTrigger", this);
            return;
        }

        used = true;

        objectToMove.position = destination.position;

        Destroy(gameObject);
    }
}
