using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class NewMovement : MonoBehaviour
{
    private const float GroundCheckDistance = 0.08f;
    private const float WallCheckDistance = 0.04f;
    private const float GroundNormalThreshold = 0.5f;
    private const float WallNormalThreshold = 0.5f;
    private const float InputDeadZone = 0.01f;

    [Header("Move")]
    [FormerlySerializedAs("maxMoveSpeed")]
    [SerializeField] private float maxGroundMoveSpeed = 8f;
    [FormerlySerializedAs("maxAirMoveSpeed")]
    [SerializeField] private float maxAirMoveSpeed = 12f;
    [FormerlySerializedAs("acceleration")]
    [SerializeField] private float moveAcceleration = 60f;
    [SerializeField] private float groundDeceleration = 80f;
    [SerializeField] private float jumpPower = 12f;
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Recoil")]
    [SerializeField] private float recoilDeceleration = 16f;
    [SerializeField] private float airHorizontalRecoilDeceleration = 2f;
    [SerializeField] private float airVerticalRecoilDeceleration = 5f;

    [Header("Collision")]
    [FormerlySerializedAs("groundLayer")]
    [SerializeField] private LayerMask collisionLayer = (1 << 0) | (1 << 3) | (1 << 7);

    private readonly RaycastHit2D[] collisionHits = new RaycastHit2D[4];

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private ContactFilter2D collisionFilter;
    private Vector2 moveInput;
    private Vector2 recoilVelocity;
    private Vector2 appliedRecoilVelocity;
    private float moveVelocityX;
    private float moveAccelerationX;
    private float jumpBufferTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        collisionFilter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = false
        };
        collisionFilter.SetLayerMask(collisionLayer);
    }

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();
        Vector2 baseVelocity = rb.linearVelocity - appliedRecoilVelocity;

        UpdateJumpBufferTimer();
        UpdateMoveVelocity(grounded);
        bool jumped = TryJump(grounded, ref baseVelocity);
        UpdateRecoilVelocity(grounded && !jumped);
        ResolveWallVelocity();

        appliedRecoilVelocity = recoilVelocity;
        rb.linearVelocity = new Vector2(moveVelocityX, baseVelocity.y) + appliedRecoilVelocity;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBufferTimer = jumpBufferTime;
        }
    }

    public void AddRecoil(Vector2 value)
    {
        Vector2 previousRecoilVelocity = recoilVelocity;
        recoilVelocity += value;

        Vector2 recoilDelta = recoilVelocity - previousRecoilVelocity;
        rb.linearVelocity += recoilDelta;
        appliedRecoilVelocity = recoilVelocity;
    }

    private void UpdateMoveVelocity(bool grounded)
    {
        float inputDirection = Mathf.Abs(moveInput.x) > InputDeadZone ? Mathf.Sign(moveInput.x) : 0f;
        float maxSpeed = grounded ? maxGroundMoveSpeed : maxAirMoveSpeed;
        moveAccelerationX = 0f;

        if (inputDirection != 0f)
        {
            if (moveVelocityX != 0f && Mathf.Sign(moveVelocityX) != inputDirection)
            {
                moveVelocityX = 0f;
                moveAccelerationX = 0f;
                return;
            }

            if (Mathf.Abs(moveVelocityX) > maxSpeed)
            {
                moveVelocityX = Mathf.MoveTowards(
                    moveVelocityX,
                    inputDirection * maxSpeed,
                    groundDeceleration * Time.fixedDeltaTime);
                return;
            }

            moveAccelerationX = inputDirection * moveAcceleration;
            moveVelocityX = Mathf.Clamp(
                moveVelocityX + moveAccelerationX * Time.fixedDeltaTime,
                -maxSpeed,
                maxSpeed);

            return;
        }

        moveAccelerationX = 0f;

        if (grounded)
        {
            moveVelocityX = Mathf.MoveTowards(moveVelocityX, 0f, groundDeceleration * Time.fixedDeltaTime);
        }
    }

    private bool TryJump(bool grounded, ref Vector2 baseVelocity)
    {
        if (jumpBufferTimer <= 0f)
        {
            return false;
        }

        if (grounded)
        {
            baseVelocity.y = jumpPower;
            jumpBufferTimer = 0f;
            return true;
        }

        return false;
    }

    private void UpdateJumpBufferTimer()
    {
        if (jumpBufferTimer <= 0f)
        {
            return;
        }

        jumpBufferTimer -= Time.fixedDeltaTime;
    }

    private void UpdateRecoilVelocity(bool grounded)
    {
        if (grounded)
        {
            recoilVelocity.y = 0f;
        }

        float verticalDeceleration = grounded ? recoilDeceleration : airVerticalRecoilDeceleration;
        float horizontalDeceleration = grounded ? groundDeceleration : airHorizontalRecoilDeceleration;

        recoilVelocity = new Vector2(
            Mathf.MoveTowards(recoilVelocity.x, 0f, horizontalDeceleration * Time.fixedDeltaTime),
            Mathf.MoveTowards(recoilVelocity.y, 0f, verticalDeceleration * Time.fixedDeltaTime));
    }

    private bool IsGrounded()
    {
        int count = playerCollider.Cast(Vector2.down, collisionFilter, collisionHits, GroundCheckDistance);

        for (int i = 0; i < count; i++)
        {
            if (collisionHits[i].normal.y > GroundNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }

    private void ResolveWallVelocity()
    {
        float totalVelocityX = moveVelocityX + recoilVelocity.x;

        if (Mathf.Abs(totalVelocityX) <= InputDeadZone)
        {
            return;
        }

        float direction = Mathf.Sign(totalVelocityX);

        if (!IsTouchingWall(direction))
        {
            return;
        }

        if (moveVelocityX != 0f && Mathf.Sign(moveVelocityX) == direction)
        {
            moveVelocityX = 0f;
            moveAccelerationX = 0f;
        }

        if (recoilVelocity.x != 0f && Mathf.Sign(recoilVelocity.x) == direction)
        {
            recoilVelocity.x = 0f;
        }
    }

    private bool IsTouchingWall(float direction)
    {
        Vector2 castDirection = direction > 0f ? Vector2.right : Vector2.left;
        int count = playerCollider.Cast(castDirection, collisionFilter, collisionHits, WallCheckDistance);

        for (int i = 0; i < count; i++)
        {
            float normalX = collisionHits[i].normal.x;

            if (direction > 0f && normalX < -WallNormalThreshold)
            {
                return true;
            }

            if (direction < 0f && normalX > WallNormalThreshold)
            {
                return true;
            }
        }

        return false;
    }
}
