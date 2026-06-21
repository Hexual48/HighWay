using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimToCursor : MonoBehaviour
{
    private Camera mainCamera;
    private float scaleY;

    private void Awake()
    {
        mainCamera = Camera.main;
        scaleY = Mathf.Abs(transform.localScale.y);
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
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
        scale.y = angle > 90f || angle < -90f ? -scaleY : scaleY;
        transform.localScale = scale;
    }
}
