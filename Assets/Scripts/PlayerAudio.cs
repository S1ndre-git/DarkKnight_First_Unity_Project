using UnityEngine;

public class PlayerAudio : MonoBehaviour
{


  

   

    public AudioSource audioSource;
    public AudioClip dashSfx;

    [Header("Attack")]
    public AudioClip swingSfx;
    public AudioClip hitSfx;

    [Header("Movement")]
    public AudioClip jump1Sfx;
    public AudioClip jump2Sfx;
    public AudioClip landSfx;

      [Header("Hurt")]
    public AudioClip hurtSfx;

    [Header("Footstep")]
    public AudioClip footstepSfx;
    public float footstepInterval = 0.4f;

    private float footstepTimer = 0f;

//音量控制
    public float hurtVolume = 1f;
    public float dashVolume = 1f;
    public float swingVolume = 1f;
    public float hitVolume = 1f;
    public float jump1Volume = 1f;
    public float jump2Volume = 1f;
    public float landVolume = 1f;
    public float footstepVolume = 1f;

    

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }
    public void PlayHurtSfx()
{
    Debug.Log("PlayHurtSfx called");

    if (audioSource != null && hurtSfx != null)
    {
        audioSource.PlayOneShot(hurtSfx, 1f);
    }
    else
    {
        Debug.Log("hurt audio missing");
    }
}

    // ===== 攻击 =====
    public void PlaySwingSfx()
    {
        if (audioSource != null && swingSfx != null)
        {
            audioSource.PlayOneShot(swingSfx,swingVolume);
        }
    }

    public void PlayHitSfx()
    {
        if (audioSource != null && hitSfx != null)
        {
            audioSource.PlayOneShot(hitSfx,hitVolume);
        }
    }

    // ===== 跳跃 =====
    public void PlayJump1Sfx()
    {
        if (audioSource != null && jump1Sfx != null)
        {
            audioSource.PlayOneShot(jump1Sfx,jump1Volume);
        }
    }

    public void PlayJump2Sfx()
    {
        if (audioSource != null && jump2Sfx != null)
        {
            audioSource.PlayOneShot(jump2Sfx,jump2Volume);
        }
    }

    // ===== 落地 =====
    public void PlayLandSfx()
    {
        if (audioSource != null && landSfx != null)
        {
            audioSource.PlayOneShot(landSfx,landVolume);
        }
    }

    //走路
    public void TryPlayFootstep(bool isMoving)
{
    if (audioSource == null || footstepSfx == null) return;

    if (isMoving)
    {
        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            audioSource.PlayOneShot(footstepSfx, footstepVolume);
            footstepTimer = footstepInterval;
        }
    }
    else
    {
        footstepTimer = 0f;
    }
}

    public void PlayDashSfx()
{
    if (audioSource != null && dashSfx != null)
    {
        audioSource.PlayOneShot(dashSfx, dashVolume);
    }
}
}