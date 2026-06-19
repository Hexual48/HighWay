using UnityEngine;

public sealed class EnemyMachine
{
    public enum State
    {
        Idle,
        Wander,
        Alert,
        Chase,
        Aim,
        Fire,
        Reload,
        Retreat,
        Cooldown,
        Dead
    }

    private readonly EnemyController controller;

    private State currentState;
    private Vector2 wanderDirection = Vector2.right;
    private float stateTimer;
    private int currentAmmo;
    private int firedShotCount;

    public State CurrentState => currentState;
    public bool IsAiming => currentState == State.Aim;
    public bool IsFiring => currentState == State.Fire;
    public float AimTimeRemaining => IsAiming ? Mathf.Max(0f, stateTimer) : 0f;

    public EnemyMachine(EnemyController controller)
    {
        this.controller = controller;
    }

    public void Start()
    {
        currentAmmo = controller.MagazineSize;
        ChangeState(State.Idle);
    }

    public void Tick(float deltaTime)
    {
        switch (currentState)
        {
            case State.Idle:
                UpdateIdle(deltaTime);
                break;
            case State.Wander:
                UpdateWander(deltaTime);
                break;
            case State.Alert:
                UpdateAlert(deltaTime);
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.Aim:
                UpdateAim(deltaTime);
                break;
            case State.Fire:
                UpdateFire(deltaTime);
                break;
            case State.Reload:
                UpdateReload(deltaTime);
                break;
            case State.Retreat:
                UpdateRetreat(deltaTime);
                break;
            case State.Cooldown:
                UpdateCooldown(deltaTime);
                break;
            case State.Dead:
                controller.StopMoving();
                break;
        }
    }

    public void ChangeState(State nextState)
    {
        currentState = nextState;

        switch (currentState)
        {
            case State.Idle:
                controller.StopMoving();
                controller.HideAlert();
                controller.AimForward();
                stateTimer = controller.IdleTime;
                break;
            case State.Wander:
                controller.HideAlert();
                wanderDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;
                controller.Face(wanderDirection);
                stateTimer = controller.WanderTime;
                break;
            case State.Alert:
                controller.StopMoving();
                stateTimer = controller.AlertTime;
                controller.ShowAlert();
                break;
            case State.Aim:
                controller.StopMoving();
                controller.HideAlert();
                stateTimer = controller.AimTime;
                break;
            case State.Fire:
                controller.StopMoving();
                controller.HideAlert();
                firedShotCount = 0;
                stateTimer = 0f;
                break;
            case State.Reload:
                controller.StopMoving();
                controller.HideAlert();
                stateTimer = controller.GetReloadDuration();
                controller.BeginReload();
                break;
            case State.Retreat:
                controller.HideAlert();
                stateTimer = controller.RetreatTime;
                break;
            case State.Cooldown:
                controller.StopMoving();
                controller.HideAlert();
                stateTimer = controller.Cooldown;
                break;
            case State.Dead:
                controller.StopMoving();
                controller.HideAlert();
                break;
            default:
                controller.HideAlert();
                break;
        }
    }

    private void UpdateIdle(float deltaTime)
    {
        if (controller.HasTarget())
        {
            ChangeState(State.Alert);
            return;
        }

        stateTimer -= deltaTime;

        if (stateTimer <= 0f)
        {
            ChangeState(State.Wander);
        }
    }

    private void UpdateWander(float deltaTime)
    {
        if (controller.HasTarget())
        {
            ChangeState(State.Alert);
            return;
        }

        controller.Move(wanderDirection);
        stateTimer -= deltaTime;

        if (stateTimer <= 0f)
        {
            ChangeState(State.Idle);
        }
    }

    private void UpdateAlert(float deltaTime)
    {
        if (!controller.HasTarget())
        {
            ChangeState(State.Idle);
            return;
        }

        controller.AimAtTarget();
        controller.UpdateAlertVisual(stateTimer);

        if (controller.IsTargetTooClose())
        {
            ChangeState(State.Retreat);
            return;
        }

        stateTimer -= deltaTime;

        if (stateTimer <= 0f)
        {
            ChangeState(controller.IsTargetInAttackRange() ? State.Aim : State.Chase);
        }
    }

    private void UpdateChase()
    {
        if (!controller.HasTarget())
        {
            ChangeState(State.Idle);
            return;
        }

        controller.AimAtTarget();

        if (controller.IsTargetTooClose())
        {
            ChangeState(State.Retreat);
            return;
        }

        controller.MoveTowardTarget();

        if (controller.IsTargetInAttackRange())
        {
            ChangeState(State.Aim);
        }
    }

    private void UpdateAim(float deltaTime)
    {
        if (!controller.HasTarget())
        {
            ChangeState(State.Idle);
            return;
        }

        if (controller.IsTargetTooClose())
        {
            ChangeState(State.Retreat);
            return;
        }

        if (!controller.IsTargetInAttackRange())
        {
            ChangeState(State.Chase);
            return;
        }

        controller.StopMoving();
        controller.AimAtTarget();
        stateTimer -= deltaTime;

        if (stateTimer <= 0f)
        {
            ChangeState(State.Fire);
        }
    }

    private void UpdateFire(float deltaTime)
    {
        Transform fireTarget = controller.CurrentTarget;

        if (fireTarget == null)
        {
            ChangeState(State.Idle);
            return;
        }

        if (controller.IsTargetTooClose(fireTarget))
        {
            ChangeState(State.Retreat);
            return;
        }

        controller.StopMoving();
        controller.AimAtTarget(fireTarget);
        stateTimer -= deltaTime;

        if (stateTimer > 0f)
        {
            return;
        }

        controller.FireAtTarget(fireTarget);
        firedShotCount++;
        currentAmmo--;

        if (currentAmmo <= 0)
        {
            ChangeState(State.Reload);
            return;
        }

        if (firedShotCount >= controller.BurstShotCount)
        {
            ChangeState(State.Cooldown);
            return;
        }

        stateTimer = controller.FireCooldown;
    }

    private void UpdateReload(float deltaTime)
    {
        controller.StopMoving();
        stateTimer -= deltaTime;
        float elapsedTime = controller.GetReloadDuration() - Mathf.Max(0f, stateTimer);
        controller.UpdateReload(elapsedTime);

        if (stateTimer > 0f)
        {
            return;
        }

        currentAmmo = controller.MagazineSize;

        if (!controller.HasTarget())
        {
            ChangeState(State.Idle);
        }
        else if (controller.IsTargetTooClose())
        {
            ChangeState(State.Retreat);
        }
        else
        {
            ChangeState(controller.IsTargetInAttackRange() ? State.Aim : State.Chase);
        }
    }

    private void UpdateRetreat(float deltaTime)
    {
        if (!controller.HasTarget())
        {
            ChangeState(State.Idle);
            return;
        }

        controller.AimAtTarget();
        controller.MoveAwayFromTarget();
        stateTimer -= deltaTime;

        if (stateTimer <= 0f || !controller.IsTargetTooClose())
        {
            ChangeState(State.Cooldown);
        }
    }

    private void UpdateCooldown(float deltaTime)
    {
        if (!controller.HasTarget())
        {
            ChangeState(State.Idle);
            return;
        }

        controller.AimAtTarget();

        if (controller.IsTargetTooClose()) 
        {
            ChangeState(State.Retreat);
            return;
        }

        stateTimer -= deltaTime;

        if (stateTimer <= 0f)
        {
            ChangeState(controller.IsTargetInAttackRange() ? State.Aim : State.Chase);
        }
    }
}
