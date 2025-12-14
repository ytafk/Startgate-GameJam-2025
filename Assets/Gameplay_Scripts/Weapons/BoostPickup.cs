using UnityEngine;

public class BoostPickup : MonoBehaviour
{
    [Header("Boost Values")]
    public float speedMultiplier = 1.5f;
    public float dashCooldownMultiplier = 0.6f;
    public float duration = 10f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var boost = other.GetComponent<PlayerBoost>();
        if (boost == null) boost = other.gameObject.AddComponent<PlayerBoost>();

        boost.ActivateBoost(speedMultiplier, dashCooldownMultiplier, duration);

        Destroy(gameObject);
    }
}
