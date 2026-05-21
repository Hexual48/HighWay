using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTemporaryShooter : MonoBehaviour
{
    [SerializeField] private int ammo = 30;
    [SerializeField] private float recoil = 12f;

    private PlayerMovement movement;
    private Camera cam;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        cam = Camera.main;
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed || ammo <= 0)
        {
            return;
        }

        Vector2 mouse = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 dir = (mouse - (Vector2)transform.position).normalized;

        movement.AddRecoil(-dir * recoil);
        ammo--;
    }
}
