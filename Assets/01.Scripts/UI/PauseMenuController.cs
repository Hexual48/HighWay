using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const float NormalTimeScale = 1f;

    [Header("References")]
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private RectTransform slideTarget;
    [SerializeField] private GameObject shellsUI;
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private PlayerInput playerInput;

    [Header("Slow Motion")]
    [SerializeField, Range(0.05f, 1f)] private float slowTimeScale = 0.5f;

    [Header("Animation")]
    [SerializeField] private float hiddenOffsetX = -1600f;
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;

    private Tween slideTween;
    private Vector2 shownPosition;
    private bool isPaused;
    private bool isSlowMotion;

    private void Awake()
    {
        if (playerInput == null)
        {
            playerInput = FindFirstObjectByType<PlayerInput>();
        }

        shownPosition = slideTarget.anchoredPosition;
        shellsUI.SetActive(false);
        settingsUI.SetActive(false);
        pauseCanvas.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();

        if (!isPaused)
            SetSlowMotion(Mouse.current != null && Mouse.current.rightButton.isPressed);
    }

    private void SetSlowMotion(bool enabled)
    {
        if (isSlowMotion == enabled)
            return;

        isSlowMotion = enabled;
        ApplyGameplayTimeScale();
    }

    public void TogglePause()
    {
        if (isPaused)
            ClosePauseMenu();
        else
            OpenPauseMenu();
    }

    public void OpenPauseMenu()
    {
        if (isPaused)
            return;

        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        playerInput?.DeactivateInput();

        Vector2 hiddenPosition = shownPosition;
        hiddenPosition.x += hiddenOffsetX;

        slideTarget.anchoredPosition = hiddenPosition;
        pauseCanvas.SetActive(true);

        slideTween?.Kill();
        slideTween = slideTarget
            .DOAnchorPos(shownPosition, slideDuration)
            .SetEase(slideEase)
            .SetUpdate(true);
    }

    public void ClosePauseMenu()
    {
        if (!isPaused)
            return;

        isPaused = false;
        shellsUI.SetActive(false);
        settingsUI.SetActive(false);

        Vector2 hiddenPosition = shownPosition;
        hiddenPosition.x += hiddenOffsetX;

        slideTween?.Kill();
        slideTween = slideTarget
            .DOAnchorPos(hiddenPosition, slideDuration)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                pauseCanvas.SetActive(false);
                isSlowMotion = Mouse.current != null && Mouse.current.rightButton.isPressed;
                ApplyGameplayTimeScale();
                AudioListener.pause = false;
                playerInput?.ActivateInput();
            });
    }

    private void ApplyGameplayTimeScale()
    {
        Time.timeScale = isSlowMotion ? slowTimeScale : NormalTimeScale;
    }

    public void QuitGame()
    {
        Time.timeScale = NormalTimeScale;
        AudioListener.pause = false;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    public void ShowShellsUI()
    {
        settingsUI.SetActive(false);
        shellsUI.SetActive(true);
    }

    public void ShowSettingsUI()
    {
        shellsUI.SetActive(false);
        settingsUI.SetActive(true);
    }

    private void OnDestroy()
    {
        slideTween?.Kill();

        if (isSlowMotion)
            Time.timeScale = NormalTimeScale;

        if (isPaused)
        {
            Time.timeScale = NormalTimeScale;
            AudioListener.pause = false;
            playerInput?.ActivateInput();
        }
    }
}
