using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const float FadeTargetAlpha = 1f;
    private const int DeadSortingOrder = 10;

    [Header("Player")]
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private GameObject weaponHandle;
    [SerializeField] private Sprite deadSprite;
    [SerializeField] private Vector2 respawnPosition = new(-7.34f, -3.34f);
    [SerializeField] private Vector3 defaultScale = new(0.5f, 0.5f, 0.5f);

    [Header("Death UI")]
    [SerializeField] private GameObject deadCanvas;
    [SerializeField] private CanvasGroup fade;
    [SerializeField, Min(0f)] private float fadeTime = 0.8f;

    [Header("Death Animation")]
    [SerializeField, Min(0f)] private float squashY = 0.38f;
    [SerializeField, Min(0f)] private float waitTime = 0.35f;
    [SerializeField, Min(0f)] private float squashTime = 0.5f;
    [SerializeField, Min(0f)] private float impactHold = 0.15f;
    [SerializeField, Min(0f)] private float preShake = 0.12f;
    [SerializeField, Min(0f)] private float hitShake = 0.7f;
    [SerializeField, Min(0f)] private float hitShakeTime = 0.2f;

    [Header("Respawn Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spawnClip;
    [SerializeField, Range(0f, 1f)] private float spawnVolume = 1f;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private PlayerInput playerInput;
    private NewMovement movement;
    private PlayerAimToCursor aim;
    private ShotgunFire shotgunFire;
    private MouseLookCameraTarget cameraTarget;
    private Sprite aliveSprite;
    private int aliveSortingOrder;
    private RigidbodyConstraints2D aliveConstraints;
    private bool movementWasEnabled;
    private bool aimWasEnabled;
    private bool shotgunWasEnabled;
    private bool handleWasActive;
    private bool isDead;
    private Sequence deathSequence;

    public bool IsDead => isDead;

    private void Awake()
    {
        ResolveReferences();
        aliveSprite = bodyRenderer != null ? bodyRenderer.sprite : null;
        aliveSortingOrder = bodyRenderer != null ? bodyRenderer.sortingOrder : 0;
        aliveConstraints = rb != null ? rb.constraints : RigidbodyConstraints2D.None;
        WireButtons();
        HideDeadCanvas();
    }

    public void TakeDamage(int damage)
    {
        if (!isDead && damage > 0)
        {
            EnterDeadState();
        }
    }

    public void Retry()
    {
        deathSequence?.Kill();
        isDead = false;
        transform.position = respawnPosition;
        transform.rotation = Quaternion.identity;
        transform.localScale = defaultScale;

        if (weaponHandle != null)
        {
            weaponHandle.transform.localRotation = Quaternion.identity;
            weaponHandle.SetActive(handleWasActive);
        }

        if (bodyRenderer != null)
        {
            bodyRenderer.sprite = aliveSprite;
            bodyRenderer.sortingOrder = aliveSortingOrder;
        }

        if (rb != null)
        {
            rb.position = respawnPosition;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = aliveConstraints;
            rb.WakeUp();
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        SetControlState(true);
        cameraTarget?.SetCursorFollowEnabled(true);
        HideDeadCanvas();

        if (audioSource != null && spawnClip != null)
        {
            audioSource.PlayOneShot(spawnClip, spawnVolume * GameSettings.SfxVolume);
        }
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void EnterDeadState()
    {
        isDead = true;
        LockPlayer();
        transform.localScale = defaultScale;

        if (bodyRenderer != null)
        {
            bodyRenderer.sortingOrder = DeadSortingOrder;
        }

        cameraTarget?.SetCursorFollowEnabled(false);
        cameraTarget?.Shake(preShake, waitTime);

        deathSequence?.Kill();
        deathSequence = DOTween.Sequence()
            .AppendInterval(waitTime)
            .AppendCallback(() => cameraTarget?.Shake(preShake, squashTime))
            .Append(transform.DOScaleY(squashY, squashTime).SetEase(Ease.InQuad))
            .AppendCallback(PlayImpact)
            .AppendInterval(impactHold)
            .AppendCallback(ShowDeadCanvas)
            .SetLink(gameObject);
    }

    private void PlayImpact()
    {
        cameraTarget?.Shake(hitShake, hitShakeTime);
        transform.localScale = defaultScale;

        if (bodyRenderer != null && deadSprite != null)
        {
            bodyRenderer.sprite = deadSprite;
        }
    }

    private void LockPlayer()
    {
        movementWasEnabled = movement != null && movement.enabled;
        aimWasEnabled = aim != null && aim.enabled;
        shotgunWasEnabled = shotgunFire != null && shotgunFire.enabled;
        handleWasActive = weaponHandle != null && weaponHandle.activeSelf;
        SetControlState(false);

        transform.rotation = Quaternion.identity;

        if (weaponHandle != null)
        {
            weaponHandle.transform.localRotation = Quaternion.identity;
            weaponHandle.SetActive(false);
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.Sleep();
        }
    }

    private void SetControlState(bool enabledState)
    {
        if (movement != null)
        {
            movement.enabled = enabledState && movementWasEnabled;
        }

        if (aim != null)
        {
            aim.enabled = enabledState && aimWasEnabled;
        }

        if (shotgunFire != null)
        {
            shotgunFire.enabled = enabledState && shotgunWasEnabled;
        }

        if (playerInput != null)
        {
            if (enabledState)
            {
                playerInput.ActivateInput();
            }
            else
            {
                playerInput.DeactivateInput();
            }
        }
    }

    private void ShowDeadCanvas()
    {
        if (deadCanvas == null)
        {
            return;
        }

        deadCanvas.SetActive(true);

        if (fade != null)
        {
            fade.alpha = 0f;
            fade.DOFade(FadeTargetAlpha, fadeTime)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .SetLink(deadCanvas);
        }
    }

    private void HideDeadCanvas()
    {
        if (fade != null)
        {
            fade.DOKill();
            fade.alpha = 0f;
        }

        if (deadCanvas != null)
        {
            deadCanvas.SetActive(false);
        }
    }

    private void ResolveReferences()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        playerInput = GetComponent<PlayerInput>();
        movement = GetComponent<NewMovement>();
        aim = GetComponent<PlayerAimToCursor>();
        shotgunFire = GetComponentInChildren<ShotgunFire>(true);
        bodyRenderer ??= GetComponent<SpriteRenderer>();

        if (weaponHandle == null)
        {
            Transform handleTransform = transform.Find("Handle");
            weaponHandle = handleTransform != null ? handleTransform.gameObject : null;
        }

        Camera mainCamera = Camera.main;
        cameraTarget = mainCamera != null ? mainCamera.GetComponent<MouseLookCameraTarget>() : null;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    private void WireButtons()
    {
        if (deadCanvas == null)
        {
            return;
        }

        foreach (Button button in deadCanvas.GetComponentsInChildren<Button>(true))
        {
            if (button.name == "Retry")
            {
                button.onClick.RemoveListener(Retry);
                button.onClick.AddListener(Retry);
            }
            else if (button.name == "Quit")
            {
                button.onClick.RemoveListener(QuitToMainMenu);
                button.onClick.AddListener(QuitToMainMenu);
            }
        }
    }

    private void OnDestroy()
    {
        deathSequence?.Kill();
    }
}
