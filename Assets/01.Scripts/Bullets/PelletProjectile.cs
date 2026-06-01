using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PelletProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1.5f;

    private Rigidbody2D rb;
    private Transform owner;
    private int damage;
    private int penetration;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed, int damageValue, int penetrationValue, Color trailColor, Transform ownerTransform)
    {
        owner = ownerTransform;
        damage = damageValue;
        penetration = penetrationValue;

        rb.linearVelocity = direction.normalized * speed;
        transform.right = direction;

        TrailRenderer trail = GetComponent<TrailRenderer>();

        if (trail != null)
        {
            trail.startColor = trailColor;
            trail.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Hit(other.transform);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Hit(collision.transform);
    }

    private void Hit(Transform hitTransform)
    {
        if (owner != null && hitTransform.IsChildOf(owner))
        {
            return;
        }

        hitTransform.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        if (penetration <= 0)
        {
            Destroy(gameObject);
            return;
        }

        penetration--;
    }
}
