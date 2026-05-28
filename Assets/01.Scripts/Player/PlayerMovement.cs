using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpPower = 12f;
    [SerializeField] private float fallGravity = 1.3f;

    [Header("Ground")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDistance = 0.02f;
    [SerializeField] private float groundNormal = 0.5f;

    [Header("Wall")]
    [SerializeField] private float wallDistance = 0.02f;
    [SerializeField] private float wallJumpPower = 12f;
    [SerializeField] private float wallJumpLockTime = 0.12f;
    [SerializeField] private float wallNormal = 0.5f;

    [Header("Momentum")]
    [SerializeField] private float momentumBrake = 12f;
    [SerializeField] private float maxMomentumX = 18f;
    [SerializeField] private float recoilBrake = 45f;
    [SerializeField] private float maxRecoilX = 12f;

    private readonly RaycastHit2D[] hits = new RaycastHit2D[4];

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private ContactFilter2D groundFilter;
    private Vector2 moveDir;
    private float momentumX;
    private float recoilX;
    private float moveLockTimer;

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

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();

        if (grounded && moveDir.x != 0f && momentumX != 0f && Mathf.Sign(moveDir.x) != Mathf.Sign(momentumX))
        {
            momentumX = 0f;
        }

        if (moveLockTimer > 0f)
        {
            moveLockTimer -= Time.fixedDeltaTime;
        }

        float inputX = moveLockTimer > 0f ? 0f : moveDir.x * speed;
        rb.linearVelocity = new Vector2(inputX + momentumX + recoilX, rb.linearVelocity.y);
        
        if (!grounded && rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Physics2D.gravity * rb.gravityScale * (fallGravity - 1f), ForceMode2D.Force);
        }

        if (grounded)
        {
            momentumX = Mathf.MoveTowards(momentumX, 0f, momentumBrake * Time.fixedDeltaTime);
        }

        recoilX = Mathf.MoveTowards(recoilX, 0f, recoilBrake * Time.fixedDeltaTime);
    }

    public void OnMove(InputValue value)
    {
        moveDir = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (IsGrounded())
        {
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            return;
        }

        int wallDir = GetWallDir();

        if (wallDir != 0)
        {
            momentumX = -wallDir * wallJumpPower;
            moveLockTimer = wallJumpLockTime;
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }
    }

    public void AddRecoil(Vector2 value)
    {
        if (!IsGrounded())
        {
            momentumX = Mathf.Clamp(momentumX + value.x, -maxMomentumX, maxMomentumX);
        }
        else
        {
            recoilX = Mathf.Clamp(recoilX + value.x, -maxRecoilX, maxRecoilX);
        }

        if (value.y != 0f)
        {
            rb.AddForce(Vector2.up * value.y, ForceMode2D.Impulse);
        }
    }

    private bool IsGrounded()
    {
        int count = playerCollider.Cast(Vector2.down, groundFilter, hits, groundDistance);

        for (int i = 0; i < count; i++)
        {
            if (hits[i].normal.y > groundNormal)
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallDir()
    {
        int leftCount = playerCollider.Cast(Vector2.left, groundFilter, hits, wallDistance);

        for (int i = 0; i < leftCount; i++)
        {
            if (hits[i].normal.x > wallNormal)
            {
                return -1;
            }
        }

        int rightCount = playerCollider.Cast(Vector2.right, groundFilter, hits, wallDistance);

        for (int i = 0; i < rightCount; i++)
        {
            if (hits[i].normal.x < -wallNormal)
            {
                return 1;
            }
        }

        return 0;
    }
}
