using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 15;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Player objesi altýnda aktif silahý bul
        var weapon = other.GetComponentInChildren<WeaponBase>();
        if (weapon != null)
        {
            weapon.AddReserveAmmo(ammoAmount);
            Destroy(gameObject);
        }
    }
}
