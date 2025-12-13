using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProwlerAI : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Player
    public float stopDistance = 0.25f;

    [Header("Attack (Animation Driven)")]
    public Animator animator;
    public float attackRange = 0.65f;
    public float attackCooldown = 0.55f;   // çok uzun yapma, kaçmayı kolaylaştırır
    public float attackLockTime = 0.25f;   // saldırı sırasında hareket kilidi

    [Header("Damage Window")]
    public float damageWindowDuration = 0.12f; // 0.08–0.15 iyi aralık
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
    public float hitInterval = 0.45f; // kaç saniyede bir vuracak

    [Header("Separation (Enemy Avoidance)")]
    public float separationRadius = 0.6f;      // birbirini algılama yarıçapı
    public float separationStrength = 2.5f;    // itme gücü
    public LayerMask enemyMask;                // Enemy layer seç

    private Rigidbody2D rb;
    private EnemyRobot overloadCore; // overload/patlama scriptin
    private float nextHitTime;
    bool isAttacking;
    bool damageWindowOpen;
    bool didDamageThisAttack;
    float nextAttackTime;
    float attackUnlockTime;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        overloadCore = GetComponent<EnemyRobot>();
    }

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
    }

    void FixedUpdate()
    {
        if (!target) return;

        float spd = GetCurrentSpeed();

        Vector2 to = (target.position - transform.position);
        float dist = to.magnitude;

        Vector2 dirToPlayer = dist <= 0.0001f ? Vector2.zero : (to / dist);

        // Separation kuvveti (yakındaki düşmanlardan uzaklaş)
        Vector2 sep = ComputeSeparation();

        Vector2 desiredVel;
        if (dist <= stopDistance)
            desiredVel = sep * spd; // durunca bile birbirini itip açsın
        else
            desiredVel = (dirToPlayer * spd) + (sep * separationStrength);

        // Fizik itmelerine şans tanımak için velocity'yi yumuşat
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVel, 0.35f);
        if (!target) return;

        // saldırı kilidindeyken hareket etme
        if (isAttacking && Time.time < attackUnlockTime)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float dist1 = Vector2.Distance(transform.position, target.position);

        if (!isAttacking && Time.time >= nextAttackTime && dist1 <= attackRange)
        {
            StartAttack();
            return;
        }


    }
    void StartAttack()
    {
        isAttacking = true;
        didDamageThisAttack = false;
        damageWindowOpen = false;

        nextAttackTime = Time.time + attackCooldown;
        attackUnlockTime = Time.time + attackLockTime;

        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Attack");
    }
    // Animation Event: vurma anında çağrılacak
    public void AttackWindowStart()
    {
        damageWindowOpen = true;
        Invoke(nameof(AttackWindowEnd), damageWindowDuration);

        // pencere açılır açılmaz bir kere kontrol et
        TryDealDamageNow();
    }
    void TryDealDamageNow()
    {
        if (!damageWindowOpen) return;
        if (didDamageThisAttack) return;

        Collider2D c = Physics2D.OverlapCircle(transform.position, damageRadius, playerMask);
        if (!c) return;

        var ph = c.GetComponentInParent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(GetCurrentDamage()); // senin overload’a göre artan damage fonksiyonun
            didDamageThisAttack = true;
        }
    }


    public void AttackWindowEnd()
    {
        damageWindowOpen = false;
    }

    // Animation Event: animasyon bitince çağrılacak
    public void AttackFinished()
    {
        isAttacking = false;
        damageWindowOpen = false;
        didDamageThisAttack = false;
    }



    Vector2 ComputeSeparation()
    {
        if (enemyMask.value == 0) return Vector2.zero;

        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, separationRadius, enemyMask);

        Vector2 sum = Vector2.zero;
        int count = 0;

        foreach (var c in cols)
        {
            if (!c) continue;
            if (c.attachedRigidbody == rb) continue; // kendisi

            Vector2 away = (Vector2)transform.position - (Vector2)c.transform.position;
            float d = away.magnitude;
            if (d < 0.0001f) continue;

            // Yakın olan daha fazla itsin
            sum += away / (d * d);
            count++;
        }

        if (count == 0) return Vector2.zero;
        return (sum / count).normalized;
    }

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

    // ✅ Temas hasarı: Collision ile (mask'e bağlı değil)
    void OnCollisionEnter2D(Collision2D col)
    {
        TryDealContactDamage(col);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        TryDealContactDamage(col);
    }

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
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
#endif
}
