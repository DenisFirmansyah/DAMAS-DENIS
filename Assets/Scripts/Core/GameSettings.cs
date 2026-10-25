using UnityEngine;

public sealed class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SFXVolume";

    public float MasterVolume { get; private set; } = 1f;
    public float MusicVolume { get; private set; } = 1f;
    public float SFXVolume { get; private set; } = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();

        // AudioManager may already exist or may initialize later and apply settings in Start.
        if (AudioManager.Instance != null)
            AudioManager.Instance.ApplySettings();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        SaveFloat(MasterVolumeKey, MasterVolume);
        AudioManager.Instance?.SetMasterVolume(MasterVolume);
    }

    public void SetMusicVolume(float value)
    {
        MusicVolume = Mathf.Clamp01(value);
        SaveFloat(MusicVolumeKey, MusicVolume);
        AudioManager.Instance?.SetMusicVolume(MusicVolume);
    }

    public void SetSFXVolume(float value)
    {
        SFXVolume = Mathf.Clamp01(value);
        SaveFloat(SfxVolumeKey, SFXVolume);
        AudioManager.Instance?.SetSFXVolume(SFXVolume);
    }

    private void LoadSettings()
    {
        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
        SFXVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
    }

    private static void SaveFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }
}
