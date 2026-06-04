using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
}
