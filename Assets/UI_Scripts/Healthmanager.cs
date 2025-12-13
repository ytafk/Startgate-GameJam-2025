using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // BU SATIRI EKLEMEN GEREK

public class Healthmanager : MonoBehaviour
{
    // ... (Diðer deðiþkenler ayný kalacak) ...
    public Slider healthSlider;
    public Image fillImage;
    public Sprite greenBar;
    public Sprite orangeBar;
    public Sprite redBar;
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    void Update()
    {
        // ESKÝ KOD: if (Input.GetKeyDown(KeyCode.Space))

        // YENÝ KOD: Klavyedeki Space tuþunu kontrol etme
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TakeDamage(10);
        }
    }

    // ... (TakeDamage ve UpdateHealthBar fonksiyonlarý ayný kalacak) ...
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        healthSlider.value = currentHealth;
        float percentage = (float)currentHealth / maxHealth;

        if (percentage > 0.5f) fillImage.sprite = greenBar;
        else if (percentage > 0.25f) fillImage.sprite = orangeBar;
        else fillImage.sprite = redBar;
    }
}