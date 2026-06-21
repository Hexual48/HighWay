using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle clickReloadToggle;

    private void Awake()
    {
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        clickReloadToggle.onValueChanged.AddListener(SetClickReload);
    }

    private void OnEnable()
    {
        masterVolumeSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
        sfxVolumeSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
        fullscreenToggle.SetIsOnWithoutNotify(GameSettings.IsFullscreen);
        clickReloadToggle.SetIsOnWithoutNotify(GameSettings.ClickReloadEnabled);
    }

    public void SetMasterVolume(float value)
    {
        GameSettings.SetMasterVolume(value);
    }

    public void SetSfxVolume(float value)
    {
        GameSettings.SetSfxVolume(value);
    }

    public void SetFullscreen(bool value)
    {
        GameSettings.SetFullscreen(value);
    }

    public void SetClickReload(bool value)
    {
        GameSettings.SetClickReload(value);
    }

    public void CloseSettings()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        sfxVolumeSlider.onValueChanged.RemoveListener(SetSfxVolume);
        fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
        clickReloadToggle.onValueChanged.RemoveListener(SetClickReload);
    }
}
