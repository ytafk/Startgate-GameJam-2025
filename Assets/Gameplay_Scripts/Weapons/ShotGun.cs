using UnityEngine;
using UnityEngine.InputSystem;

public class Shotgun : WeaponBase
{
    [Header("Shotgun")]
    public int pelletCount = 8;              // aynı anda çıkan pellet sayısı
    public float spreadAngle = 18f;          // toplam yayılma açısı (derece)
    public float pelletSpeedMultiplier = 1f; // istersen 0.9 gibi

    public override void OnPress()
    {
        // ✅ Ammo / Reload / Cooldown kontrolü WeaponBase’te
        if (!CanShoot()) return;

        FireShotgun();
        ConsumeAmmoAndSetCooldown();
    }

    public override void OnRelease() { }

    private void FireShotgun()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(
            mouseScreen.x, mouseScreen.y, -cam.transform.position.z));

        Vector2 baseDir = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        int count = Mathf.Max(1, pelletCount);
        float half = spreadAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0.5f : (i / (count - 1f));
            float angle = Mathf.Lerp(-half, half, t);

            Vector2 dir = Rotate(baseDir, angle);

            var rb = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            rb.linearVelocity = dir * (bulletSpeed * pelletSpeedMultiplier);

            // ✅ Silahın overload + range değerlerini mermiye aktar
            var bullet = rb.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.overloadAmount = overloadPerHit;
                bullet.maxRange = maxRange;
            }
        }
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }
}
