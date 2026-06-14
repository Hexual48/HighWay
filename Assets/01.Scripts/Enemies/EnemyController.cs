using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyVision2D vision;
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyFire enemyFire;
    [SerializeField] private EnemyAiming aiming;
    [SerializeField] private EnemyHoldShoot holdShoot;

    [Header("State Time")]
    [SerializeField] private float idleTime = 1f;
    [SerializeField] private float wanderTime = 1.5f;
    [SerializeField, Min(0f)] private float alertTime = 0.25f;
    [SerializeField] private float aimTime = 0.4f;
    [SerializeField] private float cooldown = 0.8f;
    [SerializeField] private float retreatTime = 0.5f;

    [Header("Fire")]
    [SerializeField, Min(1)] private int burstShotCount = 1;
    [SerializeField, Min(0f)] private float fireCooldown = 0.1f;

    [Header("Reload")]
    [SerializeField, Min(1)] private int magazineSize = 5;
    [SerializeField, Min(0f)] private float reloadTime = 1.5f;

    [Header("Alert Visual")]
    [SerializeField] private SpriteRenderer alertRenderer;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 2f;

    [Header("Combat Range")]
    [SerializeField, Min(0.1f)] private float attackRange = 5f;
    [SerializeField, Min(0f)] private float retreatDistance = 1.5f;
    [SerializeField] private Color attackRangeGizmoColor = Color.red;
    [SerializeField] private Color retreatDistanceGizmoColor = Color.cyan;

    private EnemyMachine machine;

    public bool IsAiming => machine != null && machine.IsAiming;
    public bool IsFiring => machine != null && machine.IsFiring;
    public Transform CurrentTarget => vision != null ? vision.currentTarget : null;
    public float AimTimeRemaining => machine != null ? machine.AimTimeRemaining : 0f;
    public float AimDuration => aimTime;

    internal float IdleTime => idleTime;
    internal float WanderTime => wanderTime;
    internal float AlertTime => alertTime;
    internal float AimTime => aimTime;
    internal float Cooldown => cooldown;
    internal float RetreatTime => retreatTime;
    internal int BurstShotCount => burstShotCount;
    internal float FireCooldown => fireCooldown;
    internal int MagazineSize => magazineSize;
    internal float ReloadTime => reloadTime;

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

        if (holdShoot == null)
        {
            holdShoot = GetComponent<EnemyHoldShoot>();
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
        machine = new EnemyMachine(this);
    }

    private void Start()
    {
        machine.Start();
    }

    private void Update()
    {
        machine.Tick(Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = attackRangeGizmoColor;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = retreatDistanceGizmoColor;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }

    internal bool HasTarget()
    {
        return CurrentTarget != null;
    }

    internal bool IsTargetInAttackRange()
    {
        return GetTargetDistance(CurrentTarget) <= attackRange;
    }

    internal bool IsTargetTooClose()
    {
        return IsTargetTooClose(CurrentTarget);
    }

    internal bool IsTargetTooClose(Transform target)
    {
        return GetTargetDistance(target) <= retreatDistance;
    }

    internal void Move(Vector2 direction)
    {
        movement?.Move(direction, moveSpeed);
    }

    internal void MoveTowardTarget()
    {
        if (HasTarget())
        {
            movement?.MoveToward(CurrentTarget.position, moveSpeed);
        }
    }

    internal void MoveAwayFromTarget()
    {
        if (HasTarget())
        {
            movement?.MoveAwayFrom(CurrentTarget.position, moveSpeed);
        }
    }

    internal void StopMoving()
    {
        movement?.Stop();
    }

    internal void AimAtTarget()
    {
        AimAtTarget(CurrentTarget);
    }

    internal void AimAtTarget(Transform target)
    {
        if (target == null)
        {
            return;
        }

        Vector2 direction = target.position - transform.position;

        if (direction.sqrMagnitude > Mathf.Epsilon)
        {
            aiming?.Aim(direction.normalized);
        }
    }

    internal void AimForward()
    {
        aiming?.AimForward();
    }

    internal void Face(Vector2 direction)
    {
        aiming?.Face(direction);
    }

    internal Transform GetFireTarget()
    {
        if (HasTarget())
        {
            return CurrentTarget;
        }

        return holdShoot != null ? holdShoot.HeldTarget : null;
    }

    internal void FireAtTarget(Transform target)
    {
        enemyFire?.FireAt(target);
    }

    internal void BeginHold()
    {
        holdShoot?.BeginHold(CurrentTarget);
    }

    internal void EndHold()
    {
        holdShoot?.EndHold();
    }

    internal float GetReloadDuration()
    {
        return aiming != null ? aiming.GetReloadDuration(reloadTime) : reloadTime;
    }

    internal void BeginReload()
    {
        aiming?.BeginReload();
    }

    internal void UpdateReload(float elapsedTime)
    {
        aiming?.UpdateReload(elapsedTime, reloadTime);
    }

    internal void ShowAlert()
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

    internal void HideAlert()
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

    internal void UpdateAlertVisual(float timeRemaining)
    {
        if (alertRenderer == null)
        {
            return;
        }

        float alpha = alertTime > 0f ? Mathf.Clamp01(timeRemaining / alertTime) : 0f;
        Color color = alertRenderer.color;
        color.a = alpha;
        alertRenderer.color = color;
    }

    private float GetTargetDistance(Transform target)
    {
        return target != null
            ? Vector2.Distance(transform.position, target.position)
            : float.MaxValue;
    }
}
