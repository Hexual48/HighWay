using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    private const float CheckDistance = 0.08f;
    private const float MinContactNormal = 0.5f;
    private const float WallJumpLockTime = 0.12f;
    private const float MomentumBrake = 12f;
    private const float RecoilBrake = 16f;
    private const float MaxAirRecoilX = 18f;
    private const float MaxGroundRecoilX = 18f;

    [Header("Move")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpPower = 12f;

    [Header("Ground")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall")]
    [SerializeField] private float wallJumpPower = 12f;

    private readonly RaycastHit2D[] hits = new RaycastHit2D[4];

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private ContactFilter2D groundFilter;
    private Vector2 moveDir;
    private float momentumX;
    private float airRecoilX;
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

        if (moveLockTimer > 0f)
        {
            moveLockTimer -= Time.fixedDeltaTime;
        }

        if (moveLockTimer <= 0f && momentumX != 0f && (moveDir.x == 0f || Mathf.Sign(moveDir.x) != Mathf.Sign(momentumX)))
        {
            momentumX = 0f;
        }

        float inputX = moveLockTimer > 0f ? 0f : moveDir.x * speed;
        rb.linearVelocity = new Vector2(inputX + momentumX + airRecoilX + recoilX, rb.linearVelocity.y);

        if (!grounded && rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Physics2D.gravity, ForceMode2D.Force);
        }

        if (grounded)
        {
            momentumX = Mathf.MoveTowards(momentumX, 0f, MomentumBrake * Time.fixedDeltaTime);
            airRecoilX = Mathf.MoveTowards(airRecoilX, 0f, MomentumBrake * Time.fixedDeltaTime);
            recoilX = Mathf.MoveTowards(recoilX, 0f, RecoilBrake * Time.fixedDeltaTime);
        }
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
            airRecoilX = Mathf.Clamp(airRecoilX + recoilX, -MaxAirRecoilX, MaxAirRecoilX);
            recoilX = 0f;
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            return;
        }

        int wallDir = GetWallDir();

        if (wallDir != 0)
        {
            momentumX = -wallDir * wallJumpPower;
            moveLockTimer = WallJumpLockTime;
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }
    }

    public void AddRecoil(Vector2 value)
    {
        if (IsGrounded())
        {
            recoilX = Mathf.Clamp(recoilX + value.x, -MaxGroundRecoilX, MaxGroundRecoilX);
        }
        else
        {
            airRecoilX = Mathf.Clamp(airRecoilX + value.x, -MaxAirRecoilX, MaxAirRecoilX);
        }

        rb.linearVelocity += value;
    }

    private bool IsGrounded()
    {
        int count = playerCollider.Cast(Vector2.down, groundFilter, hits, CheckDistance);

        for (int i = 0; i < count; i++)
        {
            if (hits[i].normal.y > MinContactNormal)
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallDir()
    {
        int leftCount = playerCollider.Cast(Vector2.left, groundFilter, hits, CheckDistance);

        for (int i = 0; i < leftCount; i++)
        {
            if (hits[i].normal.x > MinContactNormal)
            {
                return -1;
            }
        }

        int rightCount = playerCollider.Cast(Vector2.right, groundFilter, hits, CheckDistance);

        for (int i = 0; i < rightCount; i++)
        {
            if (hits[i].normal.x < -MinContactNormal)
            {
                return 1;
            }
        }

        return 0;
    }
}
