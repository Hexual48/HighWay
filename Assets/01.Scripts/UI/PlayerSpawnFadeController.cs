using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public sealed class PlayerSpawnFadeController : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField, Min(0.01f)] private float fadeDuration = 1.5f;

    private PlayerInput playerInput;
    private PauseMenuController pauseMenu;
    private Coroutine fadeRoutine;
    private bool pauseMenuWasEnabled;

    public bool IsFading => fadeRoutine != null;

    private void Awake()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
        pauseMenu = FindFirstObjectByType<PauseMenuController>();

        if (fadeCanvasGroup == null)
        {
            fadeCanvasGroup = GetComponent<CanvasGroup>();
        }

        if (fadeCanvasGroup == null)
        {
            fadeCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        BeginTransition();
    }

    private void Start()
    {
        fadeRoutine = StartCoroutine(FadeOut());
    }

    public void PlayFade()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        BeginTransition();
        fadeRoutine = StartCoroutine(FadeOut());
    }

    private void BeginTransition()
    {
        Time.timeScale = 0f;
        playerInput?.DeactivateInput();

        if (pauseMenu != null)
        {
            pauseMenuWasEnabled = pauseMenu.enabled;
            pauseMenu.enabled = false;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = false;
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        Time.timeScale = 1f;
        playerInput?.ActivateInput();

        if (pauseMenu != null)
        {
            pauseMenu.enabled = pauseMenuWasEnabled;
        }

        fadeRoutine = null;
    }

    private void OnDestroy()
    {
        if (fadeRoutine != null)
        {
            Time.timeScale = 1f;
        }
    }
}
