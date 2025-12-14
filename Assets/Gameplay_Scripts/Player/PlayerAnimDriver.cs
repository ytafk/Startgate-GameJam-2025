using UnityEngine;

public class PlayerAnimDriver : MonoBehaviour
{
    public Animator animator;
    public Rigidbody2D rb;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float speed = rb ? rb.linearVelocity.magnitude : 0f;
        animator.SetFloat("Speed", speed);
    }

    public void TriggerShoot()
    {
        // Dead iken shoot spam olmasın
        if (animator.GetBool("IsDead")) return;
        animator.SetTrigger("Shoot");
    }

    public void SetDashing(bool v)
    {
        if (animator.GetBool("IsDead")) return;
        animator.SetBool("IsDashing", v);
    }

    public void TriggerHit()
    {
        if (animator.GetBool("IsDead")) return;
        animator.SetTrigger("Hit");
    }

    public void SetDead(bool v)
    {
        animator.SetBool("IsDead", v);

        if (v)
        {
            // ✅ Ölürken pending trigger’ları temizle
            animator.ResetTrigger("Hit");
            animator.ResetTrigger("Shoot");
        }
    }
}
