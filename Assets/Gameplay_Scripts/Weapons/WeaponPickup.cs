using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponBase weaponPrefab;
    public string pickupName = "Silah";

    // --- MERMÝ HAFIZASI ---
    // Varsayýlan -1 (Veri yok demektir, silahýn kendi varsayýlanýný kullanýr)
    [HideInInspector] public int savedAmmoInMag = -1;
    [HideInInspector] public int savedReserveAmmo = -1;

    // Silah yere düþerken bu fonksiyonu çaðýrýp mermiyi kaydedeceðiz
    public void SetAmmoData(int mag, int reserve)
    {
        savedAmmoInMag = mag;
        savedReserveAmmo = reserve;
    }
}
