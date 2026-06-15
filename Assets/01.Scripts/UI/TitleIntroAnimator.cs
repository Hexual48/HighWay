using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleIntroAnimator : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const float BlinkDuration = 0.75f;
    private const float BlinkHoldDuration = 0.15f;
    private const float RotationBaseChance = 0.1f;
    private const float RotationChanceIncrease = 0.05f;
    private const float RotationAngle = 10f;
    private const float PressedScaleX = 1.12f;
    private const float PressedScaleY = 0.65f;

    [Header("Title")]
    [SerializeField] private Transform highTitle;
    [SerializeField] private Transform wayTitle;
    [SerializeField] private float startOffsetX = 15f;
    [SerializeField] private float moveDuration = 0.7f;
    [SerializeField] private float wayDelay = 0.12f;
    [SerializeField] private Ease moveEase = Ease.OutBack;

    [Header("Click To Start")]
    [SerializeField] private SpriteRenderer clickToStart;
    [SerializeField] private float clickTextFadeDuration = 0.5f;
    [SerializeField] private float clickTextDelay = 0.2f;
    [SerializeField] private float pressEffectDuration = 0.55f;

    [Header("Screen Close")]
    [SerializeField] private Transform fadeUp;
    [SerializeField] private Transform fadeDown;
    [SerializeField] private float fadeStartOffset = 5f;
    [SerializeField] private float screenCloseDuration = 0.6f;
    [SerializeField] private Ease screenCloseEase = Ease.InOutCubic;

    private Tween introTween;
    private Tween idleTween;
    private Tween pressTween;
    private Tween closeTween;
    private Vector3 highTarget;
    private Vector3 wayTarget;
    private Vector3 clickScale;
    private Quaternion clickRotation;
    private float rotationChance;
    private bool canClick;

    private Transform ClickTransform => clickToStart.transform;

    private void Awake()
    {
        highTarget = highTitle.localPosition;
        wayTarget = wayTitle.localPosition;
        clickScale = ClickTransform.localScale;
        clickRotation = ClickTransform.localRotation;
        SetAlpha(0f);
    }

    private void Start()
    {
        PlayIntro();
    }

    private void Update()
    {
        if (canClick && Input.GetMouseButtonDown(0))
            PlayPressEffect();
    }

    public void PlayIntro()
    {
        KillTweens();
        canClick = false;
        rotationChance = RotationBaseChance;

        highTitle.localPosition = highTarget + Vector3.right * startOffsetX;
        wayTitle.localPosition = wayTarget + Vector3.right * startOffsetX;
        SetAlpha(0f);

        introTween = DOTween.Sequence()
            .Append(highTitle.DOLocalMove(highTarget, moveDuration).SetEase(moveEase))
            .Join(wayTitle.DOLocalMove(wayTarget, moveDuration).SetEase(moveEase).SetDelay(wayDelay))
            .AppendInterval(clickTextDelay)
            .Append(clickToStart.DOFade(1f, clickTextFadeDuration).SetEase(Ease.OutQuad))
            .OnComplete(() =>
            {
                canClick = true;
                PlayNextIdleEffect();
            });
    }

    private void PlayNextIdleEffect()
    {
        if (Random.value < rotationChance)
        {
            rotationChance = RotationBaseChance;
            Quaternion tilted = clickRotation * Quaternion.Euler(0f, 0f, -RotationAngle);

            idleTween = DOTween.Sequence()
                .Append(ClickTransform.DOLocalRotateQuaternion(tilted, 0.55f).SetEase(Ease.OutElastic))
                .Append(ClickTransform.DOLocalRotateQuaternion(clickRotation, 0.3f).SetEase(Ease.OutSine))
                .OnComplete(PlayNextIdleEffect);
            return;
        }

        rotationChance += RotationChanceIncrease;
        idleTween = DOTween.Sequence()
            .Append(clickToStart.DOFade(0f, BlinkDuration * 0.5f).SetEase(Ease.InOutSine))
            .AppendInterval(BlinkHoldDuration)
            .Append(clickToStart.DOFade(1f, BlinkDuration * 0.5f).SetEase(Ease.InOutSine))
            .OnComplete(PlayNextIdleEffect);
    }

    private void PlayPressEffect()
    {
        canClick = false;
        idleTween?.Kill();
        pressTween?.Kill();
        SetAlpha(1f);

        Vector3 pressedScale = new(
            clickScale.x * PressedScaleX,
            clickScale.y * PressedScaleY,
            clickScale.z);

        ClickTransform.localScale = clickScale;
        ClickTransform.localRotation = clickRotation;

        pressTween = DOTween.Sequence()
            .Append(ClickTransform.DOScale(pressedScale, pressEffectDuration * 0.2f).SetEase(Ease.OutQuad))
            .Append(ClickTransform.DOScale(clickScale, pressEffectDuration * 0.8f).SetEase(Ease.OutBounce))
            .OnComplete(PlayScreenClose);
    }

    private void PlayScreenClose()
    {
        Vector3 upTarget = new(fadeUp.localPosition.x, 2.5f, fadeUp.localPosition.z);
        Vector3 downTarget = new(fadeDown.localPosition.x, -2.5f, fadeDown.localPosition.z);

        fadeUp.localPosition = upTarget + Vector3.up * fadeStartOffset;
        fadeDown.localPosition = downTarget + Vector3.down * fadeStartOffset;
        fadeUp.gameObject.SetActive(true);
        fadeDown.gameObject.SetActive(true);

        closeTween = DOTween.Sequence()
            .Append(fadeUp.DOLocalMove(upTarget, screenCloseDuration).SetEase(screenCloseEase))
            .Join(fadeDown.DOLocalMove(downTarget, screenCloseDuration).SetEase(screenCloseEase))
            .OnComplete(() => SceneManager.LoadScene(MainMenuSceneName));
    }

    private void SetAlpha(float alpha)
    {
        Color color = clickToStart.color;
        color.a = alpha;
        clickToStart.color = color;
    }

    private void KillTweens()
    {
        introTween?.Kill();
        idleTween?.Kill();
        pressTween?.Kill();
        closeTween?.Kill();
    }

    private void OnDestroy()
    {
        KillTweens();
    }
}
