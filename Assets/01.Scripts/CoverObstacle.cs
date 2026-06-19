using UnityEngine;

[DisallowMultipleComponent]
public class CoverObstacle : MonoBehaviour
{
    [SerializeField] private bool blocksBullet = true;
    [SerializeField, Min(0)] private int durability;

    public bool BlocksBullet => blocksBullet;

    public void TakeBulletHit(int damage)
    {
        if (!blocksBullet || durability <= 0)
        {
            return;
        }

        durability = Mathf.Max(0, durability - Mathf.Max(0, damage));

        if (durability == 0)
        {
            Destroy(gameObject);
        }
    }
}
