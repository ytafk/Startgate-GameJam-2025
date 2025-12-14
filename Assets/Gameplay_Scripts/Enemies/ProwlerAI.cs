using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProwlerAI : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;
    public string playerTag = "Player"; // Hedefi bulmak için tag
    public float stopDistance = 0.25f;

    [Header("Attack (Animation Driven)")]
    public Animator animator;
    public float attackRange = 0.65f;
    public float attackCooldown = 0.55f;
    public float attackLockTime = 0.25f;   // Saldırı sırasında hareket kilidi

    [Header("Damage Window")]
    public float damageWindowDuration = 0.12f;
    public float damageRadius = 0.35f;
    public LayerMask playerMask;

    [Header("Base Stats")]
    public float baseSpeed = 2.0f;
    public float baseDamage = 6.0f;

    [Header("Overload Scaling (0..42)")]
    public float maxOverload = 42f;
    public float maxSpeedMultiplier = 2.0f;
    public float maxDamageMultiplier = 2.2f;

    [Header("Contact Damage")]
    public float hitInterval = 0.45f;

    [Header("Separation (Enemy Avoidance)")]
    public float separationRadius = 0.6f;
    public float separationStrength = 2.5f;
    public LayerMask enemyMask;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleMask;
    public float avoidDistance = 1.2f;
    public float avoidRadius = 0.25f;
    public float avoidStrength = 3.0f;
    public float avoidSideBias = 0.35f; // sürekli sağ/sol seçsin diye küçük bias

    // Referanslar
    private Rigidbody2D rb;
    private EnemyRobot overloadCore;

    // State Değişkenleri
    private float nextHitTime;
    private bool isAttacking;
    private bool damageWindowOpen;
    private bool didDamageThisAttack;
    private float nextAttackTime;
    private float attackUnlockTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Yerçekimini kapat (Simple kodundan gelen güvenlik)
        overloadCore = GetComponent<EnemyRobot>();
    }

    void Start()
    {
        AcquireTarget();
    }

    void FixedUpdate()
    {
        // 1. HEDEF KONTROLÜ (Simple kodundan: Robust Target Check)
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            AcquireTarget();
            if (target == null)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        // 2. SALDIRI KİLİDİ (Complex kodundan)
        if (isAttacking && Time.time < attackUnlockTime)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 3. HAREKET MANTIĞI (Complex kodundan: Separation + Smoothing)
        float spd = GetCurrentSpeed();
        Vector2 to = (target.position - transform.position);
        float dist = to.magnitude;
        Vector2 dirToPlayer = dist <= 0.0001f ? Vector2.zero : (to / dist);

        // Birbirini itme kuvveti
        Vector2 sep = ComputeSeparation();
        Vector2 desiredVel;

        if (dist <= stopDistance)
            desiredVel = sep * spd; // Hedefe vardıysa sadece diğerlerinden uzaklaşsın
        else
            desiredVel = (dirToPlayer * spd) + (sep * separationStrength);

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVel, 0.35f);

        // 4. SALDIRI BAŞLATMA (Complex kodundan)
        float distToTarget = Vector2.Distance(transform.position, target.position);
        if (!isAttacking && Time.time >= nextAttackTime && distToTarget <= attackRange)
        {
            StartAttack();
        }

       
        Vector2 avoid = ComputeObstacleAvoidance(dirToPlayer);   // ✅ yeni

        

        if (dist <= stopDistance)
            desiredVel = (sep * separationStrength) + (avoid * avoidStrength);
        else
            desiredVel = (dirToPlayer * spd) + (sep * separationStrength) + (avoid * avoidStrength);

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVel, 0.35f);

    }
    Vector2 ComputeObstacleAvoidance(Vector2 forwardDir)
    {
        if (obstacleMask.value == 0) return Vector2.zero;
        if (forwardDir.sqrMagnitude < 0.0001f) return Vector2.zero;

        Vector2 origin = transform.position;

        // Öne doğru "kalın" ray: circlecast
        RaycastHit2D hit = Physics2D.CircleCast(origin, avoidRadius, forwardDir, avoidDistance, obstacleMask);
        if (!hit.collider) return Vector2.zero;

        // Engel normali: engelden uzağa yön
        Vector2 away = hit.normal;

        // Hafif yan bias: sürekli aynı tarafa kırılmasın diye
        Vector2 side = new Vector2(-forwardDir.y, forwardDir.x); // sol
        away += side * avoidSideBias;

        return away.normalized;
    }

    // --- HEDEF BULMA (Simple'dan) ---
    void AcquireTarget()
    {
        var go = GameObject.FindGameObjectWithTag(playerTag);
        target = go ? go.transform : null;
    }

    // --- SALDIRI SİSTEMİ (Complex'ten) ---
    void StartAttack()
    {
        isAttacking = true;
        didDamageThisAttack = false;
        damageWindowOpen = false;

        nextAttackTime = Time.time + attackCooldown;
        attackUnlockTime = Time.time + attackLockTime;

        rb.linearVelocity = Vector2.zero; // Saldırırken dur

        if (animator != null)
            animator.SetTrigger("Attack");
    }

    // Animation Event: Vurma penceresi başlar
    public void AttackWindowStart()
    {
        damageWindowOpen = true;
        Invoke(nameof(AttackWindowEnd), damageWindowDuration);
        TryDealDamageNow(); // Açılır açılmaz kontrol et
    }

    // Aktif saldırı hasarı (Pencere açıkken)
    void TryDealDamageNow()
    {
        if (!damageWindowOpen) return;
        if (didDamageThisAttack) return;

        Collider2D c = Physics2D.OverlapCircle(transform.position, damageRadius, playerMask);
        if (!c) return;

        var ph = c.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(GetCurrentDamage());
            didDamageThisAttack = true;
        }
    }

    public void AttackWindowEnd()
    {
        damageWindowOpen = false;
    }

    // Animation Event: Animasyon bitti
    public void AttackFinished()
    {
        isAttacking = false;
        damageWindowOpen = false;
        didDamageThisAttack = false;
    }

    // --- FİZİKSEL HESAPLAMALAR ---
    Vector2 ComputeSeparation()
    {
        if (enemyMask.value == 0) return Vector2.zero;

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, separationRadius, enemyMask);
        Vector2 sum = Vector2.zero;
        int count = 0;

        foreach (var c in cols)
        {
            if (!c) continue;
            if (c.attachedRigidbody == rb) continue;

            Vector2 away = (Vector2)transform.position - (Vector2)c.transform.position;
            float d = away.magnitude;
            if (d < 0.0001f) continue;

            sum += away / (d * d);
            count++;
        }

        if (count == 0) return Vector2.zero;
        return (sum / count).normalized;
    }

    // --- STATS & OVERLOAD ---
    float Power01()
    {
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

    // --- TEMAS HASARI (Collision) ---
    void OnCollisionEnter2D(Collision2D col) => TryDealContactDamage(col);
    void OnCollisionStay2D(Collision2D col) => TryDealContactDamage(col);

    void TryDealContactDamage(Collision2D col)
    {
        if (Time.time < nextHitTime) return;

        var ph = col.collider.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(GetCurrentDamage());
            nextHitTime = Time.time + hitInterval;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}