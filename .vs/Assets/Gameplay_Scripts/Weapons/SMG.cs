using System.Collections;
using UnityEngine;

public class SMG : WeaponBase
{
    [Header("Burst")]
    public int burstCount = 4;
    public float burstRate = 0.05f;
    public float burstCooldown = 0.35f;

    bool bursting;
    bool canBurst = true;
    Coroutine co;

    public override void OnPress()
    {
        if (!canBurst || bursting) return;

        if (co != null) { StopCoroutine(co); co = null; }
        co = StartCoroutine(BurstRoutine());
    }

    public override void OnRelease() { }

    IEnumerator BurstRoutine()
    {
        bursting = true;
        canBurst = false;

        int count = Mathf.Max(1, burstCount);

        for (int i = 0; i < count; i++)
        {
            // her pellet gibi: tek tek CanShoot kontrolü
            if (!TryFire()) break;
            yield return new WaitForSeconds(Mathf.Max(0.01f, burstRate));
        }

        bursting = false;

        yield return new WaitForSeconds(Mathf.Max(0.01f, burstCooldown));
        canBurst = true;

        co = null;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        bursting = false;
        canBurst = true;
        if (co != null) { StopCoroutine(co); co = null; }
    }
}

