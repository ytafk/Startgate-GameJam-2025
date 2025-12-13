using UnityEngine;
using UnityEngine.InputSystem;

public class SniperRifle : WeaponBase
{
    [Header("Sniper (Hitscan)")]
    public LayerMask hitMask;          // sadece düşman layer'ları
    public bool penetrate = true;      // içinden geçsin mi
    public int maxPenetrations = 999;  // kaç hedefe kadar geçsin

    public override void OnPress()
    {
        // ✅ Ammo / Reload / Cooldown kontrolü WeaponBase’te
        if (!CanShoot()) return;

        FireHitscan();
        ConsumeAmmoAndSetCooldown();
    }

    public override void OnRelease() { }

    private void FireHitscan()
    {
        // Mouse yönünü hesapla
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(
            mouseScreen.x, mouseScreen.y, -cam.transform.position.z));

        Vector2 dir = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        // Tüm isabetleri al (düşmanların içinden geçecek)
        RaycastHit2D[] hits = Physics2D.RaycastAll(firePoint.position, dir, maxRange, hitMask);

        // Yakından uzağa sırala
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        int penetrated = 0;

        foreach (var h in hits)
        {
            if (h.collider == null) continue;

            // Enemy scriptin (overload bar yapan)
            EnemyRobot enemy = h.collider.GetComponentInParent<EnemyRobot>();
            if (enemy != null)
            {
                enemy.AddOverload(overloadPerHit);
                penetrated++;

                if (!penetrate || penetrated >= maxPenetrations)
                    break;
            }
        }

        Debug.DrawRay(firePoint.position, dir * maxRange, Color.cyan, 0.15f);
    }
}
