using UnityEngine;

public class EnemyFire : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private PelletProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float projectileSpeed = 12f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private int penetration;
    [SerializeField] private Color trailColor = Color.red;

    private void Awake()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    public void FireAt(Transform target)
    {
        if (target == null || projectilePrefab == null)
        {
            return;
        }

        Vector2 origin = firePoint.position;
        Vector2 direction = ((Vector2)target.position - origin).normalized;

        PelletProjectile projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        projectile.Launch(direction, projectileSpeed, damage, penetration, trailColor, transform);
    }
}
