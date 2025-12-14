using UnityEngine;

public class Pistol : WeaponBase
{
    public override void OnPress()
    {
        // tek týk = tek mermi
        TryFire();
    }

    public override void OnRelease() { }
}



