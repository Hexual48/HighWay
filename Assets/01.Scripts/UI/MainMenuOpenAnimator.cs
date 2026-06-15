using DG.Tweening;
using UnityEngine;

public class MainMenuOpenAnimator : MonoBehaviour
{
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

    [Header("Camera Zoom")]
    [SerializeField] private Camera menuCamera;
    [SerializeField] private float zoomOutSize = 30f;
    [SerializeField] private float zoomDefaultSize = 5f;
    [SerializeField] private float zoomDuration = 1.2f;
    [SerializeField, Min(0.1f)] private float zoomReturnSpeedMultiplier = 2f;

    private Tween openTween;

    private void Awake()
    {
        fadeRoot.gameObject.SetActive(true);
        SetAlpha(highTitle, 0f);
        SetAlpha(wayTitle, 0f);
    }

    private void Start()
    {
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
        /*.Append(menuCamera.DOOrthoSize(zoomOutSize, zoomDuration).SetEase(Ease.InOutBack))
        .Append(menuCamera
            .DOOrthoSize(7f, zoomDuration * 0.7f / zoomReturnSpeedMultiplier)
            .SetEase(Ease.InOutBack))
        .Append(menuCamera
            .DOOrthoSize(zoomDefaultSize, zoomDuration * 0.3f / zoomReturnSpeedMultiplier)
            .SetEase(Ease.OutBounce));*/
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
