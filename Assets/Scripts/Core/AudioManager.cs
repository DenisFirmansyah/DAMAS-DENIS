using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Main Menu Music")]
    [SerializeField] private AudioClip mainMenuMusic;

    [Header("Match Music")]
    [SerializeField] private AudioClip matchMusic;

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

    private void Start()
    {
        // ApplySettings();
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(mainMenuMusic);
    }

    public void PlayMatchMusic()
    {
        PlayMusic(matchMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void SetMasterVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }

    public void SetMusicVolume(float value)
    {
        musicSource.volume = Mathf.Clamp01(value);
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = Mathf.Clamp01(value);
    }

    // public void ApplySettings()
    // {
    //     if (GameSettings.Instance == null)
    //         return;

    //     SetMasterVolume(GameSettings.Instance.MasterVolume);
    //     SetMusicVolume(GameSettings.Instance.MusicVolume);
    //     SetSFXVolume(GameSettings.Instance.SFXVolume);
    // }
}