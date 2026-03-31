using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    private EnemyAttack enemyAttack;

    void Start()
    {
        enemyAttack = GetComponentInParent<EnemyAttack>();
    }

    public void DoAttack()
    {
        if (enemyAttack != null)
        {
            enemyAttack.DoAttack();
        }
    }
}