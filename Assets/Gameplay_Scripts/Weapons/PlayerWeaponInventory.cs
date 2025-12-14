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
    public UIManager messageUI; // Eski text tabanlı UI

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
        // Başlangıçta animasyon sürücüsünü al
        if (!anim) anim = GetComponent<PlayerAnimDriver>();

        Equip(null);
        RefreshUI();
    }

    void Update()
    {
        // ✅ Silahlar aktif olmasa bile reload/cooldown state güncellensin
        if (slot1 != null) slot1.Tick();
        if (slot2 != null) slot2.Tick();

        // ===== FIRE INPUT + SHOOT ANIM (Script 2'den gelen gelişmiş mantık) =====
        // Input System kontrolleri
        bool held = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool pressedThisFrame = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool releasedThisFrame = Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;

        if (current != null)
        {
            // 1. Tıklama Anı
            if (pressedThisFrame)
            {
                // Silahın kendi ateşleme sistemi
                current.OnPress();

                // Animasyon: Tek tıkla ateşleme
                if (anim) anim.TriggerShoot();

                // Basılı tutma için zamanlayıcıyı ayarla
                nextShootAnimTime = Time.time + shootAnimInterval;
            }

            // 2. Basılı Tutma Anı (Sürekli Ateş Eden Silahlar İçin Animasyon Tekrarı)
            if (held && wasHeld && Time.time >= nextShootAnimTime)
            {
                // Animasyon: Tekrar tetikle
                if (anim) anim.TriggerShoot();
                nextShootAnimTime = Time.time + shootAnimInterval;
            }

            // 3. Bırakma Anı
            if (releasedThisFrame)
            {
                current.OnRelease();
            }
        }

        wasHeld = held;

        // ===== DİĞER KONTROLLER =====

        // Reload (R Tuşu)
        if (current != null && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            current.StartReload();
        }

        if (Keyboard.current != null)
        {
            // Slot Değiştirme (1 ve 2 Tuşları)
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
        // Slot 1 boşsa oraya al
        if (slot1 == null)
        {
            slot1 = SpawnWeapon(pickup.weaponPrefab);
            Equip(slot1);
            Destroy(pickup.gameObject);
            return;
        }

        // Slot 2 boşsa oraya al
        if (slot2 == null)
        {
            slot2 = SpawnWeapon(pickup.weaponPrefab);
            Equip(slot2);
            Destroy(pickup.gameObject);
            return;
        }

        // İki slot da doluysa:
        // Eğer elimizde silah yoksa (ama slotlar doluysa - nadir durum) 1'i seç
        if (current == null)
        {
            Equip(slot1);
        }

        // 1. Mevcut silahı yere at (Pickup oluştur)
        DropCurrentAsPickup(pickup.transform.position);

        // 2. Mevcut silahı inventory'den sil ve yok et
        RemoveFromSlots(current);
        Destroy(current.gameObject);
        current = null;

        // 3. Yeni silahı oluştur ve boşalan slota koy
        var newWpn = SpawnWeapon(pickup.weaponPrefab);
        PutIntoFirstEmptySlot(newWpn);

        // 4. Yeni silahı kuşan
        Equip(newWpn);

        // Yerdeki kutuyu yok et
        Destroy(pickup.gameObject);
    }

    WeaponBase SpawnWeapon(WeaponBase prefab)
    {
        var w = Instantiate(prefab, weaponsHolder);
        w.firePoint = firePoint;
        if (!w.cam) w.cam = Camera.main;

        // ✅ Script 1'den gelen özellik: Elde silah sprite'ı görünmesin
        foreach (var sr in w.GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = false;

        w.gameObject.SetActive(false);
        return w;
    }

    // ---------------------------------------------------------
    // EQUIP (BİRLEŞTİRİLEN KRİTİK NOKTA)
    // ---------------------------------------------------------
    void Equip(WeaponBase w)
    {
        // Eski silahı kapat
        if (current != null)
        {
            current.OnRelease();
            current.gameObject.SetActive(false);
        }

        current = w;

        // Yeni silahı aç
        if (current != null)
        {
            current.gameObject.SetActive(true);
            current.Tick();
        }

        // >>>>> Script 1'den Eklenen Özellik: İkon Güncelleme <<<<<
        if (WeaponUIManager.Instance != null)
        {
            WeaponUIManager.Instance.UpdateCurrentWeapon(current);
        }
        // >>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        RefreshUI();

        // Equip olunca basılı tutma durumunu sıfırla (bug önleme)
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