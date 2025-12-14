
using System.Collections;
using UnityEngine;

public class Rifle : WeaponBase
{
    [Header("Rifle")]
    public float holdAutoDelay = 0.10f;

    bool holding;
    Coroutine loop;

    public override void OnPress()
    {
        holding = true;

        // basınca hemen 1 mermi
        TryFire();

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

    IEnumerator HoldLoop()
    {
        if (holdAutoDelay > 0f)
            yield return new WaitForSeconds(holdAutoDelay);

        while (holding)
        {
            TryFire();
            yield return null;
        }

        loop = null;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        holding = false;
        if (loop != null) { StopCoroutine(loop); loop = null; }
    }
}



