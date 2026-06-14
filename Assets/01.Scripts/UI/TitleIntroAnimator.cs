using System.Collections;
using DG.Tweening;
using UnityEngine;

public class TitleIntroAnimator : MonoBehaviour
{
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

    [Header("Click To Start Effects")]
    [SerializeField, Range(0f, 1f)] private float blinkMinAlpha = 0.35f;
    [SerializeField] private float blinkDuration = 0.75f;
    [SerializeField] private Vector2 effectInterval = new(1.5f, 3f);
    [SerializeField, Range(0f, 1f)] private float rotationChance = 0.25f;
    [SerializeField] private float rotationAngle = 10f;
    [SerializeField] private Vector2 pressedScale = new(1.12f, 0.65f);

    private Sequence introSequence;
    private Sequence blinkSequence;
    private Sequence rotationSequence;
    private Sequence clickSequence;
    private Coroutine effectCoroutine;
    private Vector3 originalClickScale;
    private Quaternion originalClickRotation;
    private bool canClick;

    private void Awake()
    {
        originalClickScale = clickToStart.transform.localScale;
        originalClickRotation = clickToStart.transform.localRotation;
        SetClickToStartAlpha(0f);
    }

    private void Start()
    {
        PlayIntro();
    }

    private void Update()
    {
        if (canClick && Input.GetMouseButtonDown(0))
        {
            PlayClickEffect();
        }
    }

    public void PlayIntro()
    {
        StopEffects();
        canClick = false;

        Vector3 highTargetPosition = highTitle.localPosition;
        Vector3 wayTargetPosition = wayTitle.localPosition;

        highTitle.localPosition = highTargetPosition + Vector3.right * startOffsetX;
        wayTitle.localPosition = wayTargetPosition + Vector3.right * startOffsetX;

        SetClickToStartAlpha(0f);

        introSequence = DOTween.Sequence();

        Tween highMove = highTitle
            .DOLocalMove(highTargetPosition, moveDuration)
            .SetEase(moveEase);

        Tween wayMove = wayTitle
            .DOLocalMove(wayTargetPosition, moveDuration)
            .SetEase(moveEase)
            .SetDelay(wayDelay);

        introSequence.Append(highMove);
        introSequence.Join(wayMove);

        introSequence.AppendInterval(clickTextDelay);
        introSequence.Append(
            clickToStart
                .DOFade(1f, clickTextFadeDuration)
                .SetEase(Ease.OutQuad));

        introSequence.AppendCallback(StartEffects);
    }

    private void StartEffects()
    {
        canClick = true;
        effectCoroutine = StartCoroutine(PlayRandomEffects());
    }

    private IEnumerator PlayRandomEffects()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(effectInterval.x, effectInterval.y));

            if (Random.value < rotationChance)
            {
                yield return PlayRotation();
            }
            else
            {
                yield return PlayBlink();
            }
        }
    }

    private IEnumerator PlayBlink()
    {
        blinkSequence = DOTween.Sequence();
        blinkSequence.Append(
            clickToStart
                .DOFade(blinkMinAlpha, blinkDuration * 0.5f)
                .SetEase(Ease.InOutSine));
        blinkSequence.Append(
            clickToStart
                .DOFade(1f, blinkDuration * 0.5f)
                .SetEase(Ease.InOutSine));

        yield return blinkSequence.WaitForCompletion();
    }

    private IEnumerator PlayRotation()
    {
        Quaternion rotatedRotation =
            originalClickRotation * Quaternion.Euler(0f, 0f, -rotationAngle);

        rotationSequence = DOTween.Sequence();
        rotationSequence.Append(
            clickToStart.transform
                .DOLocalRotateQuaternion(rotatedRotation, 0.55f)
                .SetEase(Ease.OutElastic));
        rotationSequence.Append(
            clickToStart.transform
                .DOLocalRotateQuaternion(originalClickRotation, 0.3f)
                .SetEase(Ease.OutSine));
        rotationSequence.OnComplete(() =>
        {
            clickToStart.transform.localRotation = originalClickRotation;
        });

        yield return rotationSequence.WaitForCompletion();
    }

    private void PlayClickEffect()
    {
        clickSequence?.Kill();
        clickToStart.transform.localScale = originalClickScale;

        Vector3 squashScale = new(
            originalClickScale.x * pressedScale.x,
            originalClickScale.y * pressedScale.y,
            originalClickScale.z);

        clickSequence = DOTween.Sequence();
        clickSequence.Append(
            clickToStart.transform
                .DOScale(squashScale, 0.1f)
                .SetEase(Ease.OutQuad));
        clickSequence.Append(
            clickToStart.transform
                .DOScale(originalClickScale, 0.45f)
                .SetEase(Ease.OutBounce));
    }

    private void SetClickToStartAlpha(float alpha)
    {
        Color color = clickToStart.color;
        color.a = alpha;
        clickToStart.color = color;
    }

    private void StopEffects()
    {
        canClick = false;

        introSequence?.Kill();
        blinkSequence?.Kill();
        rotationSequence?.Kill();
        clickSequence?.Kill();

        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }

        SetClickToStartAlpha(1f);
        clickToStart.transform.localScale = originalClickScale;
        clickToStart.transform.localRotation = originalClickRotation;
    }

    private void OnDestroy()
    {
        StopEffects();
    }
}
