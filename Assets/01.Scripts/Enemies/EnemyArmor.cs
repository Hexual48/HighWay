using UnityEngine;

public class EnemyArmor : MonoBehaviour
{
    public enum ArmorType
    {
        Mask = 1,
        Helmet = 2
    }

    [SerializeField, Min(1)] private int health = 100;
    [SerializeField] private ArmorType armorType = ArmorType.Mask;
    [SerializeField] private AudioClip shieldBrokenClip;
    [SerializeField, Range(0f, 1f)] private float shieldBrokenVolume = 1f;

    private EnemyHealth ownerHealth;
    private SpriteRenderer[] renderers;
    private Color[] initialColors;
    private int initialHealth;
    private bool broken;

    public EnemyHealth OwnerHealth => ownerHealth;
    public int HitPriority => (int)armorType;

    private void Awake()
    {
        ownerHealth = GetComponentInParent<EnemyHealth>();
        initialHealth = health;
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        initialColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            initialColors[i] = renderers[i].color;
        }
    }

    public bool TakeDamage(int damage)
    {
        if (broken || damage <= 0)
        {
            return false;
        }

        health -= damage;

        Debug.Log($"[Armor Damage] Object={name}, Damage={damage}, HealthLeft={Mathf.Max(health, 0)}", gameObject);

        if (health > 0)
        {
            return false;
        }

        broken = true;
        Debug.Log($"[Armor Destroyed] Object={name}, Enemy={(ownerHealth != null ? ownerHealth.name : "None")}", gameObject);
        PlayShieldBrokenSound();
        gameObject.SetActive(false);
        return true;
    }

    public void Respawn()
    {
        health = initialHealth;
        broken = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].color = initialColors[i];
            }
        }

        gameObject.SetActive(true);
    }

    private void PlayShieldBrokenSound()
    {
        if (shieldBrokenClip == null)
        {
            return;
        }

        AudioSource audioSource = ownerHealth != null
            ? ownerHealth.GetComponent<AudioSource>()
            : GetComponentInParent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.PlayOneShot(shieldBrokenClip, shieldBrokenVolume * GameSettings.SfxVolume);
        }
    }
}
