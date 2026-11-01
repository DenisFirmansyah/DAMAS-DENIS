using UnityEngine;

[CreateAssetMenu(fileName = "NewSfxCue", menuName = "DAMAS DENIS/Audio/SFX Cue")]
public sealed class SfxCue : ScriptableObject
{
    [Tooltip("One is picked at random each play. Variants stop repeats sounding robotic.")]
    [SerializeField] private AudioClip[] clips;
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;
    [Tooltip("Random pitch shift of +/- this amount.")]
    [Range(0f, 0.3f)] [SerializeField] private float pitchVariation = 0.05f;
    [Tooltip("Most copies of THIS cue that may sound at once.")]
    [Min(1)] [SerializeField] private int maxVoices = 3;
    [Tooltip("Seconds before this cue may play again. Merges same-moment duplicates.")]
    [Min(0f)] [SerializeField] private float minInterval = 0.05f;

    public float Volume => volume;
    public float PitchVariation => pitchVariation;
    public int MaxVoices => maxVoices;
    public float MinInterval => minInterval;

    /// <summary>A random clip, or null if none are assigned.</summary>
    public AudioClip PickClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}