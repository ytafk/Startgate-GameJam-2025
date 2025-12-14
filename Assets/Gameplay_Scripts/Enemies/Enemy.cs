using System; // Action event'i için gerekli
using UnityEngine;

public class EnemyRobot : MonoBehaviour
{
    [Header("Overload Settings")]
    public float maxOverload = 42f;
    public float currentOverload = 0f;

    [Header("Optional")]
    public float overloadDecayPerSecond = 0f; // Zamanla azalma hızı

    [Header("FX")]
    public GameObject explosionPrefab;

    // Düşman öldüğünde tetiklenecek global olay
    public static event Action<EnemyRobot> OnAnyEnemyDied;

    // UI Bağlantısı
    EnemyOverloadUI ui;

    void Awake()
    {
        ui = GetComponentInChildren<EnemyOverloadUI>();
        UpdateUI();
    }

    void Update()
    {
        // Aşırı yükün zamanla azalması (Opsiyonel)
        if (overloadDecayPerSecond > 0f && currentOverload > 0f)
        {
            currentOverload -= overloadDecayPerSecond * Time.deltaTime;
            currentOverload = Mathf.Max(0f, currentOverload);
            UpdateUI();
        }
    }

    public void AddOverload(float amount)
    {
        currentOverload += amount;
        currentOverload = Mathf.Clamp(currentOverload, 0f, maxOverload);

        UpdateUI();

        if (currentOverload >= maxOverload)
        {
            Explode();
        }
    }

    void Explode()
    {
        // 1. Patlama Efekti
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // 2. Loot (Ganimet) Düşürme
        var dropper = GetComponent<EnemyLootDropper>();
        if (dropper != null) dropper.Drop();

        // ✅ 3. SKOR EKLEME (BURASI EKLENDİ)
        // Eğer sahnede WeaponUIManager varsa 42 puan ekle
        if (WeaponUIManager.Instance != null)
        {
            WeaponUIManager.Instance.AddScore(42);
        }

        // 4. Ölüm Eventini Bildir (WaveManager vs. için)
        OnAnyEnemyDied?.Invoke(this);

        // 5. Bileşenleri Devre Dışı Bırak (Ölüm animasyonu vs. için güvenlik)
        GetComponent<EnemyAnimDriver>()?.SetDead();

        var ai = GetComponent<ProwlerAI>();
        if (ai) ai.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero; // Unity 6 için linearVelocity

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // 6. Objeyi Yok Et
        Destroy(gameObject);
    }

    void UpdateUI()
    {
        if (ui)
            ui.SetValue(currentOverload / maxOverload);
    }
}