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
    private Sequence deathSequence;
    private bool isDead;

    public bool IsDead => isDead;

    private void Awake()
    {
        controller ??= GetComponent<EnemyController>();
        bodyRenderer ??= GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();

        if (weaponHandle == null)
        {
            Transform handle = transform.Find("Handle");
            weaponHandle = handle != null ? handle.gameObject : null;
        }
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
            .OnComplete(() => Destroy(gameObject))
            .SetLink(gameObject);
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
