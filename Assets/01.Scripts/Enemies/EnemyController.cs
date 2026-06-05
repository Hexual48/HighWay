using UnityEngine;

[RequireComponent(typeof(EnemyVision2D))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyFire))]
[RequireComponent(typeof(EnemyAiming))]
public class EnemyController : MonoBehaviour
{
    private enum EnemyState
    {
        Idle,
        Wander,
        Alert,
        Chase,
        Aim,
        Fire,
        Retreat,
        Cooldown,
        Dead
    }

    [Header("References")]
    [SerializeField] private EnemyVision2D vision;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyFire enemyFire;
    [SerializeField] private EnemyAiming aiming;

    [Header("State Time")]
    [SerializeField] private float idleTime = 1f;
    [SerializeField] private float wanderTime = 1.5f;
    [SerializeField, Min(0f)] private float alertTime = 0.25f;
    [SerializeField] private float aimTime = 0.4f;
    [SerializeField] private float cooldown = 0.8f;
    [SerializeField] private float retreatTime = 0.5f;

    [Header("Alert Visual")]
    [SerializeField] private SpriteRenderer alertRenderer;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 2f;

    [Header("Combat Range")]
    [SerializeField, Min(0.1f)] private float attackRange = 5f;
    [SerializeField, Min(0f)] private float retreatDistance = 1.5f;
    [SerializeField] private Color attackRangeGizmoColor = Color.red;
    [SerializeField] private Color retreatDistanceGizmoColor = Color.cyan;

    private EnemyState currentState = EnemyState.Idle;
    private Vector2 wanderDirection = Vector2.right;
    private float stateTimer;

    private void Awake()
    {
        if (vision == null)
        {
            vision = GetComponent<EnemyVision2D>();
        }

        if (movement == null)
        {
            movement = GetComponent<EnemyMovement>();
        }

        if (enemyFire == null)
        {
            enemyFire = GetComponent<EnemyFire>();
        }

        if (aiming == null)
        {
            aiming = GetComponent<EnemyAiming>();
        }

        if (alertRenderer == null)
        {
            Transform alert = transform.Find("AlertPos/Alert");

            if (alert != null)
            {
                alertRenderer = alert.GetComponent<SpriteRenderer>();
            }
        }

        HideAlert();
    }

    private void Start()
    {
        ChangeState(EnemyState.Idle);
    }

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Wander:
                UpdateWander();
                break;
            case EnemyState.Alert:
                UpdateAlert();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Aim:
                UpdateAim();
                break;
            case EnemyState.Fire:
                UpdateFire();
                break;
            case EnemyState.Retreat:
                UpdateRetreat();
                break;
            case EnemyState.Cooldown:
                UpdateCooldown();
                break;
            case EnemyState.Dead:
                StopMoving();
                break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = attackRangeGizmoColor;
        Gizmos.DrawWireSphere(transform.position, GetAttackRange());

