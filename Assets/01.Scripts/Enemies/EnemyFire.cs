using UnityEngine;
using UnityEngine.Serialization;

public class EnemyFire : MonoBehaviour
{
    private const int GizmoSegmentCount = 12;

    [Header("Projectile")]
    [FormerlySerializedAs("projectilePrefab")]
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private Transform firePoint;
    [FormerlySerializedAs("projectileSpeed")]
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private Color trailColor = Color.red;

    [Header("Spread")]
    [SerializeField, Range(0f, 180f)] private float spreadAngle;
    [SerializeField, Min(0.1f)] private float spreadGizmoDistance = 5f;
    [SerializeField] private Color spreadGizmoColor = Color.yellow;

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
        float spreadOffset = Random.Range(-spreadAngle * 0.5f, spreadAngle * 0.5f);
        Vector2 spreadDirection = Quaternion.Euler(0f, 0f, spreadOffset) * direction;

        BulletProjectile bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.LaunchEnemy(spreadDirection, bulletSpeed, trailColor, transform);
    }

    private void OnDrawGizmos()
    {
        Transform originTransform = firePoint != null ? firePoint : transform;
        Vector2 origin = originTransform.position;
        Vector2 forward = originTransform.right;
        float halfSpread = spreadAngle * 0.5f;

        Gizmos.color = spreadGizmoColor;

        Vector2 leftDirection = Quaternion.Euler(0f, 0f, halfSpread) * forward;
        Vector2 rightDirection = Quaternion.Euler(0f, 0f, -halfSpread) * forward;
        Gizmos.DrawLine(origin, origin + leftDirection * spreadGizmoDistance);
        Gizmos.DrawLine(origin, origin + rightDirection * spreadGizmoDistance);

        Vector2 previousPoint = origin + rightDirection * spreadGizmoDistance;

        for (int i = 1; i <= GizmoSegmentCount; i++)
        {
            float angle = Mathf.Lerp(-halfSpread, halfSpread, i / (float)GizmoSegmentCount);
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * forward;
            Vector2 nextPoint = origin + direction * spreadGizmoDistance;
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
}
