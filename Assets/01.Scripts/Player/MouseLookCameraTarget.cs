using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLookCameraTarget : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cursorWeight = 0.35f;
    [SerializeField, Range(0f, 1f)] private float verticalCursorScale = 0.5f;
    [SerializeField] private float maxOffset = 5f;
    [SerializeField] private float verticalOffset = 6.5f;
    [SerializeField] private float smoothTime = 0.04f;

    [Header("Screen Shake")]
    [SerializeField, Min(0f)] private float shotShakeStrength = 0.25f;
    [SerializeField, Min(0f)] private float shotShakeDuration = 0.12f;
    [SerializeField, Min(0f)] private float shakeFrequency = 35f;

    private Vector3 velocity;
    private Vector3 followPosition;
    private float zPosition;
    private float shakeTimeRemaining;
    private float activeShakeDuration;
    private float activeShakeStrength;
    private float shakeSeed;
    private bool cursorFollowEnabled = true;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        zPosition = transform.position.z;
        SnapToPlayer();
        followPosition = transform.position;
        shakeSeed = Random.value * 1000f;
    }

    private void LateUpdate()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (player == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Vector3 basePosition = player.position + Vector3.up * verticalOffset;
        Vector3 targetPosition = basePosition;

        if (cursorFollowEnabled && mainCamera != null && Mouse.current != null)
        {
            Vector3 cursorPosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            cursorPosition.z = player.position.z;

            Vector3 cursorOffset = Vector3.ClampMagnitude(cursorPosition - player.position, maxOffset);
            cursorOffset.y *= verticalCursorScale;
            targetPosition = basePosition + cursorOffset * cursorWeight;
        }

        targetPosition.z = zPosition;
        followPosition = Vector3.SmoothDamp(followPosition, targetPosition, ref velocity, smoothTime);
        transform.position = followPosition + CalculateShakeOffset();
    }

    public void ShakeFromShot()
    {
        Shake(shotShakeStrength, shotShakeDuration);
    }

    public void Shake(float strength, float duration)
    {
        if (strength <= 0f || duration <= 0f)
        {
            return;
        }

        activeShakeStrength = Mathf.Max(activeShakeStrength, strength);
        activeShakeDuration = duration;
        shakeTimeRemaining = duration;
        shakeSeed = Random.value * 1000f;
    }

    public void SetCursorFollowEnabled(bool enabledState)
    {
        cursorFollowEnabled = enabledState;
    }

    private Vector3 CalculateShakeOffset()
    {
        if (shakeTimeRemaining <= 0f)
        {
            return Vector3.zero;
        }

        shakeTimeRemaining = Mathf.Max(0f, shakeTimeRemaining - Time.deltaTime);
        float envelope = activeShakeDuration > 0f ? shakeTimeRemaining / activeShakeDuration : 0f;
        float elapsed = activeShakeDuration - shakeTimeRemaining;
        float sample = elapsed * shakeFrequency;
        float x = Mathf.PerlinNoise(shakeSeed, sample) * 2f - 1f;
        float y = Mathf.PerlinNoise(shakeSeed + 1f, sample) * 2f - 1f;

        if (shakeTimeRemaining <= 0f)
        {
            activeShakeStrength = 0f;
        }

        return new Vector3(x, y, 0f) * (activeShakeStrength * envelope);
    }

    private void SnapToPlayer()
    {
        if (player == null)
        {
            return;
        }

        transform.position = new Vector3(
            player.position.x,
            player.position.y + verticalOffset,
            zPosition);
    }
}
