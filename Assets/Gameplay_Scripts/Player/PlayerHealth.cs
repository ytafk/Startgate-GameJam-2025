using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 1. BU SATIRI EKLEMEN ÞART

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Stats")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("UI References")]
    public Slider healthSlider;
    public Image fillImage;

    [Header("Bar Colors")]
    public Sprite greenBar;
    public Sprite orangeBar;
    public Sprite redBar;

    [Header("Scene Settings")]
    public string gameOverSceneName = "GameOver"; // 2. Geçilecek sahnenin adýný buraya yaz

    void Start()
    {
        currentHP = maxHP;

        if (healthSlider == null)
        {
            Debug.LogError("HATA: Inspector'da 'Health Slider' kutusu boþ!");
            return;
        }

        healthSlider.minValue = 0;
        healthSlider.maxValue = maxHP;
        healthSlider.wholeNumbers = false;
        healthSlider.value = currentHP;

        UpdateHealthUI();
    }

    public void TakeDamage(float dmg)
    {
        currentHP -= dmg;

        if (currentHP <= 0f)
        {
            currentHP = 0f;
            Die(); // Ölüm fonksiyonunu çaðýr
        }

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
        Debug.Log("Player öldü! Sahne yükleniyor...");
        // 3. SAHNE YÜKLEME KOMUTU BURADA
        SceneManager.LoadScene(gameOverSceneName);
    }
}