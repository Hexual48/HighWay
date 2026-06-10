using UnityEngine;

[RequireComponent(typeof(EnemyController))]
[RequireComponent(typeof(EnemyFire))]
public class EnemyBlink : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController controller;
    [SerializeField] private EnemyFire enemyFire;
    [SerializeField] private LineRenderer aimLine;

    [Header("Aim Line")]
    [SerializeField, Min(0.001f)] private float lineWidth = 0.035f;
    [SerializeField] private Color lineColor = Color.red;
    [SerializeField] private int sortingOrder = 100;

    [Header("Blink")]
    [SerializeField, Min(0f)] private float blinkDuration = 0.35f;
    [SerializeField, Min(0.02f)] private float blinkInterval = 0.08f;

    private Material runtimeMaterial;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<EnemyController>();
        }

        if (enemyFire == null)
        {
            enemyFire = GetComponent<EnemyFire>();
        }

        if (aimLine == null)
        {
            aimLine = CreateAimLine();
        }

        ConfigureAimLine();
        SetLineVisible(false);
    }

    private void Update()
    {
        Transform target = controller != null ? controller.CurrentTarget : null;

        if (controller == null || !controller.IsAiming || target == null || aimLine == null)
        {
            SetLineVisible(false);
            return;
        }

        aimLine.SetPosition(0, enemyFire != null ? enemyFire.FireOrigin : (Vector2)transform.position);
        aimLine.SetPosition(1, target.position);

        bool isBlinking = controller.AimTimeRemaining <= blinkDuration;
        bool isVisible = !isBlinking
            || Mathf.FloorToInt(controller.AimTimeRemaining / blinkInterval) % 2 == 0;
        SetLineVisible(isVisible);
    }

    private LineRenderer CreateAimLine()
    {
        GameObject lineObject = new GameObject("AimLine");
        lineObject.transform.SetParent(transform, false);
        return lineObject.AddComponent<LineRenderer>();
    }

    private void ConfigureAimLine()
    {
        if (aimLine == null)
        {
            return;
        }

        aimLine.useWorldSpace = true;
        aimLine.positionCount = 2;
        aimLine.startWidth = lineWidth;
        aimLine.endWidth = lineWidth;
        aimLine.startColor = lineColor;
        aimLine.endColor = lineColor;
        aimLine.sortingOrder = sortingOrder;

        if (aimLine.sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader != null)
            {
                runtimeMaterial = new Material(shader);
                aimLine.material = runtimeMaterial;
            }
        }
    }

    private void SetLineVisible(bool visible)
    {
        if (aimLine != null)
        {
            aimLine.enabled = visible;
        }
    }

    private void OnDisable()
    {
        SetLineVisible(false);
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
