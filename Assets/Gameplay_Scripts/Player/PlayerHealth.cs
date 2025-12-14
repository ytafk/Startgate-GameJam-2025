using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    public float maxHP = 100f;
    public float currentHP;
    public bool IsDead { get; private set; } // Dýþarýdan okunabilir ama sadece buradan deðiþtirilebilir

    [Header("UI References")]
    public Slider healthSlider;
    public Image fillImage;

    [Header("Bar Colors")]
    public Sprite greenBar;
    public Sprite orangeBar;
    public Sprite redBar;

    [Header("Animation & Control")]
    public PlayerAnimDriver anim;
    public float hitAnimCooldown = 0.25f;
    private float nextHitAnimTime;

    [Header("Scene Settings")]
    public string gameOverSceneName = "GameOver";
    public float delayBeforeSceneLoad = 2.0f; // Animasyonun görünmesi için bekleme süresi

    void Awake()
    {
        // Bileþen referanslarýný al
        if (!anim) anim = GetComponent<PlayerAnimDriver>();
    }

    void Start()
    {
        // Caný baþlat ve sýnýrla
        currentHP = maxHP;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        // UI Slider Ayarlarý
        if (healthSlider == null)
        {
            Debug.LogError("HATA: Inspector'da 'Health Slider' kutusu boþ!");
        }
        else
        {
            healthSlider.minValue = 0;
            healthSlider.maxValue = maxHP;
            healthSlider.wholeNumbers = false;
            healthSlider.value = currentHP;
        }

        UpdateHealthUI();
    }

    public void TakeDamage(float dmg)
    {
        // Ölüler hasar alamaz
        if (IsDead) return;
        if (dmg <= 0f) return;

        currentHP -= dmg;

        // Can eksiye düþmesin
        if (currentHP <= 0f)
        {
            currentHP = 0f;
            UpdateHealthUI(); // Son durumu UI'da göster (boþ bar)
            Die();
            return;
        }

        // UI'ý güncelle
        UpdateHealthUI();

        // Hit Animasyonu (Cooldown kontrolü ile)
        if (anim && Time.time >= nextHitAnimTime)
        {
            anim.TriggerHit();
            nextHitAnimTime = Time.time + hitAnimCooldown;
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthSlider == null) return;

        healthSlider.value = currentHP;

        if (fillImage != null)
        {
            float percentage = currentHP / maxHP;

            if (percentage > 0.5f) fillImage.sprite = greenBar;
            else if (percentage > 0.25f) fillImage.sprite = orangeBar;
            else fillImage.sprite = redBar;
        }
    }

    void Die()
    {
        if (IsDead) return; // Zaten ölüyse tekrar çaðýrma
        IsDead = true;

        Debug.Log("Player öldü! Ýþlemler baþlatýlýyor...");

        // 1. Animasyonu Oynat
        if (anim) anim.SetDead(true);

        // 2. Kontrolleri Kilitle (Player hareket edemesin)
        var lockCtrl = GetComponent<PlayerControlLock>();
        if (!lockCtrl) lockCtrl = gameObject.AddComponent<PlayerControlLock>();
        lockCtrl.Lock();

        // 3. Sahneyi Yükle (Animasyonun bitmesi için biraz bekleyerek)
        Invoke(nameof(LoadGameOverScene), delayBeforeSceneLoad);
    }

    void LoadGameOverScene()
    {
        SceneManager.LoadScene(gameOverSceneName);
    }
}