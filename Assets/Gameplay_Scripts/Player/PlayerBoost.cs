using System.Collections;
using UnityEngine;

public class PlayerBoost : MonoBehaviour
{
    [Header("Boost Settings")]
    public float speedMultiplier = 1.5f;          // %50 daha hızlı
    public float dashCooldownMultiplier = 0.6f;   // cooldown %40 azalır
    public float duration = 10f;

    Coroutine co;

    // Dışarıdan pickup bu fonksiyonu çağıracak
    public void ActivateBoost(float speedMul, float dashCdMul, float seconds)
    {
        speedMultiplier = speedMul;
        dashCooldownMultiplier = dashCdMul;
        duration = seconds;

        if (co != null) StopCoroutine(co);
        co = StartCoroutine(BoostRoutine());
    }

    IEnumerator BoostRoutine()
    {
        // ✅ buff aktif flag’leri
        _active = true;

        yield return new WaitForSeconds(duration);

        _active = false;
        co = null;
    }

    bool _active;
    public bool IsActive => _active;

    // Bu değerleri diğer scriptler kullanacak:
    public float CurrentSpeedMultiplier => _active ? speedMultiplier : 1f;
    public float CurrentDashCooldownMultiplier => _active ? dashCooldownMultiplier : 1f;
}
