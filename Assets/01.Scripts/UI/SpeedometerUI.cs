using TMPro;
using UnityEngine;

public class SpeedometerUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody2D targetRigidbody;
    [SerializeField] private bool horizontalOnly;

    [Header("Display")]
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private string speedFormat = "Speed: {0:0.0} u/s";
    [SerializeField, Min(0.02f)] private float refreshInterval = 0.05f;
    [SerializeField, Min(0f)] private float displayMultiplier = 1f;
    [SerializeField, Min(0f)] private float smoothing = 12f;

    private float currentSpeed;
    private float displayedSpeed;
    private float refreshTimer;

    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        if (target == null)
        {
            target = transform;
        }

        if (targetRigidbody == null)
        {
            targetRigidbody = target.GetComponent<Rigidbody2D>();
        }
    }

    private void Start()
    {
        if (speedText == null)
        {
            speedText = CreateDefaultText();
        }

        UpdateText();
    }

    private void FixedUpdate()
    {
        if (targetRigidbody == null)
        {
            return;
        }

        Vector2 velocity = targetRigidbody.linearVelocity;

        if (horizontalOnly)
        {
            velocity.y = 0f;
        }

        currentSpeed = velocity.magnitude * displayMultiplier;
    }

    private void Update()
    {
        if (smoothing > 0f)
        {
            displayedSpeed = Mathf.Lerp(displayedSpeed, currentSpeed, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
        }
        else
        {
            displayedSpeed = currentSpeed;
        }

        refreshTimer -= Time.deltaTime;

        if (refreshTimer <= 0f)
        {
            UpdateText();
            refreshTimer = refreshInterval;
        }
    }

    private TMP_Text CreateDefaultText()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();

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
        text.textWrappingMode = TextWrappingModes.NoWrap;
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

        speedText.text = string.Format(speedFormat, displayedSpeed);
    }
}
