using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private const float FadeDelay = 0.3f;

    [SerializeField] private EnemyController controller;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private GameObject weaponHandle;
    [SerializeField] private Transform headAccessory;
    [SerializeField] private Transform mask;
    [SerializeField] private Sprite deadSprite;
    [SerializeField, Min(0f)] private float fadeTime = 0.4f;

    private Rigidbody2D rb;
    private Collider2D enemyCollider;
    private EnemyArmor[] armors;
    private Sequence deathSequence;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private Vector3 spawnScale;
    private Vector3 headPosition;
    private Sprite aliveSprite;
    private Color aliveBodyColor;
    private RigidbodyConstraints2D aliveConstraints;
    private bool colliderWasEnabled;
    private bool weaponWasActive;
    private bool isDead;

    public bool IsDead => isDead;

    private void Awake()
    {
        controller ??= GetComponent<EnemyController>();
        bodyRenderer ??= GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        armors = GetComponentsInChildren<EnemyArmor>(true);

        if (weaponHandle == null)
        {
            Transform handle = transform.Find("Handle");
            weaponHandle = handle != null ? handle.gameObject : null;
        }

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        spawnScale = transform.localScale;
        headPosition = headAccessory != null ? headAccessory.localPosition : Vector3.zero;
        aliveSprite = bodyRenderer != null ? bodyRenderer.sprite : null;
        aliveBodyColor = bodyRenderer != null ? bodyRenderer.color : Color.white;
        aliveConstraints = rb != null ? rb.constraints : RigidbodyConstraints2D.None;
        colliderWasEnabled = enemyCollider != null && enemyCollider.enabled;
        weaponWasActive = weaponHandle != null && weaponHandle.activeSelf;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        EnterDeadState();
    }

    private void EnterDeadState()
    {
        isDead = true;
        controller?.EnterDeadState();
        DisableRemainingArmor();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        if (weaponHandle != null)
        {
            weaponHandle.SetActive(false);
        }

        if (headAccessory != null)
        {
            headAccessory.localPosition += Vector3.down;
        }

        if (bodyRenderer != null)
        {
            bodyRenderer.sprite = deadSprite;
            Color color = bodyRenderer.color;
            color.a = 1f;
            bodyRenderer.color = color;
        }

        deathSequence?.Kill();
        deathSequence = DOTween.Sequence()
            .AppendInterval(FadeDelay)
            .Append(CreateFadeTween())
            .OnComplete(() => gameObject.SetActive(false))
            .SetLink(gameObject);
    }

    public void Respawn()
    {
        deathSequence?.Kill();
        isDead = false;
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        transform.localScale = spawnScale;

        if (headAccessory != null)
        {
            headAccessory.localPosition = headPosition;
        }

        if (bodyRenderer != null)
        {
            bodyRenderer.sprite = aliveSprite;
            bodyRenderer.color = aliveBodyColor;
        }

        if (weaponHandle != null)
        {
            weaponHandle.SetActive(weaponWasActive);
        }

        if (rb != null)
        {
            rb.position = spawnPosition;
            rb.rotation = spawnRotation.eulerAngles.z;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = aliveConstraints;
            rb.WakeUp();
        }

        if (enemyCollider != null)
        {
            enemyCollider.enabled = colliderWasEnabled;
        }

        foreach (EnemyArmor armor in armors)
        {
            if (armor != null)
            {
                armor.Respawn();
            }
        }

        controller?.Respawn();
    }

    private void DisableRemainingArmor()
    {
        foreach (EnemyArmor armor in GetComponentsInChildren<EnemyArmor>(true))
        {
            if (armor.gameObject != gameObject)
            {
                armor.gameObject.SetActive(false);
            }
        }
    }

    private Tween CreateFadeTween()
    {
        HashSet<SpriteRenderer> renderers = new HashSet<SpriteRenderer>();

        if (bodyRenderer != null)
        {
            renderers.Add(bodyRenderer);
        }

        AddRenderers(headAccessory, renderers);
        AddRenderers(mask, renderers);

        Sequence fadeSequence = DOTween.Sequence();
        bool hasRenderer = false;

        foreach (SpriteRenderer renderer in renderers)
        {
            Tween fadeTween = renderer.DOFade(0f, fadeTime).SetEase(Ease.InQuad);

            if (!hasRenderer)
            {
                fadeSequence.Append(fadeTween);
                hasRenderer = true;
            }
            else
            {
                fadeSequence.Join(fadeTween);
            }
        }

        if (!hasRenderer)
        {
            fadeSequence.AppendInterval(fadeTime);
        }

        return fadeSequence;
    }

    private static void AddRenderers(Transform root, HashSet<SpriteRenderer> renderers)
    {
        if (root == null)
        {
            return;
        }

        foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderers.Add(renderer);
        }
    }

    private void OnDestroy()
    {
        deathSequence?.Kill();
    }
}
