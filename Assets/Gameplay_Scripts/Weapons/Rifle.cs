using System.Collections;
using UnityEngine;

public class Rifle : WeaponBase
{
    [Header("Rifle (Inspector)")]
    public float fireRate = 7f;          // rifle full-auto hızı (SMG/minigun'dan yavaş)
    public float holdAutoDelay = 0.10f;  // basılı tutunca otomatik başlamadan önce küçük gecikme

    private bool holding;
    private Coroutine loop;

    protected override void Awake()
    {
        base.Awake();
        SyncCooldownFromFireRate();
    }

    void OnValidate()
    {
        SyncCooldownFromFireRate();
    }

    private void SyncCooldownFromFireRate()
    {
        // Rifle'ın "atışlar arası bekleme" süresini fireRate'ten türetiyoruz
        shotCooldown = 1f / Mathf.Max(0.01f, fireRate);
    }

    public override void OnPress()
    {
        holding = true;

        // ✅ Basınca hemen 1 mermi (semi-auto)
        TryShootOnce();

        // Basılı tutunca full-auto devam
        if (loop == null)
            loop = StartCoroutine(HoldLoop());
    }

    public override void OnRelease()
    {
        holding = false;

        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
    }

    private IEnumerator HoldLoop()
    {
        if (holdAutoDelay > 0f)
            yield return new WaitForSeconds(holdAutoDelay);

        while (holding)
        {
            TryShootOnce();
            yield return null; // WeaponBase cooldown zaten Time.time ile kontrol ediyor
        }

        loop = null;
    }

    private void TryShootOnce()
    {
        if (!CanShoot()) return;

        FireOnce();
        ConsumeAmmoAndSetCooldown();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // slot değişiminde coroutine kilitlenmesin
        holding = false;
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
    }
}


