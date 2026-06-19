using TMPro;
using UnityEngine;

public class SpeedometerUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private bool horizontalOnly;

    [Header("Display")]
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private string speedFormat = "Speed: {0:0.0} u/s";
    [SerializeField, Min(0.02f)] private float refreshInterval = 0.05f;
    [SerializeField, Min(0f)] private float displayMultiplier = 1f;

    private Vector3 previousPosition;
    private float currentSpeed;
    private float refreshTimer;

    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        if (target == null)
        {
            target = transform;
        }
    }

    private void Start()
    {
        previousPosition = target != null ? target.position : transform.position;

        if (speedText == null)
        {
            speedText = CreateDefaultText();
        }

        UpdateText();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 currentPosition = target.position;
        Vector3 movement = currentPosition - previousPosition;

        if (horizontalOnly)
        {
            movement.y = 0f;
        }

        currentSpeed = Time.deltaTime > 0f
            ? movement.magnitude / Time.deltaTime * displayMultiplier
            : 0f;

        previousPosition = currentPosition;
        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            UpdateText();
            refreshTimer = refreshInterval;
        }
    }

    private TMP_Text CreateDefaultText()
    {
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
        {
            return null;
        }

        GameObject textObject = new GameObject("SpeedText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(24f, -24f);
        rectTransform.sizeDelta = new Vector2(280f, 48f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.raycastTarget = false;
        text.enableWordWrapping = false;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.fontSize = 28f;
        text.color = Color.white;

        return text;
    }

    private void UpdateText()
    {
        if (speedText == null)
        {
            return;
        }

        speedText.text = string.Format(speedFormat, currentSpeed);
    }
}
