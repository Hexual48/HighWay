using DG.Tweening;
using UnityEngine;

public class MainMenuOpenAnimator : MonoBehaviour
{
    private static bool hasPlayedThisSession;

    [SerializeField] private Transform fadeRoot;
    [SerializeField] private Transform fadeUp;
    [SerializeField] private Transform fadeDown;
    [SerializeField] private SpriteRenderer highTitle;
    [SerializeField] private SpriteRenderer wayTitle;
    [SerializeField] private float spinDelay = 1f;
    [SerializeField] private float spinDuration = 0.8f;
    [SerializeField] private Ease spinEase = Ease.InOutCubic;
    [SerializeField] private float titleFadeDuration = 0.6f;
    [SerializeField] private float openOffset = 5f;
    [SerializeField] private float openDuration = 0.6f;
    [SerializeField] private Ease openEase = Ease.InOutCubic;

    private Tween openTween;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionState()
    {
        hasPlayedThisSession = false;
    }

    private void Awake()
    {
        if (hasPlayedThisSession)
        {
            SetAlpha(highTitle, 1f);
            SetAlpha(wayTitle, 1f);
            fadeRoot.gameObject.SetActive(false);
            return;
        }

        fadeRoot.gameObject.SetActive(true);
        SetAlpha(highTitle, 0f);
        SetAlpha(wayTitle, 0f);
    }

    private void Start()
    {
        if (hasPlayedThisSession)
            return;

        hasPlayedThisSession = true;

        Vector3 upTarget = fadeUp.position + Vector3.up * openOffset;
        Vector3 downTarget = fadeDown.position + Vector3.down * openOffset;

        openTween = DOTween.Sequence()
            .AppendInterval(spinDelay)
            .Append(highTitle.DOFade(1f, titleFadeDuration).SetEase(Ease.OutQuad))
            .Join(wayTitle.DOFade(1f, titleFadeDuration).SetEase(Ease.OutQuad))
            .Append(fadeRoot.DORotate(
                Vector3.forward * 360f,
                spinDuration,
                RotateMode.FastBeyond360).SetRelative().SetEase(spinEase))
            .AppendInterval(spinDelay)
            .Append(fadeRoot.DORotate(
                Vector3.forward * 360f,
                spinDuration,
                RotateMode.FastBeyond360).SetRelative().SetEase(spinEase))
            .Append(fadeUp.DOMove(upTarget, openDuration).SetEase(openEase))
            .Join(fadeDown.DOMove(downTarget, openDuration).SetEase(openEase))
            .AppendCallback(() => fadeRoot.gameObject.SetActive(false));
    }

    private static void SetAlpha(SpriteRenderer spriteRenderer, float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    private void OnDestroy()
    {
        openTween?.Kill();
    }
}
