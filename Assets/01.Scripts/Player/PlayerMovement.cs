using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpPower = 12f;

    [Header("Ground")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDistance = 0.08f;

    [Header("Extra")]
    [SerializeField] private float brake = 35f;
    [SerializeField] private float stopDeadzone = 0.01f;

    private readonly RaycastHit2D[] hits = new RaycastHit2D[1];

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private ContactFilter2D groundFilter;
    private Vector2 moveDir;
    private float extraX;

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
        if (moveDir.x != 0f && extraX != 0f && Mathf.Sign(moveDir.x) != Mathf.Sign(extraX))
        {
            extraX = 0f;
        }

        Vector2 vel = rb.linearVelocity;
        vel.x = moveDir.x * speed + extraX;
        rb.linearVelocity = vel;

        if (Mathf.Abs(moveDir.x) <= stopDeadzone)
        {
            extraX = Mathf.MoveTowards(extraX, 0f, brake * Time.fixedDeltaTime);
        }
    }

    public void OnMove(InputValue value)
    {
        moveDir = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed || !IsGrounded())
        {
            return;
        }

        Vector2 velocity = rb.linearVelocity;
        velocity.y = jumpPower;
        rb.linearVelocity = velocity;
    }

    public void AddRecoil(Vector2 value)
    {
        extraX += value.x;

        if (value.y != 0f)
        {
            rb.AddForce(Vector2.up * value.y, ForceMode2D.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return playerCollider.Cast(Vector2.down, groundFilter, hits, groundDistance) > 0;
    }
}
