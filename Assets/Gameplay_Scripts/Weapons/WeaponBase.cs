//using UnityEngine;
//using UnityEngine.InputSystem;

//public abstract class WeaponBase : MonoBehaviour
//{
//    [Header("Info")]
//    public string weaponName = "Silah";

//    public WeaponPickup pickupPrefab;


//    [Header("Refs")]
//    public Transform firePoint;
//    public Rigidbody2D bulletPrefab;
//    public Camera cam;

//    [Header("Bullet")]
//    public float bulletSpeed = 18f;

//    protected virtual void Awake()
//    {
//        if (!cam) cam = Camera.main;
//    }

//    protected void FireOnce()
//    {
//        Vector2 mouseScreen = Mouse.current.position.ReadValue();
//        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z));
//        Vector2 dir = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

//        var b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
//        b.linearVelocity = dir * bulletSpeed;
//    }

//    public abstract void OnPress();
//    public abstract void OnRelease();
//}
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Info")]
    public string weaponName = "Weapon";
    public WeaponPickup pickupPrefab; // sende varsa kalsın

    [Header("Refs")]
    public Transform firePoint;
    public Rigidbody2D bulletPrefab;
    public Camera cam;

    [Header("Combat")]
    public float overloadPerHit = 3f;
    public float maxRange = 25f;

    [Header("Bullet")]
    public float bulletSpeed = 18f;

    [Header("Timing")]
    public float shotCooldown = 0.12f;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int startReserveAmmo = 90;
    public float reloadTime = 1.6f;
    public bool autoReloadOnEmpty = true;

    [Header("Debug")]
    public bool logShootBlock;

    protected int ammoInMag;
    protected int reserveAmmo;
    protected bool isReloading;

    float nextShotTime;
    float reloadEndTime;

    protected virtual void Awake()
    {
        if (!cam) cam = Camera.main;
        ammoInMag = magazineSize;
        reserveAmmo = startReserveAmmo;
    }

    protected virtual void OnEnable()
    {
        // silah tekrar aktif olunca reload/shot state’i düzgün kalsın
    }

    protected virtual void OnDisable()
    {
        // child sınıflar coroutine temizleyecek
    }

    // ✅ Silah kapalı olsa bile inventory burayı çağırabilir (reload timer tamamlamak için)
    public void Tick()
    {
        TickReload();
    }

    void TickReload()
    {
        if (isReloading && Time.time >= reloadEndTime)
            FinishReload();
    }

    protected bool CanShoot()
    {
        TickReload();

        if (isReloading)
        {
            if (logShootBlock) Debug.Log($"{weaponName} blocked: reloading");
            return false;
        }

        if (Time.time < nextShotTime)
        {
            if (logShootBlock) Debug.Log($"{weaponName} blocked: cooldown");
            return false;
        }

        if (ammoInMag > 0) return true;

        if (autoReloadOnEmpty)
            StartReload();

        if (logShootBlock) Debug.Log($"{weaponName} blocked: empty");
        return false;
    }

    protected void ConsumeAmmoAndSetCooldown()
    {
        ammoInMag = Mathf.Max(0, ammoInMag - 1);
        nextShotTime = Time.time + Mathf.Max(0.01f, shotCooldown);

        if (autoReloadOnEmpty && ammoInMag == 0)
            StartReload();
    }

    public void StartReload()
    {
        TickReload();

        if (isReloading) return;
        if (ammoInMag >= magazineSize) return;
        if (reserveAmmo <= 0) return;

        isReloading = true;
        reloadEndTime = Time.time + Mathf.Max(0.01f, reloadTime);
    }

    void FinishReload()
    {
        isReloading = false;

        int need = magazineSize - ammoInMag;
        int take = Mathf.Min(need, reserveAmmo);

        ammoInMag += take;
        reserveAmmo -= take;
    }

    // 🔥 DIŞARIDAN TEK SEFERLİK ATEŞ (anim event / player / inventory burayı çağırabilir)
    public virtual bool TryFire()
    {
        if (!CanShoot()) return false;

        FireOnce();
        ConsumeAmmoAndSetCooldown();
        return true;
    }

    // 🔥 DIŞARIDAN “BASILDI / BIRAKILDI” (full-auto silahlar için)
    public void PressFire() => OnPress();
    public void ReleaseFire() => OnRelease();

    protected void FireOnce()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z));
        Vector2 dir = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        var rb = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        rb.linearVelocity = dir * bulletSpeed;

        // ✅ Bullet scriptin overload/range alıyorsa buradan set et
        var b = rb.GetComponent<Bullet>();
        if (b != null)
        {
            b.overloadAmount = overloadPerHit;
            b.maxRange = maxRange;
        }
    }
    public void AddReserveAmmo(int amount)
    {
        if (amount <= 0) return;
        reserveAmmo += amount;
    }


    // Silahların kendi input davranışı
    public abstract void OnPress();
    public abstract void OnRelease();
}


