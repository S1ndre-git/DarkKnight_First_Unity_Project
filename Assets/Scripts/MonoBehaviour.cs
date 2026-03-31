using UnityEngine;

public class RangedEnemyAnimationRelay : MonoBehaviour
{
    private RangedEnemy rangedEnemy;

    void Start()
    {
        rangedEnemy = GetComponentInParent<RangedEnemy>();
    }

    public void FireArrow()
    {
        if (rangedEnemy != null)
        {
            rangedEnemy.FireArrow();
        }
    }
}