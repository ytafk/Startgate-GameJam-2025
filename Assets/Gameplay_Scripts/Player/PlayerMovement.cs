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
    public float normalDrag = 6f;    // Yürürken daha az kayma (sürtünme)
    public float dashDrag = 0.5f;    // Dash sırasında daha akışkan kayma

    [Header("Animation")]
    public PlayerAnimDriver anim;    // Animasyonları yönetecek script referansı

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.right;

    private bool isDashing;
    private bool canDash = true;

    // ✅ Boost ref (opsiyonel)
    PlayerBoost boost;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Top-down oyun olduğu için yerçekimi kapalı

        // Eğer Inspector'dan atanmadıysa otomatik bulmayı dene
        if (!anim) anim = GetComponent<PlayerAnimDriver>();

        // ✅ varsa yakala
        boost = GetComponent<PlayerBoost>();
    }

    // Input System: Move (WASD)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        // Hareket varsa son yönü hatırla (Dash atarken duruyorsak bu yöne atılsın diye)
        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDir = moveInput.normalized;
    }

    // Input System: Dash (Space)
    public void OnDash(InputValue value)
    {
        // Sadece tuşa basıldığı an çalışsın (basılı tutarken değil)
        if (!value.isPressed) return;

        if (canDash && !isDashing)
            StartCoroutine(DashRoutine());
    }

    void FixedUpdate()
    {
        // Dash atarken normal hareket kodunu çalıştırma
        if (isDashing) return;

        // ✅ boost component sonradan eklendiyse yakalayabilsin
        if (!boost) boost = GetComponent<PlayerBoost>();

        float speedMul = (boost != null) ? boost.CurrentSpeedMultiplier : 1f;

        // Normal hareket fiziği
        rb.linearDamping = normalDrag; // Unity 6 öncesi için rb.drag kullanın
        rb.linearVelocity = moveInput.normalized * (moveSpeed * speedMul); // Unity 6 öncesi için rb.velocity kullanın
    }

    private IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        // ✅ boost component sonradan eklendiyse yakalayabilsin
        if (!boost) boost = GetComponent<PlayerBoost>();

        float dashCdMul = (boost != null) ? boost.CurrentDashCooldownMultiplier : 1f;

        // ✅ Animasyonu Başlat
        if (anim) anim.SetDashing(true);

        // Dash fiziği: Sürtünmeyi azalt, hızı artır
        rb.linearDamping = dashDrag;

        // Eğer şu an bir yöne basıyorsak oraya, basmıyorsak son baktığımız yöne dash at
        Vector2 dir = (moveInput.sqrMagnitude > 0.01f) ? moveInput.normalized : lastMoveDir;
        rb.linearVelocity = dir * dashSpeed;

        // Dash süresi kadar bekle
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        // ✅ Animasyonu Bitir
        if (anim) anim.SetDashing(false);

        // ✅ Tekrar dash atabilmek için cooldown (boost ile kısalır)
        yield return new WaitForSeconds(dashCooldown * dashCdMul);
        canDash = true;
    }
}
