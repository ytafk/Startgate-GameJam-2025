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
    public TextMeshProUGUI highScoreText;  // En Yüksek Skor Yazısı

    [Header("Wave & Enemy UI")]
    public TextMeshProUGUI waveText;       // ✅ YENİ: Dalga sayısını gösterecek Text
    public TextMeshProUGUI enemyCountText; // ✅ YENİ: Kalan düşman sayısını gösterecek Text

    private int currentScore = 0;
    private int highScore = 0;             // Hafızadaki en yüksek skor

    // Takip edilen silah
    private WeaponBase currentWeapon;
    private WeaponIconData currentIconData;

    void Awake()
    {
        Instance = this;
        if (infoPanel) infoPanel.SetActive(false);

        // Oyun açılınca kayıtlı en yüksek skoru yükle
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

    // --- YENİ EKLENEN FONKSİYONLAR ---
    public void UpdateWaveUI(int waveIndex)
    {
        if (waveText != null)
            waveText.text = "WAVE: " + waveIndex.ToString();
    }

    public void UpdateEnemyCountUI(int count)
    {
        if (enemyCountText != null)
            enemyCountText.text = "Enemies:" + count.ToString();
    }
    // ---------------------------------

    public void AddScore(int amount)
    {
        currentScore += amount;

        // Eğer mevcut skor, rekoru geçerse kaydet
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + currentScore.ToString();
        }

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

    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
        highScore = 0;
        UpdateScoreUI();
    }
}