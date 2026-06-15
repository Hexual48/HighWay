using UnityEngine;

public class ailenMoving : MonoBehaviour
{
    [Header("Y Axis Rotation")]
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Floating Movement")]
    [SerializeField] private float floatingHeight = 0.5f;
    [SerializeField] private float floatingSpeed = 2f;

    private Vector3 startLocalPosition;
    private float elapsedTime;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);

        float yOffset = Mathf.Sin(elapsedTime * floatingSpeed) * floatingHeight;
        transform.localPosition = startLocalPosition + Vector3.up * yOffset;
    }
}
