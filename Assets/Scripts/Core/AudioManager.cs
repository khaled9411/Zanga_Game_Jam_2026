using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Continuous Sounds")]
    public AudioSource ambianceSource;
    public AudioSource heartbeatSource;

    [Header("Ambiance Event")]
    public SoundEvent ambianceEvent;

    [Header("SFX Pool Settings")]
    public int sfxPoolSize = 10;
    private AudioSource[] sfxSources;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        InitializeSFXPool();
    }

    private void Start()
    {
        ambianceSource.loop = true;
        ambianceEvent.Play(ambianceSource);

        heartbeatSource.loop = true;
        heartbeatSource.volume = 0f;
        heartbeatSource.Play();
    }

    private void InitializeSFXPool()
    {
        sfxSources = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject go = new GameObject("SFX_Source_" + i);
            go.transform.SetParent(this.transform);
            sfxSources[i] = go.AddComponent<AudioSource>();
            sfxSources[i].spatialBlend = 0f;
        }
    }

    public void PlaySound2D(SoundEvent soundEvent)
    {
        if (soundEvent == null) return;

        foreach (var source in sfxSources)
        {
            if (!source.isPlaying)
            {
                soundEvent.Play(source);
                return;
            }
        }
    }

    public void SetLowHealthState(bool isLowHealth)
    {
        float targetVolume = isLowHealth ? 1f : 0f;
        heartbeatSource.DOFade(targetVolume, 1f).SetEase(Ease.InOutSine);
    }
}