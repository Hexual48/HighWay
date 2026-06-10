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
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float smoothTime = 0.04f;

    private Vector3 velocity;
    private float zPosition;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        zPosition = transform.position.z;
        SnapToPlayer();
    }

    private void LateUpdate()
    {
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

        if (mainCamera != null && Mouse.current != null)
        {
            Vector3 cursorPosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            cursorPosition.z = player.position.z;

            Vector3 cursorOffset = Vector3.ClampMagnitude(cursorPosition - player.position, maxOffset);
            cursorOffset.y *= verticalCursorScale;
            targetPosition = basePosition + cursorOffset * cursorWeight;
            targetPosition = ResetVerticalMovementWhenCursorBlocked(cursorPosition, basePosition, targetPosition);
        }

        targetPosition.z = zPosition;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    private Vector3 ResetVerticalMovementWhenCursorBlocked(
        Vector3 cursorPosition,
        Vector3 basePosition,
        Vector3 targetPosition)
    {
        Vector2 origin = player.position;
        Vector2 offset = (Vector2)cursorPosition - origin;
        float distance = offset.magnitude;

        if (distance <= Mathf.Epsilon)
        {
            return targetPosition;
        }

        RaycastHit2D hit = Physics2D.Raycast(origin, offset / distance, distance, groundLayer);

        if (hit.collider != null)
        {
            targetPosition.y = basePosition.y;
            velocity.y = 0f;
        }

        return targetPosition;
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
