using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip roamingMusic;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => ApplySettings();

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void PlayMainMenuMusic() => PlayMusic(mainMenuMusic);
    public void PlayGameplayMusic() => PlayMusic(gameplayMusic);
    public void PlayRoamingMusic() => PlayMusic(roamingMusic);

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void SetMasterVolume(float value) => AudioListener.volume = Mathf.Clamp01(value);

    public void SetMusicVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(value);
    }

    public void SetSFXVolume(float value)
    {
        if (sfxSource != null)
            sfxSource.volume = Mathf.Clamp01(value);
    }

    public void ApplySettings()
    {
        GameSettings settings = GameSettings.Instance;
        if (settings == null)
            return;

        SetMasterVolume(settings.MasterVolume);
        SetMusicVolume(settings.MusicVolume);
        SetSFXVolume(settings.SFXVolume);
    }
}
