using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyController))]
public class EnemyLaser : MonoBehaviour
{
    [Header("Aim")]
    [SerializeField, Min(0f)] private float laserDuration = 0.2f;

    [Header("Blink")]
    [SerializeField, Min(0.02f)] private float blinkInterval = 0.08f;

    private EnemyController controller;
    private EnemyFire enemyFire;
    private LineRenderer aimLine;
    private Material runtimeMaterial;

    private void Awake()
    {
        controller = GetComponent<EnemyController>();
        enemyFire = GetComponent<EnemyFire>();
        aimLine = GetComponentInChildren<LineRenderer>(true);

        if (controller == null || aimLine == null)
        {
            enabled = false;
            return;
        }

        aimLine.useWorldSpace = true;
        aimLine.positionCount = 2;

        if (aimLine.sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader != null)
            {
                runtimeMaterial = new Material(shader);
                aimLine.material = runtimeMaterial;
            }
        }

        aimLine.enabled = false;
    }

    private void Update()
    {
        Transform target = controller != null ? controller.CurrentTarget : null;

        if (controller == null || !controller.IsAiming || target == null || aimLine == null)
        {
            if (aimLine != null)
            {
                aimLine.enabled = false;
            }

            return;
        }

        aimLine.SetPosition(0, enemyFire != null ? enemyFire.FireOrigin : (Vector2)transform.position);
        aimLine.SetPosition(1, target.position);

        float blinkDuration = Mathf.Max(0f, controller.AimDuration - laserDuration);
        bool isBlinking = controller.AimTimeRemaining <= blinkDuration;
        float safeBlinkInterval = Mathf.Max(0.02f, blinkInterval);
        aimLine.enabled = !isBlinking
            || Mathf.FloorToInt(controller.AimTimeRemaining / safeBlinkInterval) % 2 == 0;
    }

    private void OnDisable()
    {
        if (aimLine != null)
        {
            aimLine.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
