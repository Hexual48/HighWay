using UnityEngine;

public class EnemyHoldShoot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyController controller;
    [SerializeField] private EnemyVision2D vision;

    [Header("Hold Shoot")]
    [SerializeField] private float holdDuration = 1.5f;

    private Transform heldTarget;
    private float holdTimer;

    public Transform HeldTarget =>
        controller != null && controller.IsFiring && holdTimer > 0f
            ? heldTarget
            : null;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<EnemyController>();
        }

        if (vision == null)
        {
            vision = GetComponent<EnemyVision2D>();
        }
    }

    private void Update()
    {
        if (controller == null || !controller.IsFiring)
        {
            return;
        }

        if (vision != null && vision.currentTarget != null)
        {
            heldTarget = vision.currentTarget;
            holdTimer = holdDuration;
            return;
        }

        holdTimer = Mathf.Max(0f, holdTimer - Time.deltaTime);
    }

    public void BeginHold(Transform target)
    {
        heldTarget = target;
        holdTimer = target != null ? holdDuration : 0f;
    }

    public void EndHold()
    {
        heldTarget = null;
        holdTimer = 0f;
    }
}
