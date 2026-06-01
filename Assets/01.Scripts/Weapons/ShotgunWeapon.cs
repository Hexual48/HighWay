using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class ShotgunWeapon : MonoBehaviour
{
    [Header("Ammo")]
    [SerializeField] private AmmoData currentAmmo;

    [Header("Projectile")]
    [SerializeField] private PelletProjectile pelletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float pelletSpeed = 30f;

    private PlayerMovement movement;
    private Camera mainCamera;

    public AmmoData CurrentAmmo => currentAmmo;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        mainCamera = Camera.main;

        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        FireAtMouse();
    }

    public void SetAmmo(AmmoData ammoData)
    {
        currentAmmo = ammoData;
    }

    private void FireAtMouse()
    {
        if (currentAmmo == null || pelletPrefab == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null || Mouse.current == null)
        {
            return;
        }

        Vector2 origin = firePoint.position;
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 aimDirection = (mousePosition - origin).normalized;

        Fire(aimDirection);
    }

    private void Fire(Vector2 aimDirection)
    {
        int pelletCount = Mathf.Max(1, currentAmmo.pelletCount);
        float halfSpread = currentAmmo.spreadAngle * 0.5f;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = Random.Range(-halfSpread, halfSpread);
            Vector2 pelletDirection = Quaternion.Euler(0f, 0f, angle) * aimDirection;

            PelletProjectile pellet = Instantiate(pelletPrefab, firePoint.position, Quaternion.identity);
            pellet.Launch(
                pelletDirection,
                pelletSpeed,
                currentAmmo.damagePerPellet,
                currentAmmo.penetration,
                currentAmmo.color,
                transform);
        }

        movement.AddRecoil(-aimDirection * currentAmmo.recoilForce);
    }
}
