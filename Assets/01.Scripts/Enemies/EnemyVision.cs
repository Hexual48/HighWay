using System.Collections.Generic;
using UnityEngine;

public class EnemyVision2D : MonoBehaviour
{
    private static readonly HashSet<EnemyVision2D> ActiveVisions = new HashSet<EnemyVision2D>();
    private const int GizmoSegmentCount = 16;

    [Header("DetectRange")]
    [SerializeField] private Transform origin;
    [SerializeField, Min(0.1f)] private float viewDistance = 6f;
    [SerializeField, Range(1f, 360f)] private float viewAngle = 70f;
    [SerializeField] private EnemyAiming aiming;

    [Header("Gizmo")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f);
    [SerializeField] private Color targetLineGizmoColor = new Color(1f, 1f, 0f);

    [Header("Detection")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float scanInterval = 0.1f;
    [SerializeField] private Vector2 facingDirection = Vector2.right;

    [Header("Shared Detection")]
    [SerializeField, Min(0f)] private float sharedDetectionRadius = 8f;
    [SerializeField, Min(0f)] private float sharedDetectionMemory = 1f;

    private float scanTimer;
    private float sharedDetectionTimer;
    public Transform currentTarget;
    
    private void Awake()
    {
        origin = transform;

        if (aiming == null)
        {
            aiming = GetComponent<EnemyAiming>();
        }
    }

    private void OnEnable()
    {
        ActiveVisions.Add(this);
    }

    private void OnDisable()
    {
        ActiveVisions.Remove(this);
    }
    
    private void Update()
    {
        if (!IsTargetValid(currentTarget))
        {
            currentTarget = null;
            sharedDetectionTimer = 0f;
        }

        scanTimer -= Time.deltaTime;
        sharedDetectionTimer = Mathf.Max(0f, sharedDetectionTimer - Time.deltaTime);

        if (scanTimer <= 0f)
        {
            scanTimer = scanInterval;
            ScanTargets();
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 scanOrigin = origin != null ? origin.position : transform.position;
        Vector2 forward = GetFacingDirection();

        float baseAngle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - viewAngle * 0.5f;
        float angleStep = viewAngle / GizmoSegmentCount;

        Gizmos.color = gizmoColor;

        float startRadians = startAngle * Mathf.Deg2Rad;
        Vector2 previousPoint = scanOrigin + new Vector2(Mathf.Cos(startRadians), Mathf.Sin(startRadians)) * viewDistance;
        Gizmos.DrawLine(scanOrigin, previousPoint);

        for (int i = 1; i <= GizmoSegmentCount; i++)
        {
            float angle = startAngle + angleStep * i;
            float radians = angle * Mathf.Deg2Rad;
            Vector2 nextPoint = scanOrigin + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * viewDistance;
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }

        Gizmos.DrawLine(scanOrigin, previousPoint);

        if (currentTarget != null)
        {
            Gizmos.color = targetLineGizmoColor;
            Gizmos.DrawLine(scanOrigin, currentTarget.position);
        }
    }

    private void ScanTargets()
    {
        Vector2 scanOrigin = origin != null ? origin.position : transform.position;
        Vector2 forward = GetFacingDirection();

        float halfAngle = viewAngle * 0.5f;
        Transform detectedTarget = null;

        Collider2D targetCollider = Physics2D.OverlapCircle(scanOrigin, viewDistance, targetLayer);

        if (targetCollider != null && IsTargetValid(targetCollider.transform))
        {
            Vector2 targetPoint = targetCollider.ClosestPoint(scanOrigin);

            if ((targetPoint - scanOrigin).sqrMagnitude <= Mathf.Epsilon)
            {
                targetPoint = targetCollider.transform.position;
            }

            Vector2 targetDirection = targetPoint - scanOrigin;
            float targetDistanceSqr = targetDirection.sqrMagnitude;

            if (targetDistanceSqr > Mathf.Epsilon && targetDistanceSqr <= viewDistance * viewDistance)
            {
                float targetDistance = Mathf.Sqrt(targetDistanceSqr);
                bool isInViewAngle = Vector2.Angle(forward, targetDirection) <= halfAngle;
                bool isBlocked = HasObstacle(scanOrigin, targetDirection.normalized, targetDistance);

                if (isInViewAngle && !isBlocked)
                {
                    detectedTarget = targetCollider.transform;
                }
            }
        }
        
        if (detectedTarget != null)
        {
            currentTarget = detectedTarget;
            sharedDetectionTimer = 0f;
            AlertNearbyEnemies(detectedTarget);
            return;
        }

        if (sharedDetectionTimer <= 0f)
        {
            currentTarget = null;
        }
    }

    private bool HasObstacle(Vector2 scanOrigin, Vector2 direction, float distance)
    {
        if (obstacleLayer.value == 0)
        {
            return false;
        }

        return Physics2D.Raycast(scanOrigin, direction, distance, obstacleLayer);
    }

    private void AlertNearbyEnemies(Transform target)
    {
        if (!IsTargetValid(target) || sharedDetectionRadius <= 0f)
        {
            return;
        }

        Vector2 scanOrigin = origin != null ? origin.position : transform.position;
        float sharedDetectionRadiusSqr = sharedDetectionRadius * sharedDetectionRadius;

        foreach (EnemyVision2D other in ActiveVisions)
        {
            if (other == null || other == this || !other.isActiveAndEnabled)
            {
                continue;
            }

            Vector2 otherPosition = other.origin != null ? other.origin.position : other.transform.position;

            if ((otherPosition - scanOrigin).sqrMagnitude > sharedDetectionRadiusSqr)
            {
                continue;
            }

            other.ReceiveSharedDetection(target);
        }
    }

    private void ReceiveSharedDetection(Transform target)
    {
        if (!IsTargetValid(target))
        {
            return;
        }

        currentTarget = target;
        sharedDetectionTimer = sharedDetectionMemory;

        Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
        aiming?.Face(direction);
        SetFacingDirection(direction);
    }

    public void SetFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        facingDirection = direction.normalized;
    }

    public Vector2 GetFacingDirection()
    {
        if (facingDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return Vector2.right;
        }

        return facingDirection.normalized;
    }

    public void SetFacingDirectionX(float directionX)
    {
        if (Mathf.Approximately(directionX, 0f))
        {
            return;
        }

        facingDirection = directionX > 0f ? Vector2.right : Vector2.left;
    }

    internal static bool IsTargetValid(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        PlayerHealth playerHealth = target.GetComponentInParent<PlayerHealth>();
        return playerHealth == null || !playerHealth.IsDead;
    }
}
