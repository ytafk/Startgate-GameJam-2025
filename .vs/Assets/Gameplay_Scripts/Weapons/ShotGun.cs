using UnityEngine;
using UnityEngine.InputSystem;

public class Shotgun : WeaponBase
{
    [Header("Shotgun")]
    public int pelletCount = 8;
    public float spreadAngle = 18f;
    public float pelletSpeedMultiplier = 1f;

    public override void OnPress()
    {
        if (!CanShoot()) return;

        FireShotgun();
        ConsumeAmmoAndSetCooldown();
    }

    public override void OnRelease() { }

    void FireShotgun()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z));
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

            var b = rb.GetComponent<Bullet>();
            if (b != null)
            {
                b.overloadAmount = overloadPerHit;
                b.maxRange = maxRange;
            }
        }
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }
}
