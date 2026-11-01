using UnityEngine;
using System.Collections.Generic;

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
    [Header("SFX Pool")]
    [SerializeField, Min(1)] private int poolSize = 12;

    private AudioSource[] pool;
    private SfxCue[] poolCue;
    private readonly Dictionary<SfxCue, float> lastPlayTime = new Dictionary<SfxCue, float>();
    private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        pool = new AudioSource[poolSize];
        poolCue = new SfxCue[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            GameObject voice = new GameObject($"SFX Voice {i}");
            voice.transform.SetParent(transform, false);
            AudioSource source = voice.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            pool[i] = source;
        }
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

    /// <summary>Plays a cue through the pool, respecting its voice limit and minimum interval.</summary>
    public void PlaySFX(SfxCue cue)
    {
        if (cue == null || pool == null) return;

        AudioClip clip = cue.PickClip();
        if (clip == null) return;

        // unscaledTime, NOT Time.time: Time.time freezes at timeScale 0,
        // which would mute pause-menu button clicks after the first one.
        float now = Time.unscaledTime;
        if (lastPlayTime.TryGetValue(cue, out float last) && now - last < cue.MinInterval)
            return;

        int sameCue = 0;
        int free = -1;
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i].isPlaying)
            {
                if (poolCue[i] == cue) sameCue++;
            }
            else if (free < 0)
            {
                free = i;
            }
        }

        if (sameCue >= cue.MaxVoices || free < 0) return; // too many of this cue, or pool full

        AudioSource source = pool[free];
        source.clip = clip;
        source.pitch = 1f + Random.Range(-cue.PitchVariation, cue.PitchVariation);
        source.volume = cue.Volume * sfxVolume;
        source.Play();

        poolCue[free] = cue;
        lastPlayTime[cue] = now;
    }

    public void SetMasterVolume(float value) => AudioListener.volume = Mathf.Clamp01(value);

    public void SetMusicVolume(float value)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(value);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
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
