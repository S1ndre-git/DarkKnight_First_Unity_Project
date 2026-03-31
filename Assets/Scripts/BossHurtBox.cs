using UnityEngine;

public class BossHurtBox : MonoBehaviour
{
    public BossAI boss;

    public void TakeDamage(int damage)
    {
        if (boss != null)
        {
            boss.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning("BossHurtBox: boss reference is missing!");
        }
    }
}