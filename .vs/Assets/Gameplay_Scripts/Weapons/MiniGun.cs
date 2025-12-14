using System.Collections;
using UnityEngine;

public class MiniGun2D : WeaponBase
{
    bool holding;
    Coroutine loop;

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

    IEnumerator FireLoop()
    {
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
