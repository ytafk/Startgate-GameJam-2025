using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponInventory : MonoBehaviour
{
    [Header("Holders")]
    public Transform weaponsHolder;   
    public Transform firePoint;       

    [Header("UI")]
    

    [Header("Inventory (2 slots)")]
    public WeaponBase slot1;
    public WeaponBase slot2;
    public WeaponBase current;

    private WeaponPickup nearbyPickup;
    public UIManager messageUI;

    void Start()
    {
        Equip(null); 
        RefreshUI();
    }

    void Update()
    {
        // ✅ Silahlar aktif olmasa bile reload/cooldown state güncellensin
        if (slot1 != null) slot1.Tick();
        if (slot2 != null) slot2.Tick();

        // Ateş
        if (current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame) current.OnPress();
            if (Mouse.current.leftButton.wasReleasedThisFrame) current.OnRelease();
        }

        // Reload
        if (current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            current.StartReload();
        }

        // Slot değiştirme 
        if (Keyboard.current.digit1Key.wasPressedThisFrame) Equip(slot1);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) Equip(slot2);

        // Yerdeki silahla değiştir / al
        if (nearbyPickup != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            TakeOrSwap(nearbyPickup);
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

        // İki slot da doluysa: mevcut silahla yerdekini değiştir
        if (current == null)
        {
           
            Equip(slot1);
        }

        // Yere bırakılacak silahı oluştur
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

    void RefreshUI()
    {
        //Debug.Log("RefreshUI called, nearby=" + (nearbyPickup ? nearbyPickup.pickupName : "null"));
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



    void PutIntoFirstEmptySlot(WeaponBase w)
    {
        if (slot1 == null) slot1 = w;
        else if (slot2 == null) slot2 = w;
        else slot1 = w; // fallback (normalde buraya düşmez)
    }

    void Equip(WeaponBase w)
    {
        if (current != null)
        {
            current.OnRelease();
            current.gameObject.SetActive(false);
            RefreshUI();
        }

        current = w;

        if (current != null)
        {
            current.gameObject.SetActive(true);
        }
        if (current != null) current.Tick();

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

        // Silahın pickup prefabını yere bırak
        var p = Instantiate(current.pickupPrefab, position, Quaternion.identity);

        // Güvenlik: pickup prefabı doğru weaponPrefab'a zaten sahip olmalı
        // (WeaponPickup2D üzerinde weaponPrefab alanı dolu olmalı)
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
