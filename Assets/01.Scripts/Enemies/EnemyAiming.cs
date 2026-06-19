using UnityEngine;

public class EnemyAiming : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyVision2D vision;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private Transform weaponHandle;

    [Header("Facing")]
    [SerializeField] private bool defaultFacesRight = true;

    [Header("Reload")]
    [SerializeField] private float rightReloadAngle = -45f;
    [SerializeField] private float leftReloadAngle = 45f;
    [SerializeField, Min(0f)] private float lowerDuration = 0.2f;
    [SerializeField, Min(0f)] private float raiseDuration = 0.2f;
    [SerializeField] private AnimationCurve reloadEase = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(1f, 1f));

    private bool isFacingRight = true;
    private float weaponScaleY;
    private float reloadStartAngle;
    private float currentReloadAngle;
    private SpriteRenderer[] facingRenderers;
    private bool[] initialFlipX;

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

        RefreshFacingRenderers();

        if (weaponHandle != null)
        {
            weaponScaleY = Mathf.Abs(weaponHandle.localScale.y);
        }
    }

    public void RefreshFacingRenderers()
    {
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        int rendererCount = 0;

        for (int i = 0; i < childRenderers.Length; i++)
        {
            if (!IsWeaponRenderer(childRenderers[i]))
            {
                rendererCount++;
            }
        }

        facingRenderers = new SpriteRenderer[rendererCount];
        initialFlipX = new bool[rendererCount];

        int facingRendererIndex = 0;

        for (int i = 0; i < childRenderers.Length; i++)
        {
            SpriteRenderer childRenderer = childRenderers[i];

            if (IsWeaponRenderer(childRenderer))
            {
                continue;
            }

            facingRenderers[facingRendererIndex] = childRenderer;
            initialFlipX[facingRendererIndex] = childRenderer.flipX;
            facingRendererIndex++;
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

    public void BeginReload()
    {
        if (weaponHandle != null)
        {
            reloadStartAngle = weaponHandle.eulerAngles.z;
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

        weaponHandle.rotation = Quaternion.Euler(
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
        bool flipFromDefault = defaultFacesRight ? !isFacingRight : isFacingRight;

        for (int i = 0; i < facingRenderers.Length; i++)
        {
            if (facingRenderers[i] != null)
            {
                facingRenderers[i].flipX = initialFlipX[i] ^ flipFromDefault;
            }
        }
    }

    private bool IsWeaponRenderer(SpriteRenderer spriteRenderer)
    {
        return weaponHandle != null && spriteRenderer.transform.IsChildOf(weaponHandle);
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
