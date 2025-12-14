using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHP = 100f;
    public float hp = 100f;

    public PlayerAnimDriver anim;

    public float hitAnimCooldown = 0.25f;
    float nextHitAnimTime;

    public bool IsDead { get; private set; }

    void Awake()
    {
        if (!anim) anim = GetComponent<PlayerAnimDriver>();
        hp = Mathf.Clamp(hp, 0f, maxHP);

        if (hp <= 0f)
        {
            IsDead = true;
            if (anim) anim.SetDead(true);
        }
    }

    public void TakeDamage(float dmg)
    {
        if (IsDead) return;
        if (dmg <= 0f) return;

        hp = Mathf.Max(0f, hp - dmg);

        if (hp <= 0f)
        {
            Die();
            return;
        }

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

        hp = Mathf.Clamp(hp + amount, 0f, maxHP);
    }

    void Die()
    {
        IsDead = true;

        // ✅ Ölürken hit/shoot triggerlarını temizleyip death’e geçir
        if (anim) anim.SetDead(true);

        // ✅ Kontrolleri kilitle
        var lockCtrl = GetComponent<PlayerControlLock>();
        if (!lockCtrl) lockCtrl = gameObject.AddComponent<PlayerControlLock>();
        lockCtrl.Lock();
    }
}
