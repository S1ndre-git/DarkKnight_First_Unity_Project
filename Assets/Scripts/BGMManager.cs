using System.Collections;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("Audio Sources")]
    public AudioSource normalSource;
    public AudioSource bossSource;

    [Header("Volume")]
    [Range(0f, 1f)] public float normalVolume = 0.5f;
    [Range(0f, 1f)] public float bossVolume = 0.6f;

    [Header("Fade")]
    public float fadeDuration = 2f;

    private Coroutine fadeCoroutine;
    private bool bossDefeated = false;

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

    private bool bgmStarted = false;

private void Start()
{
    if (normalSource != null)
    {
        normalSource.loop = true;
        normalSource.volume = 0f;
    }

    if (bossSource != null)
    {
        bossSource.loop = true;
        bossSource.volume = 0f;
    }
}

public void StartGameBGM()
{
    if (bgmStarted) return;
    bgmStarted = true;

    if (normalSource != null)
    {
        normalSource.volume = normalVolume;

        if (!normalSource.isPlaying)
            normalSource.Play();
    }

    if (bossSource != null)
    {
        bossSource.volume = 0f;

        if (!bossSource.isPlaying)
            bossSource.Play();
    }
}

    public void EnterBossZone()
    {
        if (bossDefeated) return;

        FadeTo(0f, bossVolume);
    }

    public void ExitBossZone()
    {
        if (bossDefeated) return;

        FadeTo(normalVolume, 0f);
    }

    public void BossDefeated()
    {
        bossDefeated = true;

        FadeTo(0f, 0f);
    }

    private void FadeTo(float targetNormalVolume, float targetBossVolume)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeRoutine(targetNormalVolume, targetBossVolume));
    }

    private IEnumerator FadeRoutine(float targetNormalVolume, float targetBossVolume)
    {
        float timer = 0f;

        float startNormalVolume = normalSource != null ? normalSource.volume : 0f;
        float startBossVolume = bossSource != null ? bossSource.volume : 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / fadeDuration);

            if (normalSource != null)
            {
                normalSource.volume = Mathf.Lerp(startNormalVolume, targetNormalVolume, t);
            }

            if (bossSource != null)
            {
                bossSource.volume = Mathf.Lerp(startBossVolume, targetBossVolume, t);
            }

            yield return null;
        }

        if (normalSource != null)
        {
            normalSource.volume = targetNormalVolume;
        }

        if (bossSource != null)
        {
            bossSource.volume = targetBossVolume;
        }
    }
}