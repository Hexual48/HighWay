using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerTemporaryShooter : MonoBehaviour
{
    [SerializeField] private int ammo = 30;
    [SerializeField] private float recoilPower = 12f;

    private PlayerMovement playerMovement;
    private Camera mainCamera;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        mainCamera = Camera.main;
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed || ammo <= 0 || mainCamera == null)
        {
            return;
        }

        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 shootDirection = (mousePosition - (Vector2)transform.position).normalized;

        playerMovement.AddExternalVelocity(-shootDirection * recoilPower);
        ammo--;
    }
}
