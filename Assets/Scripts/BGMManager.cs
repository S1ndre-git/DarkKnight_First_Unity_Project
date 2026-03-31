using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (audioSource != null && !audioSource.isPlaying)
            audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = volume;
    }
}