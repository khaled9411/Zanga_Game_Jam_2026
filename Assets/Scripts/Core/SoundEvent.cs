using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundEvent", menuName = "Audio/Sound Event")]
public class SoundEvent : ScriptableObject
{
    [Header("Audio Clips")]
    public AudioClip[] clips;

    [Header("Settings")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 2f)] public float pitch = 1f;

    [Header("Randomness")]
    [Range(0f, 0.5f)] public float volumeRandomness = 0.1f;
    [Range(0f, 0.5f)] public float pitchRandomness = 0.1f;

    public void Play(AudioSource source)
    {
        if (clips.Length == 0) return;

        source.clip = clips[Random.Range(0, clips.Length)];

        source.volume = volume + Random.Range(-volumeRandomness, volumeRandomness);
        source.pitch = pitch + Random.Range(-pitchRandomness, pitchRandomness);

        source.Play();
    }
}