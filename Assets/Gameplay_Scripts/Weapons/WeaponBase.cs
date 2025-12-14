using UnityEngine;
using UnityEngine.InputSystem;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Info")]
    public string weaponName = "Silah";
    public WeaponPickup pickupPrefab;

    [Header("Combat")]
    public float overloadPerHit = 3f;
    public float maxRange = 25f;
    public float shotCooldown = 0.2f;

    [Header("Ammo")]
    public int magazineSize = 12;
    public int startReserveAmmo = 48;
    public float reloadTime = 1.2f;
    public bool autoReloadOnEmpty = true;

    [Header("Refs")]
    public Transform firePoint;
    public Rigidbody2D bulletPrefab;
    public Camera cam;

    [Header("Bullet Stats")]
    public float bulletSpeed = 18f;

    [Header("Debug")]
    public bool logShootBlock;

    // =========================================================
    // DÜZELTİLEN KISIM BURASI (protected -> public yapıldı)
    // =========================================================
    [HideInInspector] public int ammoInMag;
    [HideInInspector] public int reserveAmmo;
    protected bool isReloading;
    // =========================================================

    float nextShotTime;
    float reloadEndTime;

    protected virtual void Awake()
    {
        if (!cam) cam = Camera.main;

        // Başlangıç mermilerini ayarla
        ammoInMag = magazineSize;
        reserveAmmo = startReserveAmmo;
    }

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }

    protected virtual void Update()
    {
        TickReload();
    }

    public void Tick()
    {
        TickReload();
    }

    void TickReload()
    {
        if (isReloading && Time.time >= reloadEndTime)
        {
            FinishReload();
        }
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

    public void AddReserveAmmo(int amount)
    {
        if (amount <= 0) return;
        reserveAmmo += amount;
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

    protected void FireOnce()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z));
        Vector2 dir = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        var rb = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Unity 6+ için linearVelocity, eski sürümler için velocity
        rb.linearVelocity = dir * bulletSpeed;

        var b = rb.GetComponent<Bullet>();
        if (b != null)
        {
            b.overloadAmount = overloadPerHit;
            b.maxRange = maxRange;
        }
    }

    public virtual bool TryFire()
    {
        if (!CanShoot()) return false;

        FireOnce();
        ConsumeAmmoAndSetCooldown();
        return true;
    }

    public void PressFire() => OnPress();
    public void ReleaseFire() => OnRelease();

    public abstract void OnPress();
    public abstract void OnRelease();
}