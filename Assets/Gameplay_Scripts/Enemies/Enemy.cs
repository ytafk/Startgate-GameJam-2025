using System;
using UnityEngine;

public class EnemyRobot : MonoBehaviour
{
    [Header("Overload Settings")]
    public float maxOverload = 42f;
    public float currentOverload = 0f;

    [Header("Optional")]
    public float overloadDecayPerSecond = 0f;

    [Header("FX")]
    public GameObject explosionPrefab;

    public static event Action<EnemyRobot> OnAnyEnemyDied;

    EnemyOverloadUI ui;

    // ✅ YENİ: Düşmanın daha önce ölüp ölmediğini kontrol eden değişken
    private bool isDead = false;

    void Awake()
    {
        ui = GetComponentInChildren<EnemyOverloadUI>();
        UpdateUI();
    }

    void Update()
    {
        // Öldüyse overload azaltmaya gerek yok
        if (isDead) return;

        if (overloadDecayPerSecond > 0f && currentOverload > 0f)
        {
            currentOverload -= overloadDecayPerSecond * Time.deltaTime;
            currentOverload = Mathf.Max(0f, currentOverload);
            UpdateUI();
        }
    }

    public void AddOverload(float amount)
    {
        // ✅ EĞER ZATEN ÖLDÜYSE İŞLEM YAPMA (Puanın tekrar eklenmesini önler)
        if (isDead) return;

        currentOverload += amount;
        currentOverload = Mathf.Clamp(currentOverload, 0f, maxOverload);

        UpdateUI();

        if (currentOverload >= maxOverload)
        {
            Explode();
        }
        var flash = GetComponent<EnemyHitFlash>();
        if (flash != null)
            flash.Flash();

    }

    void Explode()
    {
        // ✅ ÇİFTE KONTROL: Eğer zaten öldüyse bu fonksiyonu hemen durdur.
        if (isDead) return;

        // İlk kez giriyorsa hemen "öldü" olarak işaretle
        isDead = true;

        // 1. Patlama Efekti
        if (explosionPrefab)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // 2. Loot
        var dropper = GetComponent<EnemyLootDropper>();
        if (dropper != null) dropper.Drop();


        // 3. SKOR EKLEME (Artık sadece 1 kere çalışacak)
        if (WeaponUIManager.Instance != null)
        {
            WeaponUIManager.Instance.AddScore(42);
        }

        // 4. Event
        OnAnyEnemyDied?.Invoke(this);

        // 5. Devre Dışı Bırakma
        GetComponent<EnemyAnimDriver>()?.SetDead();

        var ai = GetComponent<ProwlerAI>();
        if (ai) ai.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // 6. Yok Et
        Destroy(gameObject);
    }

    void UpdateUI()
    {
        if (ui)
            ui.SetValue(currentOverload / maxOverload);
    }
}