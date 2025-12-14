using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponInventory : MonoBehaviour
{
    [Header("Holders")]
    public Transform weaponsHolder;
    public Transform firePoint;

    [Header("Inventory (2 slots)")]
    public WeaponBase slot1;
    public WeaponBase slot2;
    public WeaponBase current;

    [Header("UI")]
    public UIManager messageUI;

    [Header("Anim")]
    public PlayerAnimDriver anim;

    [Header("Shoot Anim Repeat")]
    [Tooltip("Basılı tutarken shoot anim trigger tekrar aralığı. Silah ateş hızını değiştirmez; sadece animasyon.")]
    public float shootAnimInterval = 0.12f;

    private WeaponPickup nearbyPickup;

    private bool wasHeld;
    private float nextShootAnimTime;

    void Start()
    {
        Equip(null);
        RefreshUI();
        if (!anim) anim = GetComponent<PlayerAnimDriver>();
    }

    void Update()
    {
        // ✅ Silahlar aktif olmasa bile reload/cooldown state güncellensin
        if (slot1 != null) slot1.Tick();
        if (slot2 != null) slot2.Tick();

        // ===== FIRE INPUT + SHOOT ANIM (silaha karışmadan) =====
        bool held = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool pressedThisFrame = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool releasedThisFrame = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;

        if (current != null)
        {
            if (pressedThisFrame)
            {
                // Silahın kendi sistemi
                current.OnPress();

                // Anim: tıkla -> 1 kere
                if (anim) anim.TriggerShoot();

                // Basılı tutma için schedule
                nextShootAnimTime = Time.time + shootAnimInterval;
            }

            if (held && wasHeld && Time.time >= nextShootAnimTime)
            {
                // Anim: basılı tut -> tekrar tekrar
                if (anim) anim.TriggerShoot();
                nextShootAnimTime = Time.time + shootAnimInterval;
            }

            if (releasedThisFrame)
            {
                // Silahın kendi sistemi
                current.OnRelease();
            }
        }

        wasHeld = held;

        // Reload
        if (current != null && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            current.StartReload();
        }

        // Slot değiştirme
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) Equip(slot1);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) Equip(slot2);

            // Yerdeki silahla değiştir / al
            if (nearbyPickup != null && Keyboard.current.gKey.wasPressedThisFrame)
            {
                TakeOrSwap(nearbyPickup);
            }
        }
    }

    void TakeOrSwap(WeaponPickup pickup)
    {
        if (slot1 == null)
        {
            slot1 = SpawnWeapon(pickup.weaponPrefab);
            Equip(slot1);
            Destroy(pickup.gameObject);
            return;
        }

        if (slot2 == null)
        {
            slot2 = SpawnWeapon(pickup.weaponPrefab);
            Equip(slot2);
            Destroy(pickup.gameObject);
            return;
        }

        if (current == null)
        {
            Equip(slot1);
        }

        // Yere bırak
        DropCurrentAsPickup(pickup.transform.position);

        // Mevcut silahı inventory’den çıkar
        RemoveFromSlots(current);
        Destroy(current.gameObject);
        current = null;

        // Yeni silahı al
        var newWpn = SpawnWeapon(pickup.weaponPrefab);
        PutIntoFirstEmptySlot(newWpn);
        Equip(newWpn);

        Destroy(pickup.gameObject);
    }

    WeaponBase SpawnWeapon(WeaponBase prefab)
    {
        var w = Instantiate(prefab, weaponsHolder);
        w.firePoint = firePoint;
        if (!w.cam) w.cam = Camera.main;

        // ✅ Elde silah sprite'ı görünmesin (pickup sprite'ı ayrı kalacak)
        foreach (var sr in w.GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = false;

        w.gameObject.SetActive(false);
        return w;
    }

    void PutIntoFirstEmptySlot(WeaponBase w)
    {
        if (slot1 == null) slot1 = w;
        else if (slot2 == null) slot2 = w;
        else slot1 = w;
    }

    void Equip(WeaponBase w)
    {
        if (current != null)
        {
            current.OnRelease();
            current.gameObject.SetActive(false);
        }

        current = w;

        if (current != null)
        {
            current.gameObject.SetActive(true);
            current.Tick();
        }

        RefreshUI();

        // equip olunca basılı mouse state sapıtmasın
        wasHeld = false;
        nextShootAnimTime = 0f;
    }

    void RemoveFromSlots(WeaponBase w)
    {
        if (slot1 == w) slot1 = null;
        if (slot2 == w) slot2 = null;
    }

    void DropCurrentAsPickup(Vector3 position)
    {
        if (current == null) return;
        if (current.pickupPrefab == null) return;

        Instantiate(current.pickupPrefab, position, Quaternion.identity);
    }

    void RefreshUI()
    {
        if (!messageUI) return;

        string s1 = slot1 ? slot1.weaponName : "";
        string s2 = slot2 ? slot2.weaponName : "";

        int active = 0;
        if (current != null && current == slot1) active = 1;
        else if (current != null && current == slot2) active = 2;

        messageUI.SetSlots(s1, s2, active);

        if (nearbyPickup != null)
        {
            bool willSwap = (slot1 != null && slot2 != null);
            string verb = willSwap ? "değiştir" : "al";
            messageUI.SetPickupPrompt($"G: {nearbyPickup.pickupName} {verb}");
        }
        else
        {
            messageUI.SetPickupPrompt("");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<WeaponPickup>();
        if (p != null) nearbyPickup = p;
        RefreshUI();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var p = other.GetComponent<WeaponPickup>();
        if (p != null && nearbyPickup == p) nearbyPickup = null;
        RefreshUI();
    }
}
