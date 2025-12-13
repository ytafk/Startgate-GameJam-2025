using System.Collections;
using UnityEngine;

public class Minigun : WeaponBase
{
    [Header("Minigun")]
    public float fireRate = 15f; // saniyedeki mermi (Inspector)

    private bool holding;
    private Coroutine loop;

    protected override void Awake()
    {
        base.Awake();
        SyncCooldownFromFireRate();
    }

    void OnValidate()
    {
        // Inspector'da fireRate değişince shotCooldown otomatik güncellensin
        SyncCooldownFromFireRate();
    }

    private void SyncCooldownFromFireRate()
    {
        // WeaponBase'deki shotCooldown'ı minigun fireRate'e göre ayarla
        shotCooldown = 1f / Mathf.Max(0.01f, fireRate);
    }

    public override void OnPress()
    {
        holding = true;

        if (loop == null)
            loop = StartCoroutine(FireLoop());
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

    private IEnumerator FireLoop()
    {
        while (holding)
        {
            // ✅ ammo/cooldown/reload kontrolü WeaponBase'te
            if (CanShoot())
            {
                FireOnce();
                ConsumeAmmoAndSetCooldown();
            }

            // cooldown'u WeaponBase takip ediyor; her frame kontrol etmek yeterli
            yield return null;
        }

        loop = null;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Slot değişiminde coroutine kilitlenmesin
        holding = false;

        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
    }
}
