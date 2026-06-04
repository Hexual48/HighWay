using UnityEngine;

[RequireComponent(typeof(EnemyVision2D))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyFire))]
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
    [SerializeField] private EnemyData enemyData;

    [Header("State Time")]
    [SerializeField] private float idleTime = 1f;
    [SerializeField] private float wanderTime = 1.5f;
    [SerializeField] private float alertTime = 0.25f;
    [SerializeField] private float retreatTime = 0.5f;

    [Header("Weapon")]
    [SerializeField] private Transform weaponHandle;
    [SerializeField] private float weaponNormalScaleY = 0.5f;
    [SerializeField] private float weaponFlippedScaleY = -0.5f;

    [Header("Facing")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private bool defaultFacesRight = true;
    [SerializeField, Min(0f)] private float flipDirectionThreshold = 0.1f;

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

        if (bodyRenderer == null)
        {
            bodyRenderer = GetComponentInChildren<SpriteRenderer>();
        }
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

        if (enemyData != null)
        {
            movement?.Move(wanderDirection, enemyData.MoveSpeed);
        }

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

        StopMoving();
        AimAtTarget();
        FireAtTarget();

        ChangeState(IsTargetTooClose() ? EnemyState.Retreat : EnemyState.Cooldown);
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
                stateTimer = idleTime;
                break;
            case EnemyState.Wander:
                wanderDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;
                vision?.SetFacingDirection(wanderDirection);
                FaceDirection(wanderDirection);
                stateTimer = wanderTime;
                break;
            case EnemyState.Alert:
                StopMoving();
                stateTimer = alertTime;
                break;
            case EnemyState.Aim:
                StopMoving();
                stateTimer = enemyData != null ? enemyData.AimTime : 0f;
                break;
            case EnemyState.Retreat:
                stateTimer = retreatTime;
                break;
            case EnemyState.Cooldown:
                StopMoving();
                stateTimer = enemyData != null ? enemyData.Cooldown : 0f;
                break;
            case EnemyState.Dead:
                StopMoving();
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
        return enemyData != null && GetTargetDistance() <= enemyData.AttackRange;
    }

    private bool IsTargetTooClose()
    {
        return enemyData != null && GetTargetDistance() <= enemyData.RetreatDistance;
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
        if (HasTarget() && enemyData != null)
        {
            movement?.MoveToward(vision.currentTarget.position, enemyData.MoveSpeed);
        }
    }

    private void MoveAwayFromTarget()
    {
        if (HasTarget() && enemyData != null)
        {
            movement?.MoveAwayFrom(vision.currentTarget.position, enemyData.MoveSpeed);
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

        vision?.SetFacingDirection(direction);
        FaceDirection(direction);

        if (weaponHandle != null)
        {
            AimWeapon(direction);
        }
    }

    private void FireAtTarget()
    {
        if (HasTarget())
        {
            enemyFire?.FireAt(vision.currentTarget);
        }
    }

    private void FaceDirection(Vector2 direction)
    {
        if (bodyRenderer == null || Mathf.Abs(direction.x) < flipDirectionThreshold)
        {
            return;
        }

        bool shouldFaceRight = direction.x > 0f;
        bodyRenderer.flipX = defaultFacesRight ? !shouldFaceRight : shouldFaceRight;
    }

    private void AimWeapon(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        weaponHandle.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector3 scale = weaponHandle.localScale;
        scale.y = angle > 90f || angle < -90f ? weaponFlippedScaleY : weaponNormalScaleY;
        weaponHandle.localScale = scale;
    }
}
