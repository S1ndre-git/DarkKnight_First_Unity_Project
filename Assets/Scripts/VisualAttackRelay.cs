using UnityEngine;

public class VisualAttackRelay : MonoBehaviour
{
    public PlayerAttack playerAttack;

    public void Attack()
    {
        if (playerAttack != null)
        {
            playerAttack.Attack();
        }
    }
}