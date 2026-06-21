using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private struct EnemyHitCandidate
    {
        public Transform HitTransform;
        public EnemyHealth Enemy;
        public EnemyArmor Armor;

        public int Priority => Armor != null ? Armor.HitPriority : 0;
    }

    private const int EnemyBulletDamage = 20;
    private const int EnemyBulletPenetration = 0;
    private const string PlayerBulletLayerName = "PlayerBullet";

    [SerializeField] private float lifeTime = 1.5f;
    [SerializeField] private LayerMask blockingLayers = (1 << 0) | (1 << 3) | (1 << 7) | (1 << 10);

    private Rigidbody2D rb;
    private Collider2D bulletCollider;
    private Transform owner;
    private Vector2 startPosition;
    private float maxTravelDistance;
    private int damage;
    private int penetration;
    private bool isPlayerBullet;
    private readonly HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();
    private readonly HashSet<EnemyHealth> armorProtectedEnemies = new HashSet<EnemyHealth>();
    private readonly HashSet<EnemyArmor> hitArmors = new HashSet<EnemyArmor>();
    private readonly Dictionary<EnemyHealth, EnemyHitCandidate> pendingEnemyHits = new Dictionary<EnemyHealth, EnemyHitCandidate>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bulletCollider = GetComponent<Collider2D>();

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        if (bulletCollider != null)
        {
            bulletCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        ResolvePendingEnemyHits();

        if (maxTravelDistance <= 0f)
        {
            return;
        }

        float maxTravelDistanceSqr = maxTravelDistance * maxTravelDistance;

        if (((Vector2)transform.position - startPosition).sqrMagnitude >= maxTravelDistanceSqr)
        {
            Destroy(gameObject);
        }
    }

    public void Launch(Vector2 direction, float speed, int damageValue, int penetrationValue, Color trailColor, Transform ownerTransform, float maxDistance = 0f)
    {
        owner = ownerTransform;
        isPlayerBullet = ownerTransform != null && ownerTransform.GetComponentInParent<PlayerHealth>() != null;

        if (isPlayerBullet)
        {
            int playerBulletLayer = LayerMask.NameToLayer(PlayerBulletLayerName);

            if (playerBulletLayer >= 0)
            {
                gameObject.layer = playerBulletLayer;
            }
        }

        startPosition = transform.position;
        maxTravelDistance = maxDistance;
        damage = damageValue;
        penetration = penetrationValue;

        rb.linearVelocity = direction.normalized * speed;
        transform.right = direction;

        TrailRenderer trail = GetComponent<TrailRenderer>();

        if (trail != null)
        {
            float startAlpha = trailColor.a <= 0f ? 1f : trailColor.a;
            Color visibleTrailColor = new Color(trailColor.r, trailColor.g, trailColor.b, startAlpha);

            trail.startColor = visibleTrailColor;
            trail.endColor = new Color(visibleTrailColor.r, visibleTrailColor.g, visibleTrailColor.b, 0f);
        }

        Destroy(gameObject, lifeTime);
    }

    public void LaunchEnemy(Vector2 direction, float speed, Color trailColor, Transform ownerTransform)
    {
        Launch(direction, speed, EnemyBulletDamage, EnemyBulletPenetration, trailColor, ownerTransform);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Hit(other.transform);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Hit(collision.transform);
    }

    private void Hit(Transform hitTransform)
    {
        if (IsOwnerTransform(hitTransform))
        {
            return;
        }

        CoverObstacle cover = hitTransform.GetComponentInParent<CoverObstacle>();

        if (cover != null && cover.BlocksBullet)
        {
            cover.TakeBulletHit(damage);
            Destroy(gameObject);
            return;
        }

        EnemyArmor armor = hitTransform.GetComponentInParent<EnemyArmor>();

        if (armor != null)
        {
            EnemyHealth protectedEnemy = armor.OwnerHealth;

            if (protectedEnemy != null)
            {
                QueueEnemyHit(hitTransform, protectedEnemy, armor);
                return;
            }
        }

        EnemyHealth enemyHealth = hitTransform.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            QueueEnemyHit(hitTransform, enemyHealth, null);
            return;
        }

        PlayerHealth playerHealth = hitTransform.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            if (!isPlayerBullet)
            {
                playerHealth.TakeDamage(damage);
            }

            ConsumePenetration();
            return;
        }

        if (IsBlockingLayer(hitTransform.gameObject.layer))
        {
            Destroy(gameObject);
        }
    }

    private void QueueEnemyHit(Transform hitTransform, EnemyHealth enemy, EnemyArmor armor)
    {
        EnemyHitCandidate candidate = new EnemyHitCandidate
        {
            HitTransform = hitTransform,
            Enemy = enemy,
            Armor = armor
        };

        if (!pendingEnemyHits.TryGetValue(enemy, out EnemyHitCandidate current)
            || candidate.Priority > current.Priority)
        {
            pendingEnemyHits[enemy] = candidate;
        }
    }

    private void ResolvePendingEnemyHits()
    {
        if (pendingEnemyHits.Count == 0)
        {
            return;
        }

        List<EnemyHitCandidate> candidates = new List<EnemyHitCandidate>(pendingEnemyHits.Values);
        pendingEnemyHits.Clear();
        candidates.Sort((a, b) =>
            ((Vector2)a.HitTransform.position - startPosition).sqrMagnitude.CompareTo(
                ((Vector2)b.HitTransform.position - startPosition).sqrMagnitude));

        foreach (EnemyHitCandidate candidate in candidates)
        {
            if (candidate.Enemy == null)
            {
                continue;
            }

            if (candidate.Armor != null)
            {
                ResolveArmorHit(candidate);
            }
            else
            {
                ResolveEnemyBodyHit(candidate);
            }
        }
    }

    private void ResolveArmorHit(EnemyHitCandidate candidate)
    {
        EnemyArmor armor = candidate.Armor;

        if (armor == null || !hitArmors.Add(armor))
        {
            return;
        }

        Debug.Log($"[Bullet Hit] EnemyArmor={candidate.HitTransform.name}, Pierce={penetration}", candidate.HitTransform.gameObject);

        bool armorBroken = isPlayerBullet && armor.TakeDamage(damage);
        bool canPierce = ConsumePenetration();

        Debug.Log(
            $"[Bullet Armor] Hit={candidate.HitTransform.name}, Armor={armor.name}, Broken={armorBroken}, PierceLeft={penetration}",
            armor.gameObject);

        if (!armorBroken || !canPierce)
        {
            armorProtectedEnemies.Add(candidate.Enemy);
        }
    }

    private void ResolveEnemyBodyHit(EnemyHitCandidate candidate)
    {
        EnemyHealth enemy = candidate.Enemy;
        Debug.Log($"[Bullet Hit] EnemyBody={candidate.HitTransform.name}, Enemy={enemy.name}, Pierce={penetration}", candidate.HitTransform.gameObject);

        if (armorProtectedEnemies.Contains(enemy))
        {
            Debug.Log($"[Bullet Body Blocked] Hit={candidate.HitTransform.name}, Enemy={enemy.name}", candidate.HitTransform.gameObject);
            return;
        }

        if (!hitEnemies.Add(enemy))
        {
            return;
        }

        if (isPlayerBullet)
        {
            Debug.Log($"[Bullet Enemy Body] Hit={candidate.HitTransform.name}, Enemy={enemy.name}, Damage={damage}", candidate.HitTransform.gameObject);
            enemy.TakeDamage(damage);
        }

        ConsumePenetration();
    }

    private bool IsOwnerTransform(Transform hitTransform)
    {
        return owner != null
            && (hitTransform == owner
                || hitTransform.IsChildOf(owner)
                || owner.IsChildOf(hitTransform));
    }

    private bool ConsumePenetration()
    {
        penetration--;

        if (penetration <= 0)
        {
            Destroy(gameObject);
            return false;
        }

        return true;
    }

    private bool IsBlockingLayer(int layer)
    {
        return (blockingLayers.value & (1 << layer)) != 0;
    }
}
