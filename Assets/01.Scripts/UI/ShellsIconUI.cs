using UnityEngine;
using UnityEngine.UI;

public class ShellsIconUI : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private PlayerAmmo playerAmmo;
    [SerializeField] private ShellsInfoUI shellsInfoUI;

    [Header("Shotshells")]
    [SerializeField] private AmmoData shotshellAmmo;
    [SerializeField] private Button shotshellButton;

    [Header("Blank")]
    [SerializeField] private AmmoData blankAmmo;
    [SerializeField] private Button blankButton;

    [Header("Flechette")]
    [SerializeField] private AmmoData flechetteAmmo;
    [SerializeField] private Button flechetteButton;

    [Header("Slug")]
    [SerializeField] private AmmoData slugAmmo;
    [SerializeField] private Button slugButton;

    private bool subscribed;

    private void Awake()
    {
        if (playerAmmo == null)
        {
            playerAmmo = FindFirstObjectByType<PlayerAmmo>();
        }
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshIcons();
    }

    private void OnDisable()
    {
        if (playerAmmo == null || !subscribed)
        {
            return;
        }

        playerAmmo.AmmoChanged -= RefreshIcons;
        subscribed = false;
    }

    private void Subscribe()
    {
        if (playerAmmo == null)
        {
            playerAmmo = FindFirstObjectByType<PlayerAmmo>();
        }

        if (playerAmmo == null || subscribed)
        {
            return;
        }

        playerAmmo.AmmoChanged += RefreshIcons;
        subscribed = true;
    }

    private void RefreshIcons()
    {
        if (playerAmmo == null)
        {
            return;
        }

        ApplyUnlockedIcon(shotshellAmmo, shotshellButton);
        ApplyUnlockedIcon(blankAmmo, blankButton);
        ApplyUnlockedIcon(flechetteAmmo, flechetteButton);
        ApplyUnlockedIcon(slugAmmo, slugButton);
    }

    public void ShowShotshellInfo()
    {
        shellsInfoUI.ShowAmmoInfo(shotshellAmmo);
    }

    public void ShowBlankInfo()
    {
        shellsInfoUI.ShowAmmoInfo(blankAmmo);
    }

    public void ShowFlechetteInfo()
    {
        shellsInfoUI.ShowAmmoInfo(flechetteAmmo);
    }

    public void ShowSlugInfo()
    {
        shellsInfoUI.ShowAmmoInfo(slugAmmo);
    }

    private void ApplyUnlockedIcon(AmmoData ammoData, Button slotButton)
    {
        if (ammoData == null || slotButton == null || !playerAmmo.IsAmmoUnlocked(ammoData))
        {
            return;
        }

        Transform iconTransform = slotButton.transform.Find("Icon");
        Image iconImage = iconTransform != null ? iconTransform.GetComponent<Image>() : null;

        Sprite icon = ammoData.ammoInfoIcon;

        if (icon == null || iconImage == null)
        {
            return;
        }

        iconImage.sprite = icon;
        iconImage.preserveAspect = true;
    }
}
