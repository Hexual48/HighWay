using UnityEngine;

public class EnemyAiming : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyVision2D vision;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private Transform weaponHandle;

    [Header("Facing")]
    [SerializeField] private bool defaultFacesRight = true;
    [SerializeField] private bool initialFacesRight = false;

    [Header("Reload")]
    [SerializeField] private float rightReloadAngle = -45f;
    [SerializeField] private float leftReloadAngle = 45f;
    [SerializeField, Min(0f)] private float lowerDuration = 0.2f;
    [SerializeField, Min(0f)] private float raiseDuration = 0.2f;
    [SerializeField] private AnimationCurve reloadEase = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(1f, 1f));

    private bool isFacingRight = true;
    private float facingScaleX;
    private float reloadStartAngle;
    private float currentReloadAngle;

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

        isFacingRight = initialFacesRight;
        RefreshFacingRenderers();

        Face(initialFacesRight ? Vector2.right : Vector2.left);
    }

    public void RefreshFacingRenderers()
    {
        facingScaleX = Mathf.Abs(transform.localScale.x);
        ApplyFacingScale();
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

    public void BeginReload()
    {
        if (weaponHandle != null)
        {
            reloadStartAngle = weaponHandle.localEulerAngles.z;
            currentReloadAngle = isFacingRight ? rightReloadAngle : leftReloadAngle;
        }
    }

    public float GetReloadDuration(float waitDuration)
    {
        return lowerDuration + Mathf.Max(0f, waitDuration) + raiseDuration;
    }

    public void UpdateReload(float elapsedTime, float waitDuration)
    {
        if (weaponHandle == null)
        {
            return;
        }

        float motion;
        float safeWaitDuration = Mathf.Max(0f, waitDuration);

        if (elapsedTime < lowerDuration)
        {
            float progress = lowerDuration > 0f ? elapsedTime / lowerDuration : 1f;
            motion = reloadEase.Evaluate(Mathf.Clamp01(progress));
        }
        else if (elapsedTime < lowerDuration + safeWaitDuration)
        {
            motion = 1f;
        }
        else
        {
            float raiseElapsed = elapsedTime - lowerDuration - safeWaitDuration;
            float progress = raiseDuration > 0f ? raiseElapsed / raiseDuration : 1f;
            motion = 1f - reloadEase.Evaluate(Mathf.Clamp01(progress));
        }

        weaponHandle.localRotation = Quaternion.Euler(
            0f,
            0f,
            reloadStartAngle + currentReloadAngle * motion);
    }

    private void FaceDirection(Vector2 direction)
    {
        if (Mathf.Approximately(direction.x, 0f))
        {
            return;
        }

        isFacingRight = direction.x > 0f;
        ApplyFacingScale();
    }

    private void ApplyFacingScale()
    {
        bool flipFromDefault = defaultFacesRight ? !isFacingRight : isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x = flipFromDefault ? -facingScaleX : facingScaleX;
        transform.localScale = scale;
    }

    private void AimWeapon(Vector2 direction)
    {
        Transform weaponParent = weaponHandle.parent;
        Vector2 localDirection = weaponParent != null
            ? weaponParent.InverseTransformVector(direction).normalized
            : direction;
        float angle = Mathf.Atan2(localDirection.y, localDirection.x) * Mathf.Rad2Deg;
        weaponHandle.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
