using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.08f;

    [Header("External Velocity")]
    [SerializeField] private float externalVelocityRecovery = 35f;

    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];

    private Rigidbody2D rigidBody;
    private Collider2D playerCollider;
    private Vector2 moveInput;
    private Vector2 externalVelocity;
    private Vector2 previousAppliedExternalVelocity;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void OnDisable()
    {
        externalVelocity = Vector2.zero;
        previousAppliedExternalVelocity = Vector2.zero;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed || !IsGrounded())
        {
            return;
        }

        Vector2 velocity = rigidBody.linearVelocity - previousAppliedExternalVelocity;
        velocity.y = jumpForce;
        rigidBody.linearVelocity = velocity + externalVelocity;
        previousAppliedExternalVelocity = externalVelocity;
    }

    public void AddExternalVelocity(Vector2 velocity)
    {
        externalVelocity += velocity;
    }

    private void Move()
    {
        Vector2 velocity = rigidBody.linearVelocity - previousAppliedExternalVelocity;
        velocity.x = GetMoveDirection() * moveSpeed;

        rigidBody.linearVelocity = velocity + externalVelocity;
        previousAppliedExternalVelocity = externalVelocity;

        externalVelocity = Vector2.MoveTowards(
            externalVelocity,
            Vector2.zero,
            externalVelocityRecovery * Time.fixedDeltaTime);
    }

    private float GetMoveDirection()
    {
        if (moveInput.x > 0.01f)
        {
            return 1f;
        }

        if (moveInput.x < -0.01f)
        {
            return -1f;
        }

        return 0f;
    }

    private bool IsGrounded()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        filter.useLayerMask = true;
        filter.useTriggers = false;

        int hitCount = playerCollider.Cast(
            Vector2.down,
            filter,
            groundHits,
            groundCheckDistance);

        return hitCount > 0;
    }
}
