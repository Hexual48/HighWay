using UnityEngine;

public static class GameSettings
{
    private const string MasterVolumeKey = "Settings.MasterVolume";
    private const string SfxVolumeKey = "Settings.SfxVolume";
    private const string FullscreenKey = "Settings.Fullscreen";
    private const string ClickReloadKey = "Settings.ClickReload";

    public static float MasterVolume { get; private set; } = 1f;
    public static float SfxVolume { get; private set; } = 1f;
    public static bool IsFullscreen { get; private set; }
    public static bool ClickReloadEnabled { get; private set; } = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        SfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        IsFullscreen = PlayerPrefs.HasKey(FullscreenKey)
            ? PlayerPrefs.GetInt(FullscreenKey) == 1
            : Screen.fullScreen;
        ClickReloadEnabled = PlayerPrefs.GetInt(ClickReloadKey, 1) == 1;

        AudioListener.volume = MasterVolume;
        Screen.fullScreen = IsFullscreen;
    }

    public static void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        AudioListener.volume = MasterVolume;
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        PlayerPrefs.Save();
    }

    public static void SetSfxVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
        PlayerPrefs.Save();
    }

    public static void SetFullscreen(bool value)
    {
        IsFullscreen = value;
        Screen.fullScreen = value;
        PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SetClickReload(bool value)
    {
        ClickReloadEnabled = value;
        PlayerPrefs.SetInt(ClickReloadKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
