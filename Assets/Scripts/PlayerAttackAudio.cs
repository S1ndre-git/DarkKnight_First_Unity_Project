// using UnityEngine;

// public class PlayerAttackAudio : MonoBehaviour
// {
//     public AudioSource audioSource;
//     public AudioClip swingSfx;
//     public AudioClip hitSfx;

//     private void Awake()
//     {
//         if (audioSource == null)
//         {
//             audioSource = GetComponent<AudioSource>();
//         }
//     }

//     public void PlaySwingSfx()
//     {
//         if (audioSource != null && swingSfx != null)
//         {
//             audioSource.PlayOneShot(swingSfx);
//         }
//     }

//     public void PlayHitSfx()
//     {
//         if (audioSource != null && hitSfx != null)
//         {
//             audioSource.PlayOneShot(hitSfx);
//         }
//     }
// }