using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SpriteMenuButton : MonoBehaviour
{
    private const float HoverScale = 1.1f;
    private const float HoverDuration = 0.15f;
    private const float PressedScaleX = 1.12f;
    private const float PressedScaleY = 0.65f;
    private const float PressDuration = 0.55f;

    [SerializeField] private MainMenuController menuController;

    private Tween feedbackTween;
    private Vector3 originalScale;
    private bool clicked;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnMouseEnter()
    {
        if (clicked)
            return;

        feedbackTween?.Kill();
        feedbackTween = transform
            .DOScale(originalScale * HoverScale, HoverDuration)
            .SetEase(Ease.OutBack);
    }

    private void OnMouseExit()
    {
        if (clicked)
            return;

        feedbackTween?.Kill();
        feedbackTween = transform
            .DOScale(originalScale, HoverDuration)
            .SetEase(Ease.OutQuad);
    }

    private void OnMouseDown()
    {
        if (clicked)
            return;

        clicked = true;
        GetComponent<Collider2D>().enabled = false;
        feedbackTween?.Kill();

        Vector3 pressedScale = new(
            originalScale.x * PressedScaleX,
            originalScale.y * PressedScaleY,
            originalScale.z);

        feedbackTween = DOTween.Sequence()
            .Append(transform.DOScale(pressedScale, PressDuration * 0.2f).SetEase(Ease.OutQuad))
            .Append(transform.DOScale(originalScale, PressDuration * 0.8f).SetEase(Ease.OutBounce))
            .OnComplete(menuController.PlayGame);
    }

    private void OnDestroy()
    {
        feedbackTween?.Kill();
    }
}
