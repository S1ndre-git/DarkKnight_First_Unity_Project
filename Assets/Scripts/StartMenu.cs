using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public GameObject overlay;

    private bool isGameStarted = false;

    private void Start()
    {
        Time.timeScale = 0f;
        overlay.SetActive(true);
    }

    public void OnStartButton()
{
    overlay.SetActive(false);
    Time.timeScale = 1f;
    isGameStarted = true;

    if (BGMManager.Instance != null)
    {
        BGMManager.Instance.StartGameBGM();
    }
}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGameStarted)
            {
                // 游戏中 → 回到开始界面
                overlay.SetActive(true);
                Time.timeScale = 0f;
                isGameStarted = false;
            }
            else
            {
                // 开始界面 → 退出游戏
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}