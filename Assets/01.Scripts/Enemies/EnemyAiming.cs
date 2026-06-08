using UnityEngine;

public class EnemyAiming : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyVision2D vision;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private Transform weaponHandle;

    [Header("Facing")]
    [SerializeField] private bool defaultFacesRight = true;

    private bool isFacingRight = true;
    private float weaponScaleY;

    private void Awake()
    {
        if (vision == null)
        {
            vision = GetComponent<EnemyVision2D>();
        }

        if (bodyRenderer == null)
        {
            bodyRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (weaponHandle != null)
        {
            weaponScaleY = Mathf.Abs(weaponHandle.localScale.y);
        }
    }

    public void Aim(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Vector2 aimDirection = direction.normalized;
        vision?.SetFacingDirection(aimDirection);
        FaceDirection(aimDirection);

        if (weaponHandle != null)
        {
            AimWeapon(aimDirection);
        }
    }

    public void Face(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Vector2 faceDirection = direction.normalized;
        vision?.SetFacingDirection(faceDirection);
        FaceDirection(faceDirection);

        if (weaponHandle != null)
        {
            AimWeapon(faceDirection);
        }
    }

    public void AimForward()
    {
        Vector2 forward = isFacingRight ? Vector2.right : Vector2.left;
        vision?.SetFacingDirection(forward);

        if (weaponHandle != null)
        {
            AimWeapon(forward);
        }
    }

    private void FaceDirection(Vector2 direction)
    {
        if (bodyRenderer == null || Mathf.Approximately(direction.x, 0f))
        {
            return;
        }

        isFacingRight = direction.x > 0f;
        bodyRenderer.flipX = defaultFacesRight ? !isFacingRight : isFacingRight;
    }

    private void AimWeapon(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        weaponHandle.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector3 scale = weaponHandle.localScale;
        scale.y = isFacingRight ? weaponScaleY : -weaponScaleY;
        weaponHandle.localScale = scale;
    }
}
