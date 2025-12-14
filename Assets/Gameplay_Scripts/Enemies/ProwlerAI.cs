using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProwlerAI : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public string playerTag = "Player";
    public float stopDistance = 0.25f;

    [Header("Base Stats")]
    public float baseSpeed = 2.5f;
    public float baseDamage = 6f;

    [Header("Overload Scaling (0..42)")]
    public float maxOverload = 42f;
    public float maxSpeedMultiplier = 2.5f;
    public float maxDamageMultiplier = 2.0f;

    [Header("Contact Damage")]
    public float hitInterval = 0.45f;

    Rigidbody2D rb;
    EnemyRobot overloadCore;
    float nextHitTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        overloadCore = GetComponent<EnemyRobot>();
    }

    void Start()
    {
        AcquireTarget();
    }

    void FixedUpdate()
    {
        // ✅ spawn/respawn/yanlış instance fix: hedefi canlı tut
        if (target == null || !target.gameObject.activeInHierarchy)
            AcquireTarget();

        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float spd = GetCurrentSpeed();

        Vector2 to = (Vector2)target.position - rb.position;
        float dist = to.magnitude;

        if (dist <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = to / dist;
        rb.linearVelocity = dir * spd;
    }

    void AcquireTarget()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        target = go ? go.transform : null;
    }

    // ===== Overload helpers =====
    float Power01()
    {
        // EnemyRobot scriptinde currentOverload alanı olmalı
        float o = overloadCore ? overloadCore.currentOverload : 0f;
        float n = Mathf.Clamp01(o / Mathf.Max(0.01f, maxOverload));
        return Mathf.SmoothStep(0f, 1f, n);
    }

    float GetCurrentSpeed()
    {
        float p = Power01();
        float mul = Mathf.Lerp(1f, maxSpeedMultiplier, p);
        return baseSpeed * mul;
    }

    float GetCurrentDamage()
    {
        float p = Power01();
        float mul = Mathf.Lerp(1f, maxDamageMultiplier, p);
        return baseDamage * mul;
    }

    // ===== Contact damage (collision) =====
    void OnCollisionEnter2D(Collision2D col) => TryDamage(col);
    void OnCollisionStay2D(Collision2D col) => TryDamage(col);

    void TryDamage(Collision2D col)
    {
        if (Time.time < nextHitTime) return;

        var ph = col.collider.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(GetCurrentDamage());
            nextHitTime = Time.time + hitInterval;
        }
    }
}
