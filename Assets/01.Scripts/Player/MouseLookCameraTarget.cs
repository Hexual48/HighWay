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
        }

        targetPosition.z = zPosition;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
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
