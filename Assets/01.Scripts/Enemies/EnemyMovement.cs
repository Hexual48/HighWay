using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private static readonly HashSet<EnemyMovement> ActiveEnemies = new HashSet<EnemyMovement>();

    [Header("Enemy Separation")]
    [SerializeField, Min(0f)] private float minimumHorizontalSpacing = 1.5f;
    [SerializeField, Min(0f)] private float verticalSeparationRange = 1f;
    [SerializeField, Min(0f)] private float separationSpeed = 4f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        ActiveEnemies.Add(this);
    }

    private void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }

    private void FixedUpdate()
    {
        SeparateFromNearbyEnemies();
    }

    public void MoveToward(Vector2 targetPosition, float speed)
    {
        Move((targetPosition - (Vector2)transform.position).normalized, speed);
    }

    public void MoveAwayFrom(Vector2 targetPosition, float speed)
    {
        Move(((Vector2)transform.position - targetPosition).normalized, speed);
    }

    public void Move(Vector2 direction, float speed)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            Stop();
            return;
        }

        Vector2 moveDirection = direction.normalized;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(moveDirection.x * speed, rb.linearVelocity.y);
            return;
        }

        transform.position += (Vector3)(moveDirection * (speed * Time.deltaTime));
    }

    public void Stop()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private void SeparateFromNearbyEnemies()
    {
        if (minimumHorizontalSpacing <= 0f || separationSpeed <= 0f)
        {
            return;
        }

        Vector2 currentPosition = rb != null ? rb.position : (Vector2)transform.position;
        float separationX = 0f;

        foreach (EnemyMovement other in ActiveEnemies)
        {
            if (other == null || other == this || !other.isActiveAndEnabled)
            {
                continue;
            }

            Vector2 otherPosition = other.rb != null
                ? other.rb.position
                : (Vector2)other.transform.position;

            if (Mathf.Abs(currentPosition.y - otherPosition.y) > verticalSeparationRange)
            {
                continue;
            }

            float horizontalOffset = currentPosition.x - otherPosition.x;
            float horizontalDistance = Mathf.Abs(horizontalOffset);

            if (horizontalDistance >= minimumHorizontalSpacing)
            {
                continue;
            }

            float pushDirection = horizontalDistance > Mathf.Epsilon
                ? Mathf.Sign(horizontalOffset)
                : (GetInstanceID() < other.GetInstanceID() ? -1f : 1f);

            separationX += pushDirection * (minimumHorizontalSpacing - horizontalDistance) * 0.5f;
        }

        float maxSeparation = separationSpeed * Time.fixedDeltaTime;
        separationX = Mathf.Clamp(separationX, -maxSeparation, maxSeparation);

        if (Mathf.Abs(separationX) <= Mathf.Epsilon)
        {
            return;
        }

        Vector2 separatedPosition = currentPosition + Vector2.right * separationX;

        if (rb != null)
        {
            rb.MovePosition(separatedPosition);
        }
        else
        {
            transform.position = separatedPosition;
        }
    }
}
