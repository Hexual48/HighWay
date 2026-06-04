using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    public float attackRange;
    public float retreatDistance;
    public float aimTime;
    public float cooldown;
    public float moveSpeed;
    
    public float AttackRange => attackRange;
    public float RetreatDistance => retreatDistance;
    public float AimTime => aimTime;
    public float Cooldown => cooldown;
    public float MoveSpeed => moveSpeed;
}
