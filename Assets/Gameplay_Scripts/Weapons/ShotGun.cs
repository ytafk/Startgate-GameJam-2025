using UnityEngine;
using UnityEngine.InputSystem;

public class ShotGun : WeaponBase
{
    [Header("Shotgun (Inspector)")]
    public int pelletCount = 8;        // aynı anda çıkan mermi sayısı
    public float spreadAngle = 18f;    // toplam yayılma (derece)
    public float pelletSpeedMultiplier = 1f; // istersen 0.9 yap

    public override void OnPress()
    {
        if (!CanShoot()) return;

        FireShotgun();
        ConsumeAmmoAndSetCooldown();
    }

    public override void OnRelease() { }

    private void FireShotgun()
    {
        // Mouse yönünü hesapla (WeaponBase.FireOnce içindeki mantıkla aynı)
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z));
        Vector2 baseDir = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        int count = Mathf.Max(1, pelletCount);
        float half = spreadAngle * 0.5f;

        for (int i = 0; i < count; i++)
        {
            // Daha doğal dağılım istersen: Random.Range(-half, half)
            float t = (count == 1) ? 0.5f : (i / (count - 1f));
            float angle = Mathf.Lerp(-half, half, t);

            Vector2 dir = Rotate(baseDir, angle);

            var b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            b.linearVelocity = dir * (bulletSpeed * pelletSpeedMultiplier);

            // Eğer Bullet scriptinde overload/range set ediyorsan burada da set et:
            // var bullet = b.GetComponent<Bullet>();
            // if (bullet) bullet.Configure(overloadPerHit, maxRange);
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
