using System.Collections;
using UnityEngine;

public class SMG : WeaponBase
{
    [Header("SMG (Burst)")]
    public int burstCount = 4;           // Burst içindeki mermi sayısı
    public float burstRate = 0.05f;      // Burst içi hız (çok hızlı)
    public float burstCooldown = 0.35f;  // Burst sonrası bekleme

    private bool isBursting;
    private bool canBurst = true;

    private Coroutine burstCoroutine;

    protected override void Awake()
    {
        base.Awake();
        SyncCooldownFromBurstRate();
    }

    void OnValidate()
    {
        SyncCooldownFromBurstRate();
    }

    private void SyncCooldownFromBurstRate()
    {
        // WeaponBase'in atış arası beklemesini burstRate'e eşitle
        shotCooldown = Mathf.Max(0.01f, burstRate);
    }

    public override void OnPress()
    {
        if (!canBurst || isBursting) return;

        // Basınca anında burst başlat
        if (burstCoroutine != null)
        {
            StopCoroutine(burstCoroutine);
            burstCoroutine = null;
        }

        burstCoroutine = StartCoroutine(BurstRoutine());
    }

    public override void OnRelease()
    {
        // Burst silahında release şart değil
    }

    private IEnumerator BurstRoutine()
    {
        isBursting = true;
        canBurst = false;

        int shotsFired = 0;

        for (int i = 0; i < burstCount; i++)
        {
            // ✅ Ammo/reload/cooldown kontrolü WeaponBase'te
            if (!CanShoot())
                break;

            FireOnce();
            ConsumeAmmoAndSetCooldown();
            shotsFired++;

            // burstRate zaten shotCooldown'a eşit, yine de burst hissi için bekletiyoruz
            yield return new WaitForSeconds(burstRate);
        }

        isBursting = false;

        // Burst sonrası bekleme
        yield return new WaitForSeconds(burstCooldown);
        canBurst = true;

        burstCoroutine = null;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (burstCoroutine != null)
        {
            StopCoroutine(burstCoroutine);
            burstCoroutine = null;
        }

        isBursting = false;
        canBurst = true;
    }
}
