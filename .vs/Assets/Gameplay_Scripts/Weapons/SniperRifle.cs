using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SniperRifle : WeaponBase
{
    [Header("Hitscan")]
    public LayerMask hitMask;
    public bool penetrate = true;
    public int maxPenetrations = 999;

    [Header("Visual")]
    public LineRenderer line;
    public float lineDuration = 0.05f;
    public float lineWidth = 0.02f; // ince çizgi

    protected override void Awake()
    {
        base.Awake(); // ✅ WeaponBase cam/ammo init bozulmasın
        if (!line) line = GetComponent<LineRenderer>();

        // Çizgiyi ince yap (istersen inspector'dan da override edebilirsin)
        if (line)
        {
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.enabled = false;
        }
    }

    public override void OnPress() => TryFire();
    public override void OnRelease() { }

    // ✅ Hitscan olduğu için TryFire override
    public override bool TryFire()
    {
        if (!CanShoot()) return false;

        FireHitscan();
        ConsumeAmmoAndSetCooldown();
        return true;
    }

    void FireHitscan()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(
            mouseScreen.x, mouseScreen.y, -cam.transform.position.z));

        Vector2 dir = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        RaycastHit2D[] hits = Physics2D.RaycastAll(firePoint.position, dir, maxRange, hitMask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // ✅ Görsel uç nokta:
        // penetrate=true -> maxRange’e kadar
        // penetrate=false -> ilk çarpana kadar
        Vector2 endPoint = (Vector2)firePoint.position + dir * maxRange;
        if (!penetrate)
        {
            if (hits.Length > 0 && hits[0].collider != null)
                endPoint = hits[0].point;
        }

        ShowLine(firePoint.position, endPoint);

        // ✅ Hasar / overload: delip geçme mantığı
        int penetrated = 0;
        foreach (var h in hits)
        {
            if (h.collider == null) continue;

            var enemy = h.collider.GetComponentInParent<EnemyRobot>();
            if (enemy != null)
            {
                enemy.AddOverload(overloadPerHit);
                penetrated++;

                if (!penetrate || penetrated >= maxPenetrations)
                    break;
            }
        }
    }

    void ShowLine(Vector2 start, Vector2 end)
    {
        if (!line) return;

        StopAllCoroutines();
        StartCoroutine(LineRoutine(
            new Vector3(start.x, start.y, 0f),
            new Vector3(end.x, end.y, 0f)
        ));
    }

    IEnumerator LineRoutine(Vector3 start, Vector3 end)
    {
        line.enabled = true;
        line.SetPosition(0, start);
        line.SetPosition(1, end);

        yield return new WaitForSeconds(lineDuration);

        line.enabled = false;
    }
}
