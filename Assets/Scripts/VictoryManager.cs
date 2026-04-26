using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject victoryPanel;
    public Image fadeImage;

    [Header("Timing")]
    public float showVictoryDelay = 2f;
    public float beforeFadeDelay = 3f;
    public float fadeDuration = 2f;

    private bool sequenceStarted = false;

    private void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }

    public void StartVictorySequence()
    {
        if (sequenceStarted) return;
        sequenceStarted = true;

        StartCoroutine(VictorySequenceRoutine());
    }

    private IEnumerator VictorySequenceRoutine()
    {
        // Wait for boss death animation
        yield return new WaitForSeconds(showVictoryDelay);

        // Show victory text
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // Let the player still move for a few seconds
        yield return new WaitForSeconds(beforeFadeDelay);

        // Fade to black
        if (fadeImage != null)
        {
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;

                float alpha = Mathf.Clamp01(timer / fadeDuration);

                Color c = fadeImage.color;
                c.a = alpha;
                fadeImage.color = c;

                yield return null;
            }
        }

        RestartGame();
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}