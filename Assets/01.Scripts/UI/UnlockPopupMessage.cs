using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockPopupMessage : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float visibleSeconds = 2f;
    [SerializeField, Min(0f)] private float fadeSeconds = 0.25f;

    private Coroutine showRoutine;
    private bool isShowing;

    private void Awake()
    {
        ResolveMissingReferences();
        SetAlpha(0f);
    }

    private void Start()
    {
        if (!isShowing)
        {
            gameObject.SetActive(false);
        }
    }

    public void Show(AmmoData ammoData)
    {
        if (ammoData == null)
        {
            return;
        }

        Show(ammoData.unlockIcon, ammoData.unlockTitle, ammoData.unlockDescription);
    }

    public void Show(Sprite icon, string title, string message)
    {
        isShowing = true;
        gameObject.SetActive(true);
        ResolveMissingReferences();

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
        }

        showRoutine = StartCoroutine(ShowRoutine(icon, title, message));
    }

    private IEnumerator ShowRoutine(Sprite icon, string title, string message)
    {
        ApplyMessage(icon, title, message);

        yield return FadeTo(1f);

        yield return new WaitForSeconds(visibleSeconds);

        yield return FadeTo(0f);
        isShowing = false;
        gameObject.SetActive(false);
        showRoutine = null;
    }

    private void ApplyMessage(Sprite icon, string title, string message)
    {
        if (titleText != null)
        {
            titleText.text = title;
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
        }

        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (canvasGroup == null || fadeSeconds <= 0f)
        {
            SetAlpha(targetAlpha);
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeSeconds));
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    private void ResolveMissingReferences()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (iconImage == null)
        {
            iconImage = FindChildComponent<Image>("Icon");
        }

        if (titleText == null)
        {
            titleText = FindChildComponent<TMP_Text>("Title");
        }

        if (messageText == null)
        {
            messageText = FindChildComponent<TMP_Text>("Desc");
        }
    }

    private T FindChildComponent<T>(string childName) where T : Component
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<T>() : null;
    }

    private void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
            canvasGroup.blocksRaycasts = alpha > 0f;
            canvasGroup.interactable = alpha > 0f;
        }
    }
}
