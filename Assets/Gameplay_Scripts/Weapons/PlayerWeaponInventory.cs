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

    [Header("UI (Old/Text)")]
    public UIManager messageUI;

    [Header("Anim")]
    public PlayerAnimDriver anim;

    [Header("Shoot Anim Repeat")]
    [Tooltip("Basılı tutarken shoot anim trigger tekrar aralığı.")]
    public float shootAnimInterval = 0.12f;

    private WeaponPickup nearbyPickup;
    private bool wasHeld;
    private float nextShootAnimTime;

    void Start()
    {
        if (!anim) anim = GetComponent<PlayerAnimDriver>();

        Equip(null);
        RefreshUI();
    }

    void Update()
    {
        if (slot1 != null) slot1.Tick();
        if (slot2 != null) slot2.Tick();

        // ===== FIRE INPUT + SHOOT ANIM =====
        bool held = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool pressedThisFrame = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool releasedThisFrame = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;

        if (current != null)
        {
            // 1. Tıklama
            if (pressedThisFrame)
            {
                current.OnPress();
                if (anim) anim.TriggerShoot();
                nextShootAnimTime = Time.time + shootAnimInterval;
            }

            // 2. Basılı Tutma
            if (held && wasHeld && Time.time >= nextShootAnimTime)
            {
                if (anim) anim.TriggerShoot();
                nextShootAnimTime = Time.time + shootAnimInterval;
            }

            // 3. Bırakma
            if (releasedThisFrame)
            {
                current.OnRelease();
            }
        }

        wasHeld = held;

        // ===== DİĞER KONTROLLER =====

        // Reload
        if (current != null && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            current.StartReload();
        }

        if (Keyboard.current != null)
        {
            // Slot Değiştirme
            if (Keyboard.current.digit1Key.wasPressedThisFrame) Equip(slot1);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) Equip(slot2);

            // Yerdeki Silahı Al / Değiştir (G Tuşu)
            if (nearbyPickup != null && Keyboard.current.gKey.wasPressedThisFrame)
            {
                TakeOrSwap(nearbyPickup);
            }
        }
    }

    // ---------------------------------------------------------
    // ENVANTER YÖNETİMİ
    // ---------------------------------------------------------

    void TakeOrSwap(WeaponPickup pickup)
    {
        // --- SLOT 1 BOŞSA ---
        if (slot1 == null)
        {
            slot1 = SpawnWeapon(pickup.weaponPrefab);

            // ✅ Mermileri kutudan silaha aktar
            slot1.LoadAmmo(pickup.savedAmmoInMag, pickup.savedReserveAmmo);

            Equip(slot1);
            Destroy(pickup.gameObject);
            return;
        }

        // --- SLOT 2 BOŞSA ---
        if (slot2 == null)
        {
            slot2 = SpawnWeapon(pickup.weaponPrefab);

            // ✅ Mermileri kutudan silaha aktar
            slot2.LoadAmmo(pickup.savedAmmoInMag, pickup.savedReserveAmmo);

            Equip(slot2);
            Destroy(pickup.gameObject);
            return;
        }

        // --- İKİSİ DE DOLUYSA DEĞİŞTİR ---

        if (current == null) Equip(slot1);

        // 1. Mevcut silahı yere at (ve mermisini kaydet)
        DropCurrentAsPickup(pickup.transform.position);

        // 2. Mevcut silahı sil
        RemoveFromSlots(current);
        Destroy(current.gameObject);
        current = null;

        // 3. Yeni silahı oluştur
        var newWpn = SpawnWeapon(pickup.weaponPrefab);

        // ✅ Mermileri kutudan yeni silaha aktar
        newWpn.LoadAmmo(pickup.savedAmmoInMag, pickup.savedReserveAmmo);

        PutIntoFirstEmptySlot(newWpn);
        Equip(newWpn);

        // Yerdeki kutuyu yok et
        Destroy(pickup.gameObject);
    }

    WeaponBase SpawnWeapon(WeaponBase prefab)
    {
        var w = Instantiate(prefab, weaponsHolder);
        w.firePoint = firePoint;
        if (!w.cam) w.cam = Camera.main;

        foreach (var sr in w.GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = false;

        w.gameObject.SetActive(false);
        return w;
    }

    // ---------------------------------------------------------
    // EQUIP
    // ---------------------------------------------------------
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

        // İkon Güncelleme
        if (WeaponUIManager.Instance != null)
        {
            WeaponUIManager.Instance.UpdateCurrentWeapon(current);
        }

        RefreshUI();

        wasHeld = false;
        nextShootAnimTime = 0f;
    }

    // ---------------------------------------------------------
    // YARDIMCI METOTLAR
    // ---------------------------------------------------------

    void PutIntoFirstEmptySlot(WeaponBase w)
    {
        if (slot1 == null) slot1 = w;
        else if (slot2 == null) slot2 = w;
        else slot1 = w;
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

        // Yere kutuyu oluştur
        var droppedObj = Instantiate(current.pickupPrefab, position, Quaternion.identity);
        var pickupScript = droppedObj.GetComponent<WeaponPickup>();

        // ✅ MEVCUT SİLAHIN MERMİSİNİ YERDEKİ KUTUYA YAZ
        if (pickupScript != null)
        {
            pickupScript.SetAmmoData(current.ammoInMag, current.reserveAmmo);
        }
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