//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class WeaponUIManager : MonoBehaviour
//{
//    // Singleton
//    public static WeaponUIManager Instance;

//    [Header("UI Bileşenleri (Canvas'tan sürükle)")]
//    public GameObject infoPanel;
//    public Image weaponIconImage;
//    public TextMeshProUGUI nameText;
//    public TextMeshProUGUI ammoText;

//    [Header("Score UI")]
//    public TextMeshProUGUI scoreText; // 1. BURAYA SKOR YAZISINI SÜRÜKLEYECEKSİN
//    private int currentScore = 0;     // Mevcut puanı hafızada tutan değişken

//    // Takip edilen silah
//    private WeaponBase currentWeapon;
//    private WeaponIconData currentIconData;

//    void Awake()
//    {
//        Instance = this;
//        if (infoPanel) infoPanel.SetActive(false);

//        // Oyun başlarken skoru sıfırla ve ekrana yaz
//        UpdateScoreUI();
//    }

//    void Update()
//    {
//        if (currentWeapon != null && infoPanel.activeSelf)
//        {
//            ammoText.text = $"{currentWeapon.ammoInMag} / {currentWeapon.reserveAmmo}";

//            if (currentWeapon.ammoInMag <= 0) ammoText.color = Color.red;
//            else ammoText.color = Color.white;
//        }
//    }

//    // --- YENİ EKLENEN SKOR FONKSİYONU ---
//    public void AddScore(int amount)
//    {
//        currentScore += amount; // Gelen puanı ekle
//        UpdateScoreUI();        // Ekrana yazdır
//    }

//    void UpdateScoreUI()
//    {
//        if (scoreText != null)
//        {
//            // Ekranda "SCORE: 42" gibi görünecek
//            scoreText.text = "SCORE: " + currentScore.ToString();
//        }
//    }
//    // -------------------------------------

//    public void UpdateCurrentWeapon(WeaponBase newWeapon)
//    {
//        currentWeapon = newWeapon;

//        if (currentWeapon != null)
//        {
//            currentIconData = currentWeapon.GetComponent<WeaponIconData>();

//            if (currentIconData != null)
//            {
//                weaponIconImage.sprite = currentIconData.icon;
//                nameText.text = currentIconData.displayName;
//                weaponIconImage.preserveAspect = true;
//                infoPanel.SetActive(true);
//            }
//            else
//            {
//                Debug.LogWarning("Bu silahta WeaponIconData scripti unutulmuş!");
//            }
//        }
//        else
//        {
//            infoPanel.SetActive(false);
//        }
//    }
//}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUIManager : MonoBehaviour
{
    // Singleton
    public static WeaponUIManager Instance;

    [Header("UI Bileşenleri")]
    public GameObject infoPanel;
    public Image weaponIconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI ammoText;

    [Header("Score UI")]
    public TextMeshProUGUI scoreText;      // Mevcut Skor Yazısı
    public TextMeshProUGUI highScoreText;  // ✅ YENİ: En Yüksek Skor Yazısı

    private int currentScore = 0;
    private int highScore = 0;             // Hafızadaki en yüksek skor

    // Takip edilen silah
    private WeaponBase currentWeapon;
    private WeaponIconData currentIconData;

    void Awake()
    {
        Instance = this;
        if (infoPanel) infoPanel.SetActive(false);

        // ✅ Oyun açılınca kayıtlı en yüksek skoru yükle
        // Eğer kayıt yoksa 0 kabul et
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        UpdateScoreUI();
    }

    void Update()
    {
        if (currentWeapon != null && infoPanel.activeSelf)
        {
            ammoText.text = $"{currentWeapon.ammoInMag} / {currentWeapon.reserveAmmo}";

            if (currentWeapon.ammoInMag <= 0) ammoText.color = Color.red;
            else ammoText.color = Color.white;
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        // ✅ Eğer mevcut skor, rekoru geçerse kaydet
        if (currentScore > highScore)
        {
            highScore = currentScore;

            // Bilgisayara/Telefona kaydet
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        // Mevcut Skoru Yaz
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + currentScore.ToString();
        }

        // ✅ En Yüksek Skoru Yaz
        if (highScoreText != null)
        {
            highScoreText.text = "BEST: " + highScore.ToString();
        }
    }

    public void UpdateCurrentWeapon(WeaponBase newWeapon)
    {
        currentWeapon = newWeapon;

        if (currentWeapon != null)
        {
            currentIconData = currentWeapon.GetComponent<WeaponIconData>();

            if (currentIconData != null)
            {
                weaponIconImage.sprite = currentIconData.icon;
                nameText.text = currentIconData.displayName;
                weaponIconImage.preserveAspect = true;
                infoPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Bu silahta WeaponIconData eksik!");
            }
        }
        else
        {
            infoPanel.SetActive(false);
        }
    }

    // Test için: Skoru sıfırlamak istersen bu fonksiyonu bir butona bağlayabilirsin
    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
        highScore = 0;
        UpdateScoreUI();
    }
}