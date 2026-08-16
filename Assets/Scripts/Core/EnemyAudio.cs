using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    private AudioSource enemySource;

    [Header("Enemy Sounds")]
    public SoundEvent aggroSound;
    public SoundEvent hitSound;
    public SoundEvent dieSound;

    private void Awake()
    {
        enemySource = GetComponent<AudioSource>();

        enemySource.spatialBlend = 1f;

        enemySource.rolloffMode = AudioRolloffMode.Linear;
        enemySource.minDistance = 2f;
        enemySource.maxDistance = 15f;
    }

    public void PlayAggro() => aggroSound?.Play(enemySource);
    public void PlayHit() => hitSound?.Play(enemySource);
    public void PlayDie() => dieSound?.Play(enemySource);
}