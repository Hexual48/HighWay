using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class ShellsInfoUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shellsUI;
    [SerializeField] private PlayerAmmo playerAmmo;

    [Header("Texts")]
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statText;

    [Header("Icon")]
    [SerializeField] private Image iconImage;

    private void Awake()
    {
        if (playerAmmo == null)
        {
            playerAmmo = FindFirstObjectByType<PlayerAmmo>();
        }
    }

    public void ShowAmmoInfo(AmmoData ammoData)
    {
        if (ammoData == null || playerAmmo == null || !playerAmmo.IsAmmoUnlocked(ammoData))
        {
            return;
        }

        descriptionText.text = ammoData.unlockDescription;
        nameText.text = GetTitleWithoutUnlocked(ammoData.unlockTitle);
        statText.text = string.Join("\n\n",
            ammoData.damagePerPellet,
            ammoData.spreadAngle,
            ammoData.recoilForce,
            ammoData.pelletCount,
            ammoData.penetration);

        if (iconImage != null)
        {
            iconImage.sprite = ammoData.ammoInfoIcon;
            iconImage.enabled = iconImage.sprite != null;
            iconImage.preserveAspect = true;
        }
    }

    public void CloseShellsUI()
    {
        shellsUI.SetActive(false);
    }

    private static string GetTitleWithoutUnlocked(string unlockTitle)
    {
        if (string.IsNullOrWhiteSpace(unlockTitle))
        {
            return string.Empty;
        }

        string title = unlockTitle.Replace("Unlocked!", string.Empty);
        title = Regex.Replace(title, @"\s*\bRound\b", string.Empty, RegexOptions.IgnoreCase);
        return title.TrimEnd();
    }
}
