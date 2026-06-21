using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BulletSelectionUI : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private PlayerAmmo playerAmmo;

    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image leftArrowImage;
    [SerializeField] private Image rightArrowImage;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color pressedColor = Color.gray;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float pressFlashSeconds = 0.1f;

    private Coroutine leftFlashRoutine;
    private Coroutine rightFlashRoutine;
    private bool subscribed;

    private void Awake()
    {
        ResolveMissingReferences();

        if (playerAmmo == null)
        {
            playerAmmo = FindFirstObjectByType<PlayerAmmo>();
        }
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void Start()
    {
        if (playerAmmo == null)
        {
            playerAmmo = FindFirstObjectByType<PlayerAmmo>();
        }

        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        if (playerAmmo != null && subscribed)
        {
            playerAmmo.AmmoChanged -= Refresh;
            playerAmmo.AmmoSwitched -= HandleAmmoSwitched;
            subscribed = false;
        }
    }

    private void Refresh()
    {
        if (playerAmmo == null)
        {
            return;
        }

        AmmoData currentAmmo = playerAmmo.CurrentAmmo;
        Sprite icon = currentAmmo != null && currentAmmo.ammoIcon != null
            ? currentAmmo.ammoIcon
            : currentAmmo != null ? currentAmmo.unlockIcon : null;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
            iconImage.preserveAspect = true;
        }

        SetArrowAvailable(leftArrowImage, playerAmmo.CanSelectPreviousAmmo());
        SetArrowAvailable(rightArrowImage, playerAmmo.CanSelectNextAmmo());
    }

    private void SetArrowAvailable(Image arrowImage, bool available)
    {
        if (arrowImage == null)
        {
            return;
        }

        arrowImage.gameObject.SetActive(available);
        arrowImage.color = normalColor;
    }

    private void HandleAmmoSwitched(int direction)
    {
        if (direction < 0)
        {
            FlashArrow(leftArrowImage, ref leftFlashRoutine);
            return;
        }

        if (direction > 0)
        {
            FlashArrow(rightArrowImage, ref rightFlashRoutine);
        }
    }

    private void FlashArrow(Image arrowImage, ref Coroutine flashRoutine)
    {
        if (arrowImage == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashArrowRoutine(arrowImage));
    }

    private IEnumerator FlashArrowRoutine(Image arrowImage)
    {
        arrowImage.gameObject.SetActive(true);
        arrowImage.color = pressedColor;
        yield return new WaitForSecondsRealtime(pressFlashSeconds);
        arrowImage.color = normalColor;
        Refresh();
    }

    private void ResolveMissingReferences()
    {
        if (iconImage == null)
        {
            iconImage = FindChildImage("Icon");
        }

        if (leftArrowImage == null)
        {
            leftArrowImage = FindChildImage("LeftArrow");
        }

        if (rightArrowImage == null)
        {
            rightArrowImage = FindChildImage("RightArrow");
        }
    }

    private Image FindChildImage(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<Image>() : null;
    }

    private void Subscribe()
    {
        if (playerAmmo == null || subscribed)
        {
            return;
        }

        playerAmmo.AmmoChanged += Refresh;
        playerAmmo.AmmoSwitched += HandleAmmoSwitched;
        subscribed = true;
    }
}
