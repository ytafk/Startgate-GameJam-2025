using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public float healAmount = 20f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
