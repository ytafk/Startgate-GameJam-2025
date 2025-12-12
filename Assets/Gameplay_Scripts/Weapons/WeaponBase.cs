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
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Info")]
    public string weaponName = "Silah";
    public WeaponPickup pickupPrefab;

    [Header("Combat (Inspector)")]
    public float overloadPerHit = 3f;     // düşmana eklenecek overload (hasar gibi düşün)
    public float maxRange = 25f;          // merminin/hitscan'in max menzili
    public float shotCooldown = 0.2f;     // atışlar arası bekleme (silaha göre değiştir)

    [Header("Ammo (Inspector)")]
    public int magazineSize = 12;         // şarjör kapasitesi
    public int startReserveAmmo = 48;     // yerden alınca yedek mermi
    public float reloadTime = 1.2f;       // şarjör değiştirme süresi
    public bool autoReloadOnEmpty = true; // mermi bitince otomatik reload başlasın mı?

    [Header("Refs")]
    public Transform firePoint;
    public Rigidbody2D bulletPrefab;
    public Camera cam;

    [Header("Bullet (Inspector)")]
    public float bulletSpeed = 18f;

    // Runtime state (hilelenmesin diye silah instance'ında durur)
    [HideInInspector] public int ammoInMag;
    [HideInInspector] public int reserveAmmo;
    [HideInInspector] public bool isReloading;

    float nextShotTime;
    float reloadEndTime;

    protected virtual void Awake()
    {
        if (!cam) cam = Camera.main;

        // Silah instantiate edilince başlangıç mermileri
        ammoInMag = magazineSize;
        reserveAmmo = startReserveAmmo;
    }

    protected virtual void Update()
    {
        TickReload();
    }

    public void StartReload()
    {
        TickReload();

        if (isReloading) return;
        if (ammoInMag >= magazineSize) return;
        if (reserveAmmo <= 0) return;

        isReloading = true;
        reloadEndTime = Time.time + reloadTime;
    }

    void TickReload()
    {
        if (!isReloading) return;

        if (Time.time >= reloadEndTime)
        {
            int need = magazineSize - ammoInMag;
            int take = Mathf.Min(need, reserveAmmo);

            ammoInMag += take;
            reserveAmmo -= take;
            isReloading = false;
        }
    }
    // Inventory, silah kapalı olsa bile bunu çağıracak
    public void Tick()
    {
        TickReload();  // sende reload'u bitiren fonksiyon adı neyse onu çağır
    }

    protected bool CanShoot()
    {
        TickReload();

        if (isReloading) return false;
        if (Time.time < nextShotTime) return false;

        if (ammoInMag <= 0)
        {
            if (autoReloadOnEmpty) StartReload();
            return false;
        }

        return true;
    }

    protected void ConsumeAmmoAndSetCooldown()
    {
        ammoInMag = Mathf.Max(0, ammoInMag - 1);
        nextShotTime = Time.time + Mathf.Max(0.01f, shotCooldown);
    }

    protected void FireOnce()
    {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -cam.transform.position.z));
        Vector2 dir = ((Vector2)mouseWorld - (Vector2)firePoint.position).normalized;

        var b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        b.linearVelocity = dir * bulletSpeed;

        // ✅ ÖNEMLİ:
        // Bullet scriptinde overload/range destekliyorsan burada set et:
        // var bullet = b.GetComponent<Bullet>();
        // if (bullet) bullet.Configure(overloadPerHit, maxRange);
    }

    // input
    public abstract void OnPress();
    public abstract void OnRelease();

    protected virtual void OnDisable()
    {
        // Slot değişiminde reload/cooldown state'i bozulmasın diye
        // isReloading aynen kalsın (bitince TickReload tamamlar)
    }
}


