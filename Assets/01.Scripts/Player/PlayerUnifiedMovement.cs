using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerUnifiedMovement : MonoBehaviour
{
    private const float GroundCheckDistance = 0.08f;
    private const float GroundNormalThreshold = 0.5f;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 70f;
    [SerializeField] private float deceleration = 90f;
    [SerializeField] private float airAcceleration = 45f;
    [SerializeField] private float airDeceleration = 35f;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 12f;
    [SerializeField] private float coyoteTime = 0.08f;
    [SerializeField] private float jumpBufferTime = 0.08f;
    [SerializeField] private float fallGravityMultiplier = 1.6f;
    [SerializeField] private float lowJumpGravityMultiplier = 2f;

    [Header("Dash")]
    [SerializeField] private float dashRecoil = 16f;
    [SerializeField] private float dashCooldown = 0.35f;

    [Header("Recoil")]
    [SerializeField] private float maxRecoilSpeed = 28f;
    [SerializeField] private bool clearRecoilWhenNoMoveInput = true;
    [SerializeField] private bool clearRecoilWhenMoveDirectionChanges = true;

    [Header("Ground")]
    [SerializeField] private LayerMask groundLayer;

    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private ContactFilter2D groundFilter;
    private Vector2 moveInput;
    private Vector2 recoilVelocity;
    private Vector2 appliedRecoilVelocity;
    private float movementVelocityX;
    private float lastMoveDirection = 1f;
    private float currentMoveDirection;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float dashCooldownTimer;
    private bool jumpHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        groundFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = false
        };
        groundFilter.SetLayerMask(groundLayer);
    }

    private void Update()
    {
        if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();
        Vector2 baseVelocity = rb.linearVelocity - appliedRecoilVelocity;

        coyoteTimer = grounded ? coyoteTime : coyoteTimer - Time.fixedDeltaTime;

        UpdateMoveState(grounded);
        TryConsumeJump(ref baseVelocity);
        ApplyBetterJumpGravity(ref baseVelocity);

        appliedRecoilVelocity = recoilVelocity;
        rb.linearVelocity = new Vector2(movementVelocityX, baseVelocity.y) + appliedRecoilVelocity;
    }

    public void OnMove(InputValue value)
    {
        float previousMoveDirection = currentMoveDirection;
        moveInput = value.Get<Vector2>();
        currentMoveDirection = Mathf.Abs(moveInput.x) > 0.01f ? Mathf.Sign(moveInput.x) : 0f;

        if (currentMoveDirection != 0f)
        {
            lastMoveDirection = currentMoveDirection;
        }

        bool stoppedMoving = currentMoveDirection == 0f && previousMoveDirection != 0f;
        bool changedDirection = currentMoveDirection != 0f && previousMoveDirection != 0f && currentMoveDirection != previousMoveDirection;

        if ((clearRecoilWhenNoMoveInput && stoppedMoving) ||
            (clearRecoilWhenMoveDirectionChanges && changedDirection))
        {
            ClearRecoil();
        }
    }

    public void OnJump(InputValue value)
    {
        jumpHeld = value.isPressed;

        if (value.isPressed)
        {
            jumpBufferTimer = jumpBufferTime;
        }
    }

    public void OnDash(InputValue value)
    {
        if (!value.isPressed || dashCooldownTimer > 0f)
        {
            return;
        }

        float dashDirection = currentMoveDirection != 0f ? currentMoveDirection : lastMoveDirection;
        AddRecoil(Vector2.right * dashDirection * dashRecoil);
        dashCooldownTimer = dashCooldown;
    }

    public void OnSprint(InputValue value)
    {
        OnDash(value);
    }

    public void AddRecoil(Vector2 value)
    {
        recoilVelocity = Vector2.ClampMagnitude(recoilVelocity + value, maxRecoilSpeed);
    }

    public void ClearRecoil()
    {
        recoilVelocity = Vector2.zero;
        appliedRecoilVelocity = Vector2.zero;
    }

    private void UpdateMoveState(bool grounded)
    {
        float targetSpeed = moveInput.x * moveSpeed;
        float rate = Mathf.Abs(targetSpeed) > 0.01f
            ? grounded ? acceleration : airAcceleration
            : grounded ? deceleration : airDeceleration;

        movementVelocityX = Mathf.MoveTowards(
            movementVelocityX,
            targetSpeed,
            rate * Time.fixedDeltaTime);
    }

    private void TryConsumeJump(ref Vector2 baseVelocity)
    {
        if (jumpBufferTimer <= 0f || coyoteTimer <= 0f)
        {
            return;
        }

        baseVelocity.y = jumpPower;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
    }

    private void ApplyBetterJumpGravity(ref Vector2 baseVelocity)
    {
        if (baseVelocity.y < 0f)
        {
            baseVelocity += Physics2D.gravity * (rb.gravityScale * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime);
        }
        else if (baseVelocity.y > 0f && !jumpHeld)
        {
            baseVelocity += Physics2D.gravity * (rb.gravityScale * (lowJumpGravityMultiplier - 1f) * Time.fixedDeltaTime);
        }
    }

    private bool IsGrounded()
    {
        int count = playerCollider.Cast(Vector2.down, groundFilter, groundHits, GroundCheckDistance);

        for (int i = 0; i < count; i++)
        {
            if (groundHits[i].normal.y > GroundNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }
}
