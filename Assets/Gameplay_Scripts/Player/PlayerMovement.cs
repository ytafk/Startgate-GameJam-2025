using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D_NewInput : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Dash")]
    public float dashSpeed = 14f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.45f;

    [Header("Slide/Drag")]
    public float normalDrag = 6f;   // yürürken daha az kay
    public float dashDrag = 0.5f;   // dash sonrası biraz kay

    [Header("Animation")]
    public PlayerAnimDriver anim;   // PlayerAnimDriver scriptin (Player üzerinde olacak)

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.right;

    private bool isDashing;
    private bool canDash = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        if (!anim) anim = GetComponent<PlayerAnimDriver>(); // otomatik bul
    }

    // Move (WASD)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        // Son yönü hatırla (dash yönü için)
        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDir = moveInput.normalized;
    }

    // Dash (Space)
    public void OnDash(InputValue value)
    {
        if (!value.isPressed) return;

        if (canDash && !isDashing)
            StartCoroutine(DashRoutine());
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        rb.linearDamping = normalDrag;
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        // ✅ animasyonu başlat
        if (anim) anim.SetDashing(true);

        rb.linearDamping = dashDrag;

        Vector2 dir = (moveInput.sqrMagnitude > 0.01f) ? moveInput.normalized : lastMoveDir;
        rb.linearVelocity = dir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        // ✅ animasyonu bitir
        if (anim) anim.SetDashing(false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
