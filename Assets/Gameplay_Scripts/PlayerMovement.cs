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
    public float dashDrag = 0.5f;   // dash sonrasý biraz kay

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.right;

    private bool isDashing;
    private bool canDash = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    // Move (WASD)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        // Son yönü hatýrla (dash yönü için)
        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDir = moveInput.normalized;
        
    }

    // Dash (Space)
    public void OnDash(InputValue value)
    {
        // Button basýldý aný (basýlý tutmaya deðil)
        if (!value.isPressed) return;

        if (canDash && !isDashing)
            StartCoroutine(DashRoutine());
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        // normal hareket
        rb.linearDamping = normalDrag;
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        // Dash anýnda kaymayý artýr (drag düþür)
        rb.linearDamping = dashDrag;

        Vector2 dir = (moveInput.sqrMagnitude > 0.01f) ? moveInput.normalized : lastMoveDir;
        rb.linearVelocity = dir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        // Cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}

