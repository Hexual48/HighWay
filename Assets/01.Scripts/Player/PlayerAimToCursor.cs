using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimToCursor : MonoBehaviour
{
    [SerializeField] private float normalScaleY = 0.5f;
    [SerializeField] private float flippedScaleY = -0.5f;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null || Mouse.current == null)
        {
            return;
        }

        Vector2 cursorPosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 currentPosition = transform.position;
        Vector2 direction = cursorPosition - currentPosition;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector3 scale = transform.localScale;
        scale.y = angle > 90f || angle < -90f ? flippedScaleY : normalScaleY;
        transform.localScale = scale;
    }
}
