using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Run Feel")]
    public float maxRunSpeed = 9f;
    public float groundAccel = 85f;
    public float groundDecel = 110f;
    public float airAccel = 55f;
    public float airDecel = 25f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Dash (Burst Velocity)")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.35f;

    [Tooltip("If true, dash keeps your current Y velocity. If false, dash zeros Y for a flat dash.")]
    public bool dashKeepsVerticalVelocity = true;

    [Tooltip("0 = keep normal gravity during dash. 0.3 = reduced gravity during dash.")]
    public float dashGravityMultiplier = 0f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    private bool dashPressed;
    private bool isDashing;
    private float dashTimeLeft;
    private float dashCooldownLeft;

    private float defaultGravityScale;
    private float lastFacingX = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultGravityScale = rb.gravityScale;
    }

    // Input System: "Move"
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (Mathf.Abs(moveInput.x) > 0.01f)
            lastFacingX = Mathf.Sign(moveInput.x);
    }

    // Input System: "Dash"
    public void OnDash(InputValue value)
    {
        if (value.isPressed)
            dashPressed = true;
    }

    private void Update()
    {
        if (dashCooldownLeft > 0f)
            dashCooldownLeft -= Time.deltaTime;
        
        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();

        // DASH state
        if (isDashing)
        {
            dashTimeLeft -= Time.fixedDeltaTime;

            rb.gravityScale = defaultGravityScale * dashGravityMultiplier;

            float yVel = dashKeepsVerticalVelocity ? rb.linearVelocity.y : 0f;
            rb.linearVelocity = new Vector2(lastFacingX * dashSpeed, yVel);

            if (dashTimeLeft <= 0f)
                EndDash();

            dashPressed = false;
            return;
        }

        rb.gravityScale = defaultGravityScale;

        // NORMAL RUN (accel/decel)
        float targetSpeed = moveInput.x * maxRunSpeed;

        float accelRate;
        if (Mathf.Abs(targetSpeed) > 0.01f)
            accelRate = grounded ? groundAccel : airAccel;
        else
            accelRate = grounded ? groundDecel : airDecel;

        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

        // DASH trigger
        if (dashPressed && dashCooldownLeft <= 0f)
            StartDash();

        dashPressed = false;
        
        HandleSpriteFlip();
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashCooldownLeft = dashCooldown;

        // If player is holding a direction, dash that way; otherwise dash facing direction.
        if (Mathf.Abs(moveInput.x) > 0.01f)
            lastFacingX = Mathf.Sign(moveInput.x);
    }

    private void EndDash()
    {
        isDashing = false;
        rb.gravityScale = defaultGravityScale;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    
    private void HandleSpriteFlip()
    {
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            spriteRenderer.flipX = moveInput.x < 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}