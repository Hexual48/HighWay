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

    private NewMovement movement;
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

        WarnIfAnimatorMissing();
        UpdateAnimatorAmmo();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            TryReload();
        }
    }

    public void OnAttack(InputValue value)
    {
        if (!isActiveAndEnabled || !value.isPressed)
        {
            return;
        }

        fireQueued = true;
    }

    public void OnReload(InputValue value)
    {
        if (!isActiveAndEnabled || !value.isPressed)
        {
            return;
        }

        TryReload();
    }

    private void LateUpdate()
    {
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
            return;
        }

        Fire(firePoint.right, ammoData);
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
        if (loadedAmmo < maxLoadedAmmo)
        {
            BeginReload();
        }
    }

    private void BeginReload()
    {
        if (isReloading || loadedAmmo >= maxLoadedAmmo)
        {
            return;
        }

        isReloading = true;
        reloadTimer = reloadTime;

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
        if (playerAmmo != null && playerAmmo.CurrentAmmo != null)
        {
            return playerAmmo.CurrentAmmo;
        }

        return currentAmmo;
    }
}
