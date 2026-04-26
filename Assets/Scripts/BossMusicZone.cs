using UnityEngine;

public class BossMusicZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.EnterBossZone();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.ExitBossZone();
        }
    }
}