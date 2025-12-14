using UnityEngine;

public class Pistol : WeaponBase
{
    public override void OnPress()
    {
        if (!CanShoot()) return;

        FireOnce();
        ConsumeAmmoAndSetCooldown();
    }

    public override void OnRelease() { }
}


