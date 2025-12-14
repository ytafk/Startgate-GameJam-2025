using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUIManager : MonoBehaviour
{
    // Singleton: Her yerden eriþebilmek için
    public static WeaponUIManager Instance;

    [Header("UI Bileþenleri (Canvas'tan sürükle)")]
    public GameObject infoPanel;      // Açýlýp kapanacak panel
    public Image weaponIconImage;     // Silahýn resminin görüneceði Image
    public TextMeshProUGUI nameText;  // Silah ismi
    public TextMeshProUGUI ammoText;  // Mermi sayýsý (12/48 gibi)

    // Takip edilen silah (O an elindeki silah)
    private WeaponBase currentWeapon;
    private WeaponIconData currentIconData;

    void Awake()
    {
        Instance = this;
        // Baþlangýçta paneli gizle
        if (infoPanel) infoPanel.SetActive(false);
    }

    void Update()
    {
        // Eðer elimizde bir silah varsa, mermi bilgisini sürekli güncelle
        if (currentWeapon != null && infoPanel.activeSelf)
        {
            // WeaponBase'deki deðiþkenlerin public olduðu için direkt okuyabiliriz
            ammoText.text = $"{currentWeapon.ammoInMag} / {currentWeapon.reserveAmmo}";

            // Mermi biterse yazýyý kýrmýzý yap (Opsiyonel görsel güzellik)
            if (currentWeapon.ammoInMag <= 0) ammoText.color = Color.red;
            else ammoText.color = Color.white;
        }
    }

    // BU FONKSÝYONU SÝLAH DEÐÝÞTÝRÝNCE ÇAÐIRACAKSIN
    public void UpdateCurrentWeapon(WeaponBase newWeapon)
    {
        currentWeapon = newWeapon;

        if (currentWeapon != null)
        {
            // Silahýn üzerindeki o yeni eklediðimiz "IconData" scriptini buluyoruz
            currentIconData = currentWeapon.GetComponent<WeaponIconData>();

            if (currentIconData != null)
            {
                // Verileri UI'a bas
                weaponIconImage.sprite = currentIconData.icon;
                nameText.text = currentIconData.displayName;

                // Resmi düzgün oranla (sünmemesi için)
                weaponIconImage.preserveAspect = true;

                infoPanel.SetActive(true); // Paneli aç
            }
            else
            {
                Debug.LogWarning("Bu silahta WeaponIconData scripti unutulmuþ!");
            }
        }
        else
        {
            // Silah yoksa paneli kapat
            infoPanel.SetActive(false);
        }
    }
}
