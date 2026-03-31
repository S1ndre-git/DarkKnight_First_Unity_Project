using UnityEngine;

public class BossAudio : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Attack")]
    public AudioClip attack1Sfx;
    public AudioClip attack2Sfx;
    public AudioClip attack3Sfx;
    public AudioClip attack4Sfx;
    public AudioClip attack5Sfx;

    [Header("Volume")]
    public float attack1Volume = 1f;
    public float attack2Volume = 1f;
    public float attack3Volume = 1f;
    public float attack4Volume = 1f;
    public float attack5Volume = 1f;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void PlayAttack1Sfx()
    {
        if (audioSource != null && attack1Sfx != null)
        {
            audioSource.PlayOneShot(attack1Sfx, attack1Volume);
        }
    }

    public void PlayAttack2Sfx()
    {
        if (audioSource != null && attack2Sfx != null)
        {
            audioSource.PlayOneShot(attack2Sfx, attack2Volume);
        }
    }

    public void PlayAttack3Sfx()
    {
        if (audioSource != null && attack3Sfx != null)
        {
            audioSource.PlayOneShot(attack3Sfx, attack3Volume);
        }
    }

    public void PlayAttack4Sfx()
    {
        if (audioSource != null && attack4Sfx != null)
        {
            audioSource.PlayOneShot(attack4Sfx, attack4Volume);
        }
    }

    public void PlayAttack5Sfx()
    {
        if (audioSource != null && attack5Sfx != null)
        {
            audioSource.PlayOneShot(attack5Sfx, attack5Volume);
        }
    }
}