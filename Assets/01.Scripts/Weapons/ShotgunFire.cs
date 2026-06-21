using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ShotgunFire : MonoBehaviour
{
    private static readonly int ReloadHash = Animator.StringToHash("Reload");
    private static readonly int AmmoHash = Animator.StringToHash("Ammo");

    [Header("Ammo")]
    [SerializeField] private AmmoData currentAmmo;
    [SerializeField, Min(1)] private int maxLoadedAmmo = 2;
    [SerializeField, Min(0f)] private float reloadTime = 1f;

    [Header("Projectile")]
    [FormerlySerializedAs("pelletPrefab")]
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private Transform firePoint;
    [FormerlySerializedAs("pelletSpeed")]
    [SerializeField] private float bulletSpeed = 30f;
    [SerializeField] private float bulletMaxDistance = 10f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireClip;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField, Range(0f, 1f)] private float fireVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float reloadVolume = 1f;

    private NewMovement movement;
    private MouseLookCameraTarget cameraTarget;
    private PlayerAmmo playerAmmo;
    private int loadedAmmo;
    private float reloadTimer;
    private bool fireQueued;
    private bool isReloading;
    private bool loggedMissingAnimator;

    public AmmoData CurrentAmmo => GetCurrentAmmo();
    public int LoadedAmmo => loadedAmmo;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        movement = GetComponentInParent<NewMovement>();
        playerAmmo = GetComponentInParent<PlayerAmmo>();
        Camera mainCamera = Camera.main;
        cameraTarget = mainCamera != null ? mainCamera.GetComponent<MouseLookCameraTarget>() : null;
        loadedAmmo = maxLoadedAmmo;

        if (firePoint == null)
        {
            firePoint = transform;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        if (animator == null)
        {
            animator = GetComponentInParent<Animator>(true);
        }

        EnsureAudioSource();
        WarnIfAnimatorMissing();
        UpdateAnimatorAmmo();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            TryReload();
        }
    }

    public void OnAttack(InputValue value)
    {
        if (Time.timeScale <= 0f || !isActiveAndEnabled || !value.isPressed)
        {
            return;
        }

        fireQueued = true;
    }

    public void OnReload(InputValue value)
    {
        if (Time.timeScale <= 0f || !isActiveAndEnabled || !value.isPressed)
        {
            return;
        }

        TryReload();
    }

    private void LateUpdate()
    {
        if (Time.timeScale <= 0f)
        {
            fireQueued = false;
            return;
        }

        UpdateReload();

        if (!fireQueued)
        {
            return;
        }

        fireQueued = false;
        FireFromWeapon();
    }

    public void SetAmmo(AmmoData ammoData)
    {
        currentAmmo = ammoData;
    }

    private void FireFromWeapon()
    {
        AmmoData ammoData = GetCurrentAmmo();

        if (ammoData == null || bulletPrefab == null || isReloading)
        {
            return;
        }

        if (loadedAmmo <= 0)
        {
            if (GameSettings.ClickReloadEnabled)
            {
                TryReload();
            }

            return;
        }

        Fire(firePoint.right, ammoData);
        cameraTarget?.ShakeFromShot();
        PlayOneShot(fireClip, fireVolume);
        loadedAmmo--;
        UpdateAnimatorAmmo();
    }

    private void Fire(Vector2 aimDirection, AmmoData ammoData)
    {
        int pelletCount = Mathf.Max(1, ammoData.pelletCount);
        float halfSpread = ammoData.spreadAngle * 0.5f;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = Random.Range(-halfSpread, halfSpread);
            Vector2 pelletDirection = Quaternion.Euler(0f, 0f, angle) * aimDirection;

            BulletProjectile bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.Launch(
                pelletDirection,
                bulletSpeed,
                ammoData.damagePerPellet,
                ammoData.penetration,
                ammoData.color,
                transform,
                bulletMaxDistance);
        }

        if (movement != null)
        {
            movement.AddRecoil(-aimDirection * ammoData.recoilForce);
        }
    }

    private void TryReload()
    {
        if (GetCurrentAmmo() != null && loadedAmmo < maxLoadedAmmo)
        {
            BeginReload();
        }
    }

    private void BeginReload()
    {
        if (GetCurrentAmmo() == null || isReloading || loadedAmmo >= maxLoadedAmmo)
        {
            return;
        }

        isReloading = true;
        reloadTimer = reloadTime;
        PlayOneShot(reloadClip, reloadVolume);

        if (animator != null)
        {
            animator.SetTrigger(ReloadHash);
            return;
        }

        WarnIfAnimatorMissing();
    }

    private void UpdateReload()
    {
        if (!isReloading)
        {
            return;
        }

        reloadTimer -= Time.deltaTime;

        if (reloadTimer > 0f)
        {
            return;
        }

        loadedAmmo = maxLoadedAmmo;
        isReloading = false;
        UpdateAnimatorAmmo();
    }

    private void UpdateAnimatorAmmo()
    {
        if (animator != null)
        {
            animator.SetInteger(AmmoHash, loadedAmmo);
            return;
        }

        WarnIfAnimatorMissing();
    }

    private void WarnIfAnimatorMissing()
    {
        if (animator != null || loggedMissingAnimator)
        {
            return;
        }

        loggedMissingAnimator = true;
        Debug.LogWarning("ShotgunFire could not find an Animator. Assign the Shotgun Animator in the animator field.", this);
    }

    private AmmoData GetCurrentAmmo()
    {
        if (playerAmmo != null)
        {
            return playerAmmo.CurrentAmmo;
        }

        return currentAmmo;
    }

    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, volume * GameSettings.SfxVolume);
    }

    private void EnsureAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }
}
