using UnityEngine;

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
    
    [SerializeField] private EnemyVision2D vision;
    [SerializeField] private EnemyData enemyData;

    private EnemyState currentState = EnemyState.Idle;
    private float stateTimer;

    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Wander:
                break;
            case EnemyState.Alert:
                break;
            case EnemyState.Chase:
                break;
            case EnemyState.Fire:
                break;
            case EnemyState.Aim:
                break;
            case EnemyState.Retreat:
                break;
            case EnemyState.Cooldown:
                break;
            case EnemyState.Dead:
                break;
        }
    }
    
    private void ChangeState(EnemyState nextState)
    {
        currentState = nextState;

        switch (currentState)
        {
            case EnemyState.Aim:
                stateTimer = enemyData.aimTime;
                break;

            case EnemyState.Cooldown:
                stateTimer = enemyData.cooldown;
                break;
        }
    }
    
    private void UpdateIdle()
    {
        if (HasTarget())
        {
            ChangeState(EnemyState.Alert);
        }
    }
    
    private void UpdateWander()
    {
        if (HasTarget())
        {
            ChangeState(EnemyState.Alert);
        }
    }
        
    private void UpdateAlert()
    {
        ChangeState(IsTargetInAttackRange() ? EnemyState.Aim : EnemyState.Chase);
    }
    
    private void UpdateChase()
    {
        if (!HasTarget())
        {
            ChangeState(EnemyState.Wander);
            return;
        }
        
        MoveTowardTarget();

        if (IsTargetInAttackRange())
        {
            ChangeState(EnemyState.Aim);
        }
    }

    private void UpdateFire()
    {
        FireAtTarget();
        
        if (IsTargetTooClose())
        {
            ChangeState(EnemyState.Retreat);
        }
        else
        {
            ChangeState(EnemyState.Cooldown);
        }
    }
    
    private void UpdateAim()
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
            ChangeState(EnemyState.Fire);
        }
    }
    
    private void UpdateRetreat()
    {
        MoveAwayFromTarget();
        
        ChangeState(EnemyState.Idle);
    }
        
    private void UpdateCooldown()
    {
        
    }
    
    //////////////// 구분선 ////////////////

    private bool HasTarget()
    {
        return vision != null && vision.currentTarget != null;
    }
    
    private float GetTargetDistance()
    {
        return Vector2.Distance(transform.position, vision.currentTarget.position);
    }
    
    private bool IsTargetInAttackRange()
    {
        return enemyData != null && HasTarget() && GetTargetDistance() <= enemyData.attackRange;
    }
    
    private bool IsTargetTooClose()
    {
        return enemyData != null && HasTarget() && GetTargetDistance() <= enemyData.retreatDistance;
    }
    
    private void MoveTowardTarget()
    {
        // 나중에 EnemyMovement로 분리
    }

    private void MoveAwayFromTarget()
    {
        // 나중에 EnemyMovement로 분리
    }

    private void AimAtTarget()
    {
        // 나중에 EnemyAiming으로 분리
    }

    private void FireAtTarget()
    {
        // 나중에 EnemyShooter로 분리
    }
}
