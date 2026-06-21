using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint")]
    [SerializeField, Min(0)] private int order;
    [SerializeField] private Vector2 respawnOffset = new(-4f, 0f);

    [Header("Activation Feedback")]
    [SerializeField] private SpriteRenderer indicatorRenderer;
    [SerializeField] private Color activatedColor = new(0.25f, 1f, 0.45f, 1f);
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activationClip;
    [SerializeField, Range(0f, 1f)] private float activationVolume = 1f;

    private bool activated;

    public Vector2 RespawnPosition => (Vector2)transform.position + respawnOffset;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        indicatorRenderer ??= GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null || !playerHealth.ActivateCheckpoint(order, RespawnPosition))
        {
            return;
        }

        activated = true;

        if (indicatorRenderer != null)
        {
            indicatorRenderer.color = activatedColor;
        }

        if (activationClip != null)
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            audioSource?.PlayOneShot(activationClip, activationVolume * GameSettings.SfxVolume);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = activatedColor;
        Gizmos.DrawWireSphere(RespawnPosition, 0.5f);
        Gizmos.DrawLine(transform.position, RespawnPosition);
    }
}
