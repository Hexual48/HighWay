using UnityEngine;
using UnityEngine.Serialization;

public class EnemyFire : MonoBehaviour
{
    [Header("Projectile")]
    [FormerlySerializedAs("projectilePrefab")]
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private Transform firePoint;
    [FormerlySerializedAs("projectileSpeed")]
    [SerializeField] private float bulletSpeed = 12f;

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
        if (target == null || bulletPrefab == null)
        {
            return;
        }

        Vector2 origin = firePoint.position;
        Vector2 direction = ((Vector2)target.position - origin).normalized;

        BulletProjectile bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.Launch(direction, bulletSpeed, damage, penetration, trailColor, transform);
    }
}