        Gizmos.color = retreatDistanceGizmoColor;
        Gizmos.DrawWireSphere(transform.position, GetRetreatDistance());
    }

    private void UpdateIdle()
    {
        if (HasTarget())
        {
            ChangeState(EnemyState.Alert);
            return;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            ChangeState(EnemyState.Wander);
        }
    }

    private void UpdateWander()
    {
        if (HasTarget())
        {
            ChangeState(EnemyState.Alert);
            return;
        }

        movement?.Move(wanderDirection, moveSpeed);

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    private void UpdateAlert()
    {
        if (!HasTarget())
        {
            ChangeState(EnemyState.Wander);
            return;
        }

        AimAtTarget();
        UpdateAlertVisual();

        if (IsTargetTooClose())
        {
            ChangeState(EnemyState.Retreat);
            return;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            ChangeState(IsTargetInAttackRange() ? EnemyState.Aim : EnemyState.Chase);
        }
    }

    private void UpdateChase()
    {
        if (!HasTarget())
        {
            ChangeState(EnemyState.Wander);
            return;
        }

        AimAtTarget();

        if (IsTargetTooClose())
        {
            ChangeState(EnemyState.Retreat);
            return;
        }

        MoveTowardTarget();

        if (IsTargetInAttackRange())
        {
            ChangeState(EnemyState.Aim);
        }
    }

    private void UpdateAim()
    {
        if (!HasTarget())
        {
            ChangeState(EnemyState.Wander);
            return;
        }

        if (IsTargetTooClose())
        {
            ChangeState(EnemyState.Retreat);
            return;
        }

        if (!IsTargetInAttackRange())
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        StopMoving();
        AimAtTarget();
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            ChangeState(EnemyState.Fire);
        }
    }

    private void UpdateFire()
    {
        if (!HasTarget())
        {
            ChangeState(EnemyState.Cooldown);
            return;
        }

        if (IsTargetTooClose())
        {
            ChangeState(EnemyState.Retreat);
            return;
        }

        StopMoving();
        AimAtTarget();
        FireAtTarget();

        ChangeState(EnemyState.Cooldown);
    }

    private void UpdateRetreat()
    {
        if (!HasTarget())
        {
            ChangeState(EnemyState.Wander);
            return;
        }

        AimAtTarget();
        MoveAwayFromTarget();
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f || !IsTargetTooClose())
        {
            ChangeState(EnemyState.Cooldown);
        }
    }

    private void UpdateCooldown()
    {
        if (!HasTarget())
        {
            ChangeState(EnemyState.Wander);
            return;
        }

        AimAtTarget();

        if (IsTargetTooClose())
        {
            ChangeState(EnemyState.Retreat);
            return;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer > 0f)
        {
            return;
        }

        ChangeState(IsTargetInAttackRange() ? EnemyState.Aim : EnemyState.Chase);
    }

    private void ChangeState(EnemyState nextState)
    {
        currentState = nextState;

        switch (currentState)
        {
            case EnemyState.Idle:
                StopMoving();
                HideAlert();
                stateTimer = idleTime;
                break;
            case EnemyState.Wander:
                HideAlert();
                wanderDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;
                aiming?.Face(wanderDirection);
                stateTimer = wanderTime;
                break;
            case EnemyState.Alert:
                StopMoving();
                stateTimer = alertTime;
                ShowAlert();
                break;
            case EnemyState.Aim:
                StopMoving();
                HideAlert();
                stateTimer = aimTime;
                break;
            case EnemyState.Retreat:
                HideAlert();
                stateTimer = retreatTime;
                break;
            case EnemyState.Cooldown:
                StopMoving();
                HideAlert();
                stateTimer = cooldown;
                break;
            case EnemyState.Dead:
                StopMoving();
                HideAlert();
                break;
            default:
                HideAlert();
                break;
        }
    }

    private bool HasTarget()
    {
        return vision != null && vision.currentTarget != null;
    }

    private float GetTargetDistance()
    {
        if (!HasTarget())
        {
            return float.MaxValue;
        }

        return Vector2.Distance(transform.position, vision.currentTarget.position);
    }

    private bool IsTargetInAttackRange()
    {
        return GetTargetDistance() <= GetAttackRange();
    }

    private bool IsTargetTooClose()
    {
        return GetTargetDistance() <= GetRetreatDistance();
    }

    private float GetAttackRange()
    {
        return attackRange;
    }

    private float GetRetreatDistance()
    {
        return retreatDistance;
    }

    private Vector2 GetDirectionToTarget()
    {
        if (!HasTarget())
        {
            return Vector2.zero;
        }

        Vector2 direction = vision.currentTarget.position - transform.position;
        return direction.sqrMagnitude <= Mathf.Epsilon ? Vector2.zero : direction.normalized;
    }

    private void MoveTowardTarget()
    {
        if (HasTarget())
        {
            movement?.MoveToward(vision.currentTarget.position, moveSpeed);
        }
    }

    private void MoveAwayFromTarget()
    {
        if (HasTarget())
        {
            movement?.MoveAwayFrom(vision.currentTarget.position, moveSpeed);
        }
    }

    private void StopMoving()
    {
        movement?.Stop();
    }

    private void AimAtTarget()
    {
        Vector2 direction = GetDirectionToTarget();

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        aiming?.Aim(direction);
    }

    private void FireAtTarget()
    {
        if (HasTarget())
        {
            enemyFire?.FireAt(vision.currentTarget);
        }
    }

    private void ShowAlert()
    {
        if (alertRenderer == null)
        {
            return;
        }

        alertRenderer.enabled = true;
        Color color = alertRenderer.color;
        color.a = 1f;
        alertRenderer.color = color;
    }

    private void HideAlert()
    {
        if (alertRenderer == null)
        {
            return;
        }

        Color color = alertRenderer.color;
        color.a = 0f;
        alertRenderer.color = color;
        alertRenderer.enabled = false;
    }

    private void UpdateAlertVisual()
    {
        if (alertRenderer == null)
        {
            return;
        }

        float alpha = alertTime > 0f ? Mathf.Clamp01(stateTimer / alertTime) : 0f;
        Color color = alertRenderer.color;
        color.a = alpha;
        alertRenderer.color = color;
    }
}
