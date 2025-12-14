using UnityEngine;

public class EnemyAnimDriver : MonoBehaviour
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
        if (animator.GetBool("IsDead"))
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        float speed = rb ? rb.linearVelocity.magnitude : 0f;
        animator.SetFloat("Speed", speed);
    }

    public void TriggerAttack()
    {
        if (animator.GetBool("IsDead")) return;
        animator.SetTrigger("Attack");
    }

    public void SetDead()
    {
        animator.SetBool("IsDead", true);
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Hit");
    }
}
